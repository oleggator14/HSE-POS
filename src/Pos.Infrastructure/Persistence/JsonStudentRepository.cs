using Pos.Domain.Ports;
using Pos.Domain.Students;
using Pos.Infrastructure.Persistence.Mapping;
using Pos.Infrastructure.Persistence.Models;
using System.Text.Json;

namespace Pos.Infrastructure.Persistence;

// Адаптер порта IStudentRepository поверх JSON-файла
// Файл читается один раз при создании, дальше работа идёт с кешем в памяти
// Save перезаписывает файл целиком: JSON по-другому не умеет

internal sealed class JsonStudentRepository : IStudentRepository
{
    private readonly string _path;

    // Зеркало файла: храним записи, а не доменные объекты,
    // чтобы изменение Student снаружи не попадало в кеш в обход Save
    private readonly Dictionary<Guid, StudentRecord> _cache;

    public JsonStudentRepository(string path)
    {
        _path = path;
        _cache = Load();
    }

    // Добавить или заменить: словарь сам решает по ключу
    public void Save(Student student)
    {
        _cache[student.Id.Value] = StudentMapper.ToRecord(student);
        Flush();
    }

    // Каждый вызов отдаёт новый объект, собранный из записи
    public Student? GetById(StudentId id) =>
        _cache.TryGetValue(id.Value, out var record) ? StudentMapper.ToDomain(record) : null;

    public IReadOnlyList<Student> GetAll() => [.. _cache.Values.Select(StudentMapper.ToDomain)];

    private Dictionary<Guid, StudentRecord> Load()
    {
        if (!File.Exists(_path))
            return [];  // Нет файла — первый запуск, начинаем с пустого набора

        var json = File.ReadAllText(_path);
        var records = JsonSerializer.Deserialize<List<StudentRecord>>(json, JsonOptions.Default) ?? [];
        return records.ToDictionary(r => r.Id);
    }

    // В файле список, а не словарь - для удобства чтения
    private void Flush()
    {
        var dir = Path.GetDirectoryName(_path);
        if (!string.IsNullOrEmpty(dir))
            Directory.CreateDirectory(dir);

        var json = JsonSerializer.Serialize(_cache.Values.ToList(), JsonOptions.Default);
        File.WriteAllText(_path, json);
    }
}

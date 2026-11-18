using Pos.Domain.Grading;
using Pos.Domain.Ports;
using Pos.Domain.Students;
using Pos.Infrastructure.Persistence.Mapping;
using Pos.Infrastructure.Persistence.Models;
using System.Text.Json;

namespace Pos.Infrastructure.Persistence;

// Адаптер порта IGradebookRepository поверх JSON-файла
// Ведомость одна на студента, поэтому ключ кеша — StudentId
// Оценки хранятся внутри ведомости: отдельного репозитория у них нет

internal sealed class JsonGradebookRepository : IGradebookRepository
{
    private readonly string _path;

    // Зеркало файла - храним записи о всех студентах
    private readonly Dictionary<Guid, GradebookRecord> _cache;

    public JsonGradebookRepository(string path)
    {
        _path = path;
        _cache = Load();
    }

    // null — студенту ещё не выставляли оценок
    public Gradebook? GetFor(StudentId studentId) =>
        _cache.TryGetValue(studentId.Value, out var record) ? GradebookMapper.ToDomain(record) : null;

    // Добавить или заменить - ведомость сохраняется целиком вместе с оценками
    public void Save(Gradebook gradebook)
    {
        _cache[gradebook.StudentId.Value] = GradebookMapper.ToRecord(gradebook);
        Flush();
    }

    public IReadOnlyList<Gradebook> GetAll() => [.. _cache.Values.Select(GradebookMapper.ToDomain)];

    // Нет файла — первый запуск, оценок ещё нет
    private Dictionary<Guid, GradebookRecord> Load()
    {
        if (!File.Exists(_path))
            return [];

        var json = File.ReadAllText(_path);
        var records = JsonSerializer.Deserialize<List<GradebookRecord>>(json, JsonOptions.Default) ?? [];
        return records.ToDictionary(r => r.StudentId);
    }

    private void Flush()
    {
        var dir = Path.GetDirectoryName(_path);
        if (!string.IsNullOrEmpty(dir))
            Directory.CreateDirectory(dir);

        var json = JsonSerializer.Serialize(_cache.Values.ToList(), JsonOptions.Default);
        File.WriteAllText(_path, json);
    }
}

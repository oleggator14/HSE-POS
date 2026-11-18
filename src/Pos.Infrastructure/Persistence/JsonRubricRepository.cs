using Pos.Domain.Ports;
using Pos.Domain.Rubrics;
using Pos.Infrastructure.Persistence.Mapping;
using Pos.Infrastructure.Persistence.Models;
using System.Text.Json;

namespace Pos.Infrastructure.Persistence;

// Адаптер порта IRubricRepository поверх JSON-файла
// Рубрикатор в системе один, поэтому кеш — одна запись, а не словарь

internal sealed class JsonRubricRepository : IRubricRepository
{
    private readonly string _path;
    private RubricRecord? _cache;  // при Save нужен целиком новый объект, поэтому без readonly

    public JsonRubricRepository(string path)
    {
        _path = path;
        _cache = Load();
    }

    public void Save(Rubric rubric)
    {
        _cache = RubricMapper.ToRecord(rubric);
        Flush();
    }

    public Rubric? Get() => _cache is null ? null : RubricMapper.ToDomain(_cache);

    private RubricRecord? Load()
    {
        if (!File.Exists(_path))
            return null;  // Нет файла - рубрикатор ещё не настроен

        var json = File.ReadAllText(_path);
        return JsonSerializer.Deserialize<RubricRecord>(json, JsonOptions.Default);
    }

    private void Flush()
    {
        var dir = Path.GetDirectoryName(_path);
        if (!string.IsNullOrEmpty(dir))
            Directory.CreateDirectory(dir);

        var json = JsonSerializer.Serialize(_cache, JsonOptions.Default);
        File.WriteAllText(_path, json);
    }
}

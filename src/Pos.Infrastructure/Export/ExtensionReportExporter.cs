using Pos.Domain.Grading;
using Pos.Domain.Ports;
using Pos.Domain.Rubrics;

namespace Pos.Infrastructure.Export;

// Выбирает экспортёр по расширению файла
// Фасад видит один IReportExporter и не знает, что форматов несколько

internal sealed class ExtensionReportExporter : IReportExporter
{
    // Регистр расширения не важен
    private readonly Dictionary<string, IReportExporter> _byExtension;

    // Хардкодим два формата, но можно расширять через DI: добавить ещё один IReportExporter и зарегистрировать его с нужным расширением
    // Фабрику экспортёров можно вынести в отдельный класс, если их станет много, но пока проще прямо в конструкторе
    public ExtensionReportExporter(CsvReportExporter csv, JsonReportExporter json)
    {
        _byExtension = new Dictionary<string, IReportExporter>(StringComparer.OrdinalIgnoreCase)
        {
            [".csv"] = csv,
            [".json"] = json
        };
    }

    public void Export(Rubric rubric, IReadOnlyList<StudentReport> reports, string path)
    {
        var extension = Path.GetExtension(path);

        if (!_byExtension.TryGetValue(extension, out var exporter))
            throw new NotSupportedException(
                $"Unsupported format '{extension}'. Supported: {string.Join(", ", _byExtension.Keys)}");

        exporter.Export(rubric, reports, path);
    }
}

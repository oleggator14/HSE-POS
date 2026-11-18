using Pos.Domain.Grading;
using Pos.Domain.Rubrics;

namespace Pos.Domain.Ports;

// Порт экспорта итогов (ТЗ: «экспорт итога»)

public interface IReportExporter
{
    // Rubric нужен для шапки: имена и максимумы критериев в том порядке, в каком их ввели
    void Export(Rubric rubric, IReadOnlyList<StudentReport> reports, string path);
}

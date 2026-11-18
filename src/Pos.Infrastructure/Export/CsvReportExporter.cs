using Pos.Domain.Grading;
using Pos.Domain.Rubrics;
using System.Globalization;
using System.Text;

namespace Pos.Infrastructure.Export;

// Экспорт в CSV для открытия в Excel: строка на студента, столбец на критерий

internal sealed class CsvReportExporter : ReportExporter
{
    // Международный CSV: «,» между ячейками, «.» в дробях
    // Русский Excel ждёт «;» и «,» — для него поменять обе строки
    private const char Separator = ',';
    private static readonly CultureInfo Culture = CultureInfo.InvariantCulture;

    protected override string BuildContent(Rubric rubric, IReadOnlyList<StudentReport> reports)
    {
        var sb = new StringBuilder();

        // Шапка: имена критериев с максимумом, в порядке рубрикатора
        var header = new List<string> { "Студент" };
        header.AddRange(rubric.Criteria.Select(c => $"{c.Name} (из {c.MaxScore})"));
        header.Add("Итог (10)");
        header.Add("Итог (целое)");
        sb.AppendLine(JoinRow(header));

        // Строка на студента
        foreach (var report in reports)
        {
            var row = new List<string> { report.Student.ToString() };  // В список строк (ряд) добавили имя + фамилию студента 

            // AddRange - в конец списка добавляем; rubric.Criteria.Select - проход по критериям рубрикатора в порядке добавления и применение лямбда функции
            // report.Evaluations.TryGetValue - проверка, есть ли оценка по данному критерию у студента; если есть, то берём её значение, если нет - пустая строка
            row.AddRange(rubric.Criteria.Select(c =>
                report.Evaluations.TryGetValue(c.Id, out var e) ? e.Score.Value.ToString(Culture) : ""));
            row.Add(report.FinalScore.TenScale.ToString(Culture));
            row.Add(report.FinalScore.RoundedTenScale.ToString(Culture));

            sb.AppendLine(JoinRow(row));
        }

        return sb.ToString();
    }

    // Без BOM Excel откроет кириллицу неправильно
    protected override Encoding FileEncoding => new UTF8Encoding(encoderShouldEmitUTF8Identifier: true);

    private static string JoinRow(IEnumerable<string> cells) =>
        string.Join(Separator, cells.Select(Escape));

    // Ячейку с разделителем, кавычкой или переводом строки берём в кавычки, внутренние кавычки удваиваем
    private static string Escape(string cell) =>
        cell.IndexOfAny([Separator, '"', '\n', '\r']) >= 0
            ? $"\"{cell.Replace("\"", "\"\"")}\""
            : cell;
}

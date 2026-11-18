using Pos.Domain.Grading;
using Pos.Domain.Ports;
using Pos.Domain.Rubrics;
using System.Text;

namespace Pos.Infrastructure.Export;

// Template Method (GoF, поведенческий): общий каркас экспорта с одним меняющимся шагом
// Порядок «собрать содержимое -> подготовить папку -> записать» фиксирован здесь,
// наследники задают только формат содержимого

internal abstract class ReportExporter : IReportExporter
{
    // Шаблонный метод: не virtual, наследник не может изменить порядок шагов
    public void Export(Rubric rubric, IReadOnlyList<StudentReport> reports, string path)
    {
        var content = BuildContent(rubric, reports);

        var dir = Path.GetDirectoryName(path);
        if (!string.IsNullOrEmpty(dir))
            Directory.CreateDirectory(dir);

        File.WriteAllText(path, content, FileEncoding);
    }

    // Обязательно меняющийся шаг для каждого формата
    protected abstract string BuildContent(Rubric rubric, IReadOnlyList<StudentReport> reports);

    // Необязательный шаг с поведением по умолчанию, формат может переопределить
    protected virtual Encoding FileEncoding => new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);
}

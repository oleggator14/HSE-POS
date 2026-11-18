using Pos.Domain.Grading;
using Pos.Domain.Rubrics;
using Pos.Infrastructure.Persistence;
using System.Text.Json;

namespace Pos.Infrastructure.Export;

// Экспорт в JSON для обработки другими программами
internal sealed class JsonReportExporter : ReportExporter
{
    protected override string BuildContent(Rubric rubric, IReadOnlyList<StudentReport> reports)
    {
        var data = reports.Select(r => new
        {
            Student = r.Student.ToString(),
            Criteria = rubric.Criteria.Select(c => new
            {
                c.Name,
                c.MaxScore,
                Score = r.Evaluations.TryGetValue(c.Id, out var e) ? e.Score.Value : (decimal?)null
            }),
            FinalScore = r.FinalScore.TenScale,
            FinalScoreRounded = r.FinalScore.RoundedTenScale
        });

        return JsonSerializer.Serialize(data, JsonOptions.Default);
    }
}

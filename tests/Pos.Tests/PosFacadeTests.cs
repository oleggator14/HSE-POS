using Pos.Application;
using Pos.Domain;
using Pos.Domain.Services;
using Pos.Domain.Students;
using Pos.Tests.Fakes;

namespace Pos.Tests;

// Сценарии фасада на подменённых репозиториях: тот же код, что вызывает меню, но без файлов

public sealed class PosFacadeTests
{
    private readonly RecordingReportExporter _exporter = new();
    private readonly PosFacade _facade;

    public PosFacadeTests()
    {
        _facade = new PosFacade(
            new InMemoryStudentRepository(),
            new InMemoryRubricRepository(),
            new InMemoryGradebookRepository(),
            new FinalScoreCalculator(),
            _exporter);

        _facade.SetupRubric([new("Домашки", 60, 10), new("Семинары", 40, 5)]);
    }

    [Fact]
    public void GiveEvaluation_WhenStudentUnknown_Throws()
    {
        var criterion = _facade.GetRubric()!.Criteria.First().Id;

        Assert.Throws<DomainException>(() => _facade.GiveEvaluation(StudentId.New(), criterion, 5));
    }

    [Fact]
    public void ExportReports_WhenStudentNotFullyGraded_ThrowsAndExportsNothing()
    {
        var student = _facade.AddStudent("Олег", "Иванов");
        _facade.GiveEvaluation(student, _facade.GetRubric()!.Criteria.First().Id, 8);

        Assert.Throws<DomainException>(() => _facade.ExportReports("report.csv"));
        Assert.Null(_exporter.LastReports);
    }

    [Fact]
    public void ExportReports_PassesStudentsSortedByLastName()
    {
        var ids = _facade.GetRubric()!.Criteria.Select(c => c.Id).ToList();
        foreach (var (first, last) in new[] { ("Анна", "Петрова"), ("Иван", "Абрамов") })
        {
            var s = _facade.AddStudent(first, last);
            _facade.GiveEvaluation(s, ids[0], 8);
            _facade.GiveEvaluation(s, ids[1], 4);
        }

        _facade.ExportReports("report.csv");

        Assert.Equal(["Абрамов", "Петрова"], _exporter.LastReports!.Select(r => r.Student.LastName));
    }
}

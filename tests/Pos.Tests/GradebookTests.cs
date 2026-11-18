using Pos.Domain;
using Pos.Domain.Grading;
using Pos.Domain.Rubrics;
using Pos.Domain.Students;

namespace Pos.Tests;

// Инварианты выставления оценок: проверяют рубрикатор, критерий и ведомость

public sealed class GradebookTests
{
    private readonly Rubric _rubric = new RubricBuilder()
        .Add("Домашки", Weight.FromPercent(60), 10)
        .Add("Семинары", Weight.FromPercent(40), 5)
        .Build();

    private CriterionId Homework => _rubric.Criteria.Single(c => c.Name == "Домашки").Id;

    private readonly Gradebook _book = Gradebook.CreateEmpty(StudentId.New());

    [Fact]
    public void Grade_WhenScoreAboveMax_Throws()
    {
        Assert.Throws<DomainException>(() => _book.Grade(_rubric, Homework, 12));
    }

    [Fact]
    public void Grade_WhenScoreNegative_Throws()
    {
        Assert.Throws<DomainException>(() => _book.Grade(_rubric, Homework, -1));
    }

    [Fact]
    public void Grade_WhenCriterionNotInRubric_Throws()
    {
        Assert.Throws<DomainException>(() => _book.Grade(_rubric, CriterionId.New(), 5));
    }

    [Fact]
    public void Grade_WhenAlreadyGraded_Throws()
    {
        _book.Grade(_rubric, Homework, 8);

        Assert.Throws<DomainException>(() => _book.Grade(_rubric, Homework, 7));
    }

    [Fact]
    public void Regrade_ChangesScoreOfExistingEvaluation()
    {
        _book.Grade(_rubric, Homework, 8);

        _book.Regrade(_rubric, Homework, 7, "пересдал");

        var evaluation = _book.EvaluationsByCriterion[Homework];
        Assert.Equal(7m, evaluation.Score.Value);
        Assert.Equal("пересдал", evaluation.Comment);
    }

    [Fact]
    public void Remove_WhenNothingToRemove_Throws()
    {
        Assert.Throws<DomainException>(() => _book.Remove(Homework));
    }
}

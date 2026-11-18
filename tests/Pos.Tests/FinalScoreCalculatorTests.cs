using Pos.Domain;
using Pos.Domain.Grading;
using Pos.Domain.Rubrics;
using Pos.Domain.Services;
using Pos.Domain.Students;

namespace Pos.Tests;

// Domain Service: итог — взвешенная сумма долей от максимума

public sealed class FinalScoreCalculatorTests
{
    private readonly FinalScoreCalculator _calculator = new();

    [Fact]
    public void Calculate_ReturnsWeightedShareOfMax()
    {
        var rubric = new RubricBuilder()
            .Add("Домашки", Weight.FromPercent(60), 10)
            .Add("Семинары", Weight.FromPercent(40), 5)
            .Build();
        var ids = rubric.Criteria.Select(c => c.Id).ToList();

        var book = Gradebook.CreateEmpty(StudentId.New());
        book.Grade(rubric, ids[0], 8);   // 8/10 * 0.6 = 0.48
        book.Grade(rubric, ids[1], 4);   // 4/5  * 0.4 = 0.32

        var total = _calculator.CalculateFinalScore(book, rubric);

        Assert.Equal(0.8m, total.Value);
        Assert.Equal(8m, total.TenScale);
        Assert.Equal(8, total.RoundedTenScale);
    }

    [Fact]
    public void Calculate_RoundsTenScaleToTwoDigits()
    {
        var rubric = new RubricBuilder().Add("Проект", Weight.FromPercent(100), 3).Build();
        var book = Gradebook.CreateEmpty(StudentId.New());
        book.Grade(rubric, rubric.Criteria.Single().Id, 1);   // 1/3

        var total = _calculator.CalculateFinalScore(book, rubric);

        Assert.Equal(3.33m, total.TenScale);
        Assert.Equal(3, total.RoundedTenScale);
    }

    [Fact]
    public void Calculate_WhenNotAllCriteriaGraded_Throws()
    {
        var rubric = new RubricBuilder()
            .Add("Домашки", Weight.FromPercent(60), 10)
            .Add("Семинары", Weight.FromPercent(40), 5)
            .Build();
        var book = Gradebook.CreateEmpty(StudentId.New());
        book.Grade(rubric, rubric.Criteria.First().Id, 8);

        Assert.Throws<DomainException>(() => _calculator.CalculateFinalScore(book, rubric));
    }
}

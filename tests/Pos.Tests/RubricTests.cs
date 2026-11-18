using Pos.Domain;
using Pos.Domain.Rubrics;

namespace Pos.Tests;

// Инварианты рубрикатора: охраняются билдером

public sealed class RubricTests
{
    [Fact]
    public void Build_WhenWeightsDoNotSumToOne_Throws()
    {
        var builder = new RubricBuilder()
            .Add("Домашки", Weight.FromPercent(50), 10)
            .Add("Семинары", Weight.FromPercent(30), 5);

        Assert.Throws<DomainException>(() => builder.Build());
    }

    [Fact]
    public void Add_WhenNameDuplicatesIgnoringCase_Throws()
    {
        var builder = new RubricBuilder().Add("Домашки", Weight.FromPercent(50), 10);

        Assert.Throws<DomainException>(() => builder.Add("  домашки ", Weight.FromPercent(50), 10));
    }

    [Fact]
    public void Build_WhenNoCriteria_Throws()
    {
        Assert.Throws<DomainException>(() => new RubricBuilder().Build());
    }

    [Fact]
    public void Build_KeepsCriteriaInOrderOfAdding()
    {
        var rubric = new RubricBuilder()
            .Add("Лекции", Weight.FromPercent(20), 1)
            .Add("Семинары", Weight.FromPercent(30), 5)
            .Add("Домашки", Weight.FromPercent(50), 10)
            .Build();

        Assert.Equal(["Лекции", "Семинары", "Домашки"], rubric.Criteria.Select(c => c.Name));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-0.1)]
    [InlineData(1.5)]
    public void Weight_WhenOutsideZeroToOne_Throws(double value)
    {
        Assert.Throws<DomainException>(() => new Weight((decimal)value));
    }
}

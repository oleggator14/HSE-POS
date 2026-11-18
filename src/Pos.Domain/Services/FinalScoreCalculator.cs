using Pos.Domain.Grading;
using Pos.Domain.Rubrics;

namespace Pos.Domain.Services;

// Domain Service для расчета итоговой оценки студента по ведомости и рубрикатору

public sealed class FinalScoreCalculator
{
    // Не static для правильной упаковки в DI контейнер
    public FinalScore CalculateFinalScore(Gradebook gradebook, Rubric rubric)
    {
        decimal totalWeightedScore = 0;

        // Проверка, что итог считается только по полному набору оценок (инвариант ТЗ: итог согласован с набором)
        var missing = rubric.Criteria.Where(c => !gradebook.Has(c.Id)).Select(c => c.Name).ToList();
        if (missing.Count > 0)
            throw new DomainException($"Cannot calculate: no evaluation for {string.Join(", ", missing.Select(n => $"'{n}'"))}.");

        foreach (var evaluation in gradebook.Evaluations)
        {
            var criterion = rubric.GetCriterionById(evaluation.CriterionId);
            totalWeightedScore += evaluation.Score.Value / criterion.MaxScore * criterion.Weight.Value;
        }

        return new FinalScore(totalWeightedScore);
    }
}

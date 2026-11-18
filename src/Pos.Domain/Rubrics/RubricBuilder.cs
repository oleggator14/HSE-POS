namespace Pos.Domain.Rubrics;

// Билдер рубрикатора - порождающий паттерн GoF - единственная точка создания рубрикатора.
// Нужен потому, что инвариант «сумма весов = 1» относится к набору целиком
// и не может выполняться, пока критерии добавляются по одному.

public sealed class RubricBuilder
{
    private readonly List<Criterion> _criteria = [];

    public RubricBuilder Add(string name, Weight weight, int maxScore)
    {
        var trimmed = name.Trim();

        // Проверяем то, что проверяемо сразу - имя не должно повторяться
        if (_criteria.Any(c => c.Name.Equals(trimmed, StringComparison.OrdinalIgnoreCase)))
            throw new DomainException($"Criterion with name '{trimmed}' already exists in the rubric.");

        _criteria.Add(new Criterion(CriterionId.New(), trimmed, weight, maxScore));
        return this;
    }

    // Здесь набор обязан стать валидным
    public Rubric Build()
    {
        if (_criteria.Count == 0)
            throw new DomainException("Cannot build a rubric with no criteria.");

        var totalWeight = _criteria.Sum(c => c.Weight.Value);
        if (totalWeight != 1)
            throw new DomainException($"Total weight of all criteria must be 1, but it is {totalWeight}.");

        return new Rubric(_criteria);
    }
}

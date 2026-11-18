namespace Pos.Domain.Rubrics;

// Rubric (корень агрегата) - рубрикатор
// Полностью управляет своими критериями

public sealed class Rubric
{
    private readonly OrderedDictionary<CriterionId, Criterion> _criteria;  // Ordered для сохранения порядка добавления

    // Внутренний конструктор, чтобы создать рубрикатор только через RubricBuilder
    internal Rubric(IEnumerable<Criterion> criteria)
    {
        _criteria = [];
        foreach (var c in criteria)
            _criteria.Add(c.Id, c);
    }

    public IReadOnlyCollection<Criterion> Criteria => _criteria.Values;

    // Инвариант ТЗ - оценка только по существующим критериям
    public Criterion GetCriterionById(CriterionId id)
    {
        if (!_criteria.TryGetValue(id, out var criterion))
            throw new DomainException($"Criterion with ID '{id}' not found in the rubric.");
        return criterion;
    }

    // Восстановление из хранилища: Id приходят готовыми, инварианты проверяются заново
    public static Rubric Restore(IEnumerable<Criterion> criteria)
    {
        var list = criteria.ToList();
        var total = list.Sum(c => c.Weight.Value);
        if (total != 1)
            throw new DomainException($"Stored rubric is invalid: total weight is {total}.");
        return new Rubric(list);
    }
}

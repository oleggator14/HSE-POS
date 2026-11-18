using Pos.Domain.Rubrics;
using Pos.Domain.Students;

namespace Pos.Domain.Grading;

// Ведомость (Entity, корень агрегата Ведомость) - содержит оценки студента по критериям, определённым в рубрикаторе
// Уникальность определяется StudentId, т.к. ведомость создается для конкретного студента
// Архитектурный инвариант разработчика - не более одной оценки по критерию на студента

public sealed class Gradebook
{
    public StudentId StudentId { get; }

    // Словарь структурно гарантирует инвариант «не более одной оценки на критерий» и даёт поиск за O(1)
    private readonly Dictionary<CriterionId, Evaluation> _evaluations;

    private Gradebook(StudentId studentId, IEnumerable<Evaluation> evaluations)
    {
        StudentId = studentId;
        _evaluations = evaluations.ToDictionary(e => e.CriterionId);
    }

    // Ведомость студента, которому ещё не выставляли оценок
    public static Gradebook CreateEmpty(StudentId studentId) => new(studentId, []);

    // Восстановление из хранилища: инварианты проверяются заново, файл мог быть повреждён
    public static Gradebook Restore(StudentId studentId, IEnumerable<Evaluation> evaluations)
    {
        var list = evaluations.ToList();

        if (list.Select(e => e.CriterionId).Distinct().Count() != list.Count)
            throw new DomainException("Stored gradebook is invalid: duplicate evaluations for the same criterion.");

        return new Gradebook(studentId, list);
    }

    // Фабричный метод выставления оценки. Три инварианта, каждый проверяет тот, у кого есть данные:
    // рубрикатор — существование критерия, критерий — диапазон балла, ведомость — отсутствие дубля
    // Rubric передаётся аргументом, а не хранится: это другой агрегат, ссылки между агрегатами — по Id
    public void Grade(Rubric rubric, CriterionId criterionId, decimal raw, string? comment = null)
    {
        var criterion = rubric.GetCriterionById(criterionId);

        if (_evaluations.ContainsKey(criterionId))
            throw new DomainException($"Criterion '{criterion.Name}' is already graded.");

        _evaluations[criterionId] = new Evaluation(criterionId, criterion.ValidateScore(raw), DateTime.UtcNow, comment);
    }

    // Исправление оценки (ТЗ). Объект тот же — меняются только значения
    public void Regrade(Rubric rubric, CriterionId criterionId, decimal raw, string? comment = null)
    {
        var criterion = rubric.GetCriterionById(criterionId);

        if (!_evaluations.TryGetValue(criterionId, out var evaluation))
            throw new DomainException($"No evaluation for criterion '{criterion.Name}' yet, use Grade instead.");

        evaluation.Correct(criterion.ValidateScore(raw), DateTime.UtcNow, comment);
    }

    // Отмена оценки (ТЗ). Рубрикатор не нужен - балл не проверяется
    public void Remove(CriterionId criterionId)
    {
        if (!_evaluations.Remove(criterionId))
            throw new DomainException("No evaluation for this criterion, nothing to remove.");
    }

    // Для CLI: спросить до вызова Grade, чтобы предложить перезапись
    public bool Has(CriterionId criterionId) => _evaluations.ContainsKey(criterionId);

    public IReadOnlyCollection<Evaluation> Evaluations => _evaluations.Values;

    // Оценки с доступом по критерию: для отчётов, где столбцы идут в порядке рубрикатора
    // AsReadOnly — обёртка без копирования, изменить через неё нельзя
    public IReadOnlyDictionary<CriterionId, Evaluation> EvaluationsByCriterion => _evaluations.AsReadOnly();

    // Все ли критерии рубрикатора оценены. Нужно и для расчёта итога, и для отчётов
    public bool IsComplete(Rubric rubric) => rubric.Criteria.All(c => _evaluations.ContainsKey(c.Id));
}

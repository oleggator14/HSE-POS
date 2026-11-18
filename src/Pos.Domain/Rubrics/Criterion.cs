using Pos.Domain.Grading;

namespace Pos.Domain.Rubrics;

// Класс Criterion (Entity). Принадлежит агрегату рубрикатора

public sealed class Criterion
{
    public CriterionId Id { get; }  // ID не может быть изменен
    public string Name { get; }  // Допускаем, что имя не может быть изменено, ради спокойствия студентов
    public Weight Weight { get; }
    public int MaxScore { get; }

    // Конструктор internal — критерий не существует вне валидного рубрикатора
    internal Criterion(CriterionId id, string name, Weight weight, int maxScore)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Criterion name cannot be null or whitespace.");
        if (maxScore <= 0)
            throw new DomainException($"MaxScore must be positive, got {maxScore}");

        Id = id;
        Name = name.Trim();  // Убираем лишние пробелы в начале и конце имени
        Weight = weight;
        MaxScore = maxScore;
    }

    // Восстановление из хранилища: Id приходит готовым
    public static Criterion Restore(CriterionId id, string name, Weight weight, int maxScore)
    => new(id, name, weight, maxScore);

    // Знает свой MaxScore, поэтому проверка диапазона живёт здесь.
    public Score ValidateScore(decimal raw)
    {
        if (raw < 0 || raw > MaxScore)
            throw new DomainException($"Score {raw} is out of range [0, {MaxScore}] for criterion '{Name}'.");

        return new Score(raw);
    }
}

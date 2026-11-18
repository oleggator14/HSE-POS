namespace Pos.Domain.Grading;

// Оценка (VO)

public sealed record Score
{
    public decimal Value { get; }

    public Score(decimal value)  // конструктор с проверкой, что оценка >= 0
    {
        if (value < 0)
            throw new DomainException($"Score must be greater than or equal to 0, got {value}");

        Value = value;
    }
}

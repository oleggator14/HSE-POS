namespace Pos.Domain.Rubrics;

// Weight (VO) - вес критерия

public sealed record Weight
{
    public decimal Value { get; }

    public Weight(decimal value)  // конструктор с проверкой, что вес критерия в пределах (0; 1]
    {
        if (value <= 0 || value > 1)
            throw new DomainException($"Weight must be between 0 and 1 (0; 1], got {value}");

        Value = value;
    }

    public static Weight FromPercent(int percent) => new(percent / 100m);  // конструктор для удобства создания веса из процента (например, 25% = 0.25)

    public override string ToString() => Value.ToString();  // метод для удобного вывода в консоль и логах
}

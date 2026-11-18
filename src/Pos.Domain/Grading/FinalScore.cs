namespace Pos.Domain.Grading;

// Итоговая оценка (VO) - агрегированная оценка по всем критериям рубрикатора. Хранит долю (0..1) — то, в чём считает домен;
// производные представления (10-балльная шкала, стандарт ВШЭ) выводятся из неё, поэтому рассогласоваться не могут

public sealed record FinalScore
{
    // Сырое значение от 0 до 1, включительно. Доменная логика гарантирует, что оно в пределах [0; 1].
    public decimal Value { get; }

    public FinalScore(decimal value)
    {
        if (value < 0 || value > 1)
            throw new DomainException($"Final score must be within [0, 1], got {value}.");

        Value = value;
    }

    // Десятибалльная шкала с двумя знаками: 7.33
    public decimal TenScale => Math.Round(Value * 10, 2, MidpointRounding.AwayFromZero);

    // Десятибалльная шкала, целое число: 7
    public int RoundedTenScale => (int)Math.Round(Value * 10, 0, MidpointRounding.AwayFromZero);

    public override string ToString() => $"{TenScale} ({RoundedTenScale})";
}

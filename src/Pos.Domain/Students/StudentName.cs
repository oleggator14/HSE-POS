namespace Pos.Domain.Students;

// Name (VO) для разделения ответственности
// Используем record вместо record struct для защиты от неявного конструктора без параметров (который есть у структур)

public sealed record StudentName
{
    public string FirstName { get; }  // без init, защита от неявного конструктора
    public string LastName { get; }  // без init, защита от неявного конструктора

    public StudentName(string firstName, string lastName)
    {
        // Проверяем, что имена не пустые и не содержат только пробелы. При ошибке ввода бросаем наш DomainException
        if (string.IsNullOrWhiteSpace(firstName))
            throw new DomainException("First name cannot be null or whitespace.");
        if (string.IsNullOrWhiteSpace(lastName))
            throw new DomainException("Last name cannot be null or whitespace.");

        // .Trim() убирает пробелы в начале и конце строки
        FirstName = firstName.Trim();
        LastName = lastName.Trim();
    }

    public override string ToString() => $"{LastName} {FirstName}";
}

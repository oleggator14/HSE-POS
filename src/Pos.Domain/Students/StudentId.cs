namespace Pos.Domain.Students;

// ID Студента (VO) как обертка над Guid, чтобы не путать его с другими Guid и ловить ошибки на этапе компиляции

public readonly record struct StudentId(Guid Value)  // сахар в скобках создает публичное поле Value {get; init} и конструктор
{
    public static StudentId New() => new(Guid.NewGuid());  // фабричный метод для создания нового ID

    public override string ToString() => Value.ToString();  // метод для удобного вывода в консоль и логах
}

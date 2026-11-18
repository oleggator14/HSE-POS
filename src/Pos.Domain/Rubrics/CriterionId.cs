namespace Pos.Domain.Rubrics;

// Создаем ID Критерия (VO) как обертку над Guid, чтобы не путать его с другими Guid и ловить ошибки на этапе компиляции

public readonly record struct CriterionId(Guid Value)  // сахар в скобках создает публичное поле Value {get; init} и конструктор
{
    public static CriterionId New() => new(Guid.NewGuid());  // фабричный метод для создания нового ID

    public override string ToString() => Value.ToString();  // метод для удобного вывода в консоль и логах
}

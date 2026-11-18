namespace Pos.Domain.Students;

// Класс Student (Entity) имеет два свойства VO. Оценки не хранит

public sealed class Student
{
    public StudentId Id { get; }  // ID не может быть изменен
    public StudentName Name { get; }  // Допускаем, что имя не может быть изменено

    private Student(StudentId id, StudentName name)
    {
        Id = id;
        Name = name;
    }

    // Фабричный метод для создания нового студента, валидация на StudentName и StudentId
    public static Student Create(StudentName name) => new(StudentId.New(), name);

    // Восстановление студента из базы данных
    public static Student Restore(StudentId id, StudentName name) => new(id, name);
}

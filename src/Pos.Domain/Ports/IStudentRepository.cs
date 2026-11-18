using Pos.Domain.Students;

namespace Pos.Domain.Ports;

// Порт: хранение студентов снаружи домена.

public interface IStudentRepository
{
    void Save(Student student);
    Student? GetById(StudentId id);
    IReadOnlyList<Student> GetAll();
}

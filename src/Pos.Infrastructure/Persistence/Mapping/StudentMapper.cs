using Pos.Domain.Students;
using Pos.Infrastructure.Persistence.Models;

namespace Pos.Infrastructure.Persistence.Mapping;

internal static class StudentMapper
{
    public static StudentRecord ToRecord(Student student) => new(
        student.Id.Value,
        student.Name.FirstName,
        student.Name.LastName
    );

    // Через Restore: Id сохраняется прежним, имя проходит валидацию заново
    public static Student ToDomain(StudentRecord record) =>
        Student.Restore(new StudentId(record.Id), new StudentName(record.FirstName, record.LastName));
}

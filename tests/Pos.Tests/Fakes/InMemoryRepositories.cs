using Pos.Domain.Grading;
using Pos.Domain.Ports;
using Pos.Domain.Rubrics;
using Pos.Domain.Students;

namespace Pos.Tests.Fakes;

// Подмены портов для тестов фасада: данные в памяти, без файлов
// Фасад зависит от интерфейсов, поэтому не замечает разницы

internal sealed class InMemoryStudentRepository : IStudentRepository
{
    private readonly Dictionary<StudentId, Student> _data = [];
    public void Save(Student student) => _data[student.Id] = student;
    public Student? GetById(StudentId id) => _data.GetValueOrDefault(id);
    public IReadOnlyList<Student> GetAll() => [.. _data.Values];
}

internal sealed class InMemoryRubricRepository : IRubricRepository
{
    private Rubric? _rubric;
    public Rubric? Get() => _rubric;
    public void Save(Rubric rubric) => _rubric = rubric;
}

internal sealed class InMemoryGradebookRepository : IGradebookRepository
{
    private readonly Dictionary<StudentId, Gradebook> _data = [];
    public Gradebook? GetFor(StudentId studentId) => _data.GetValueOrDefault(studentId);
    public void Save(Gradebook gradebook) => _data[gradebook.StudentId] = gradebook;
    public IReadOnlyList<Gradebook> GetAll() => [.. _data.Values];
}

// Ничего не пишет на диск, только запоминает, что ему передали
internal sealed class RecordingReportExporter : IReportExporter
{
    public IReadOnlyList<StudentReport>? LastReports { get; private set; }

    public void Export(Rubric rubric, IReadOnlyList<StudentReport> reports, string path) =>
        LastReports = reports;
}

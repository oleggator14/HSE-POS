using Pos.Domain.Grading;
using Pos.Domain.Students;

namespace Pos.Domain.Ports;

// Ведомость определяется студентом

public interface IGradebookRepository
{
    Gradebook? GetFor(StudentId studentId);
    void Save(Gradebook gradebook);
    IReadOnlyList<Gradebook> GetAll();
}

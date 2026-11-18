using Pos.Domain.Rubrics;
using Pos.Domain.Students;

namespace Pos.Domain.Grading;

// Итог по одному студенту: кто, сколько набрал и из чего это сложилось
// Собирается на лету для отчёта, нигде не хранится

public sealed record StudentReport(
    StudentId StudentId,
    StudentName Student,
    FinalScore FinalScore,
    IReadOnlyDictionary<CriterionId, Evaluation> Evaluations
);

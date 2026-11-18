namespace Pos.Infrastructure.Persistence.Models;

internal sealed record GradebookRecord(
    Guid StudentId,
    IReadOnlyCollection<EvaluationRecord> Evaluations
);

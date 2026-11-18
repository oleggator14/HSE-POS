namespace Pos.Infrastructure.Persistence.Models;

internal sealed record EvaluationRecord(
    Guid CriterionId,
    decimal Score,
    DateTime Date,
    string? Comment
);

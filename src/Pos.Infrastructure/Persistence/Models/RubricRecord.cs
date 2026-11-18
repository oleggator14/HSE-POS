namespace Pos.Infrastructure.Persistence.Models;

internal sealed record RubricRecord(
    IReadOnlyCollection<CriterionRecord> Criteria
);

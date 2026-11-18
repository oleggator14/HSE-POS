namespace Pos.Infrastructure.Persistence.Models;

internal sealed record CriterionRecord(
    Guid CriterionId,
    string Name,
    decimal Weight,
    int MaxScore
);

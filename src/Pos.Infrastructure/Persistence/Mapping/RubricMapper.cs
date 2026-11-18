using Pos.Domain.Rubrics;
using Pos.Infrastructure.Persistence.Models;

namespace Pos.Infrastructure.Persistence.Mapping;

internal static class RubricMapper
{
    public static RubricRecord ToRecord(Rubric rubric) => new([.. rubric.Criteria.Select(ToRecordCriterion)]);

    public static Rubric ToDomain(RubricRecord record)
    {
        var criteria = record.Criteria.Select(ToDomainCriterion);
        return Rubric.Restore(criteria);
    }

    private static CriterionRecord ToRecordCriterion(Criterion c) =>
        new(c.Id.Value, c.Name, c.Weight.Value, c.MaxScore);

    private static Criterion ToDomainCriterion(CriterionRecord r) =>
        Criterion.Restore(
            new CriterionId(r.CriterionId),
            r.Name,
            new Weight(r.Weight),
            r.MaxScore
        );
}

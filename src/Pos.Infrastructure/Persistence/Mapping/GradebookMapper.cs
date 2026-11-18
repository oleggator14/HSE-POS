using Pos.Domain.Grading;
using Pos.Domain.Rubrics;
using Pos.Domain.Students;
using Pos.Infrastructure.Persistence.Models;

namespace Pos.Infrastructure.Persistence.Mapping;

// Перевод ведомости между доменом и форматом хранения
internal static class GradebookMapper
{
    public static GradebookRecord ToRecord(Gradebook gradebook) =>
        new(gradebook.StudentId.Value, [.. gradebook.Evaluations.Select(ToRecordEvaluation)]);

    // Через Restore: инварианты ведомости проверяются заново, файл мог быть повреждён
    public static Gradebook ToDomain(GradebookRecord record) =>
        Gradebook.Restore(
            new StudentId(record.StudentId),
            record.Evaluations.Select(ToDomainEvaluation)
        );

    private static EvaluationRecord ToRecordEvaluation(Evaluation e) =>
        new(e.CriterionId.Value, e.Score.Value, e.Date, e.Comment);

    private static Evaluation ToDomainEvaluation(EvaluationRecord r) =>
        Evaluation.Restore(
            new CriterionId(r.CriterionId),
            new Score(r.Score),
            r.Date,
            r.Comment
        );
}

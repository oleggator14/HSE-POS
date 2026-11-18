using Pos.Domain.Rubrics;

namespace Pos.Domain.Grading;

// Оценка как сущность (Entity - но идентичность локальная, определяется критерием + студентом в ведомости)
// Создается только в рамках агрегата Ведомость, в Gradebook
// StudentId не хранит: студент определён ведомостью, в которой оценка лежит

public sealed class Evaluation
{
    public CriterionId CriterionId { get; }
    public Score Score { get; private set; }
    public DateTime Date { get; private set; }
    public string? Comment { get; private set; }

    internal Evaluation(CriterionId criterionId, Score score, DateTime date, string? comment = null)
    {
        CriterionId = criterionId;
        Score = score;
        Date = date;
        Comment = comment?.Trim();  // Убираем лишние пробелы в начале и конце комментария
    }

    // Исправление оценки (ТЗ). Критерий не меняется — он задаёт идентичность.
    internal void Correct(Score score, DateTime date, string? comment)
    {
        Score = score;
        Date = date;
        Comment = comment?.Trim();
    }

    // Восстановление из хранилища
    public static Evaluation Restore(CriterionId criterionId, Score score, DateTime date, string? comment = null) => new(criterionId, score, date, comment);
}

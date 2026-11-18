using Pos.Application;
using Pos.Domain;
using Pos.Domain.Rubrics;

namespace Pos.Cli;

// Демонстрация сквозного сценария ТЗ: студенты и критерии -> оценки -> итог -> экспорт
// Работает только через фасад — те же сценарии, что вызывает меню
// Заодно показывает, что инварианты домена не пропускают неверные данные

public sealed class DemoRunner
{
    private readonly PosFacade _facade;
    private readonly string _outputDir;

    // Пауза между шагами, чтобы вывод можно было прочитать по ходу
    private const int StepDelayMs = 3000;

    public DemoRunner(PosFacade facade, string outputDir)
    {
        _facade = facade;
        _outputDir = outputDir;
    }

    public void Run()
    {
        Console.WriteLine("=== ПОС: демонстрация сквозного сценария ===");

        Step("1. Заводим студентов");
        var ivanov = _facade.AddStudent("Олег", "Иванов");
        var petrova = _facade.AddStudent("Анна", "Петрова");
        var abramov = _facade.AddStudent("Иван", "Абрамов");
        foreach (var s in _facade.GetStudents())
            Console.WriteLine($"  {s.Name}");

        Step("2. Настраиваем рубрикатор");
        ExpectRejected("веса 50% + 30% не дают 100%", () =>
            _facade.SetupRubric([new("Домашки", 50, 10), new("Семинары", 30, 5)]));
        ExpectRejected("два критерия с одинаковым именем", () =>
            _facade.SetupRubric([new("Домашки", 50, 10), new("домашки", 50, 10)]));

        _facade.SetupRubric(
        [
            new("Посещал лекции", 20, 1),
            new("Ходил на семинары", 30, 5),
            new("Домашки", 50, 10),
        ]);

        var criteria = _facade.GetRubric()!.Criteria.ToList();
        foreach (var c in criteria)
            Console.WriteLine($"  {c.Name}: макс {c.MaxScore}, вес {c.Weight.Value * 100:0.##}%");

        var lectures = criteria[0].Id;
        var seminars = criteria[1].Id;
        var homework = criteria[2].Id;

        Step("3. Выставляем оценки");
        _facade.GiveEvaluation(ivanov, lectures, 1);
        _facade.GiveEvaluation(ivanov, seminars, 4);
        _facade.GiveEvaluation(ivanov, homework, 8);
        _facade.GiveEvaluation(petrova, lectures, 1);
        _facade.GiveEvaluation(petrova, seminars, 5);
        _facade.GiveEvaluation(petrova, homework, 9.5m, "отличная работа");
        _facade.GiveEvaluation(abramov, lectures, 0);
        _facade.GiveEvaluation(abramov, seminars, 3);
        Console.WriteLine("  Иванов и Петрова оценены полностью, у Абрамова нет оценки за домашки");

        Step("4. Проверяем инварианты");
        ExpectRejected("балл 12 при максимуме 10", () =>
            _facade.GiveEvaluation(abramov, homework, 12));
        ExpectRejected("критерий не из рубрикатора", () =>
            _facade.GiveEvaluation(abramov, CriterionId.New(), 5));
        ExpectRejected("повторная оценка по тому же критерию", () =>
            _facade.GiveEvaluation(ivanov, homework, 7));
        ExpectRejected("итог при неполном наборе оценок", () =>
            _facade.GetFinalScore(abramov));
        ExpectRejected("экспорт, пока оценены не все", () =>
            _facade.ExportReports(Path.Combine(_outputDir, "report.csv")));

        Step("5. Исправляем и отменяем оценки");
        _facade.CorrectEvaluation(ivanov, homework, 7, "пересдал после дедлайна");
        var corrected = _facade.GetEvaluations(ivanov)[homework];
        Console.WriteLine($"  Иванов, домашки исправлены: {corrected.Score.Value:0.##} ({corrected.Comment})");

        _facade.CancelEvaluation(petrova, lectures);
        Console.WriteLine($"  Петрова, лекции отменены: оценка есть = {_facade.HasEvaluation(petrova, lectures)}");
        _facade.GiveEvaluation(petrova, lectures, 1);
        Console.WriteLine("  Петрова, лекции выставлены заново");

        _facade.GiveEvaluation(abramov, homework, 6);
        Console.WriteLine("  Абрамов, домашки выставлены — теперь все оценены полностью");

        Step("6. Итоги");
        foreach (var s in _facade.GetStudents())
            Console.WriteLine($"  {s.Name}: {_facade.GetFinalScore(s.Id)}");

        Step("7. Экспорт");
        var csv = Path.Combine(_outputDir, "report.csv");
        var json = Path.Combine(_outputDir, "report.json");
        _facade.ExportReports(csv);
        _facade.ExportReports(json);
        Console.WriteLine($"  CSV:  {csv}");
        Console.WriteLine($"  JSON: {json}");
        ExpectRejected("неподдерживаемый формат .txt", () =>
            _facade.ExportReports(Path.Combine(_outputDir, "report.txt")));

        Console.WriteLine();
        Console.WriteLine("=== Демонстрация завершена ===");
    }

    private static void Step(string title)
    {
        Thread.Sleep(StepDelayMs);
        Console.WriteLine();
        Console.WriteLine(title);
    }

    // Операция обязана быть отклонена: если прошла — это ошибка модели, видно сразу
    private static void ExpectRejected(string what, Action action)
    {
        try
        {
            action();
            Console.WriteLine($"  [!] {what}: ожидался отказ, но операция прошла");
        }
        catch (Exception ex) when (ex is DomainException or NotSupportedException)
        {
            Console.WriteLine($"  [отклонено] {what}: {ex.Message}");
        }
    }
}

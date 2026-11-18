using Pos.Application;
using Pos.Domain;
using Pos.Domain.Rubrics;
using Pos.Domain.Students;

namespace Pos.Cli;

// Консольный интерфейс: спрашивает, показывает, ловит ошибки
// Правил здесь нет — всё делает фасад и домен

public sealed class MenuRunner
{
    private readonly PosFacade _facade;

    // Пункт меню: что нажать, что показать, что выполнить
    private sealed record MenuItem(string Key, string Title, Action Handler);

    private readonly IReadOnlyList<MenuItem> _items;

    public MenuRunner(PosFacade facade)
    {
        _facade = facade;
        _items =
        [
            new("1", "Добавить студента", AddStudent),
            new("2", "Список студентов", ListStudents),
            new("3", "Настроить рубрикатор", SetupRubric),
            new("4", "Показать рубрикатор", ShowRubric),
            new("5", "Выставить оценку", GiveEvaluation),
            new("6", "Оценки студента", ShowEvaluations),
            new("7", "Отменить оценку", CancelEvaluation),
            new("8", "Итог студента", ShowFinalScore),
            new("9", "Экспорт итогов", ExportReports),
        ];
    }

    public void Run()
    {
        while (true)
        {
            PrintMenu();
            Console.Write("> ");
            var choice = Console.ReadLine();

            // null — поток ввода закрыт (Ctrl+Z / Ctrl+D), выходим как по «0»
            if (choice is null || choice.Trim() == "0")
                return;

            var item = _items.FirstOrDefault(i => i.Key == choice.Trim());
            if (item is null)
            {
                Console.WriteLine("Нет такого пункта");
                Console.WriteLine();
                continue;
            }

            try
            {
                item.Handler();
            }
            catch (DomainException ex)
            {
                // Нарушено доменное правило: показываем причину и продолжаем работу
                Console.WriteLine($"Error: {ex.Message}");
            }
            catch (NotSupportedException ex)
            {
                // Неизвестный формат экспорта
                Console.WriteLine($"Error: {ex.Message}");
            }

            Console.WriteLine();
        }
    }

    private void PrintMenu()
    {
        Console.WriteLine("=== ПОС — Преподавательская Оценка Студентов ===");
        foreach (var item in _items)
            Console.WriteLine($"{item.Key}. {item.Title}");
        Console.WriteLine("0. Выход");
    }

    // ───────────── Студенты ─────────────

    private void AddStudent()
    {
        Console.Write("Фамилия и имя: ");
        var input = Console.ReadLine() ?? "";

        // Первое слово — фамилия, остаток — имя
        var parts = input.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        var lastName = parts.Length > 0 ? parts[0] : "";
        var firstName = parts.Length > 1 ? parts[1] : "";

        _facade.AddStudent(firstName, lastName);
        Console.WriteLine($"Добавлен: {lastName} {firstName}");
    }

    private void ListStudents()
    {
        var students = _facade.GetStudents();

        if (students.Count == 0)
        {
            Console.WriteLine("Студентов пока нет");
            return;
        }

        Console.WriteLine($"Студенты ({students.Count}):");
        for (var i = 0; i < students.Count; i++)
            Console.WriteLine($"{i + 1,4}. {students[i].Name}");
    }

    // ───────────── Рубрикатор ─────────────

    private void SetupRubric()
    {
        // Рубрикатор неизменяем: настройка заново заменяет его целиком
        if (_facade.GetRubric() is not null &&
            !ConsoleHelper.ReadYesNo("Рубрикатор уже настроен. Заменить целиком?"))
            return;

        var count = ConsoleHelper.ReadInt("Число критериев (сумма весов должна давать 100%): ", 1, 20);
        var inputs = new List<CriterionInput>();

        for (var i = 1; i <= count; i++)
        {
            Console.WriteLine($"Критерий {i}:");
            var name = ConsoleHelper.ReadString("  Название: ");
            var maxScore = ConsoleHelper.ReadInt("  Максимальный балл: ", 1, 1000);
            var weight = ConsoleHelper.ReadInt("  Вес, %: ", 1, 100);
            inputs.Add(new CriterionInput(name, weight, maxScore));
        }

        // Сумма весов, дубли имён — проверит домен при сборке
        _facade.SetupRubric(inputs);
        Console.WriteLine("Рубрикатор сохранён");
    }

    private void ShowRubric()
    {
        var rubric = RequireRubric();
        if (rubric is null) return;

        Console.WriteLine("Рубрикатор:");
        PrintCriteria(rubric.Criteria.ToList());
    }

    private void GiveEvaluation()
    {
        var rubric = RequireRubric();
        if (rubric is null) return;

        var student = SelectStudent();
        if (student is null) return;

        var criterion = SelectCriterion(rubric);

        // Перезапись — осознанное действие: спрашиваем до ввода балла
        var exists = _facade.HasEvaluation(student.Id, criterion.Id);
        if (exists && !ConsoleHelper.ReadYesNo($"Оценка по «{criterion.Name}» уже есть. Исправить?"))
            return;

        var score = ConsoleHelper.ReadDecimal($"Балл (от 0 до {criterion.MaxScore}): ");
        var comment = ConsoleHelper.ReadString("Комментарий (Enter — без него): ");
        var commentOrNull = string.IsNullOrWhiteSpace(comment) ? null : comment;

        // Диапазон балла проверяет домен; здесь только выбираем сценарий
        if (exists)
            _facade.CorrectEvaluation(student.Id, criterion.Id, score, commentOrNull);
        else
            _facade.GiveEvaluation(student.Id, criterion.Id, score, commentOrNull);

        Console.WriteLine(exists ? "Оценка исправлена" : "Оценка выставлена");
    }

    private void ShowEvaluations()
    {
        var rubric = RequireRubric();
        if (rubric is null) return;

        var student = SelectStudent();
        if (student is null) return;

        var evaluations = _facade.GetEvaluations(student.Id);

        // Идём по критериям рубрикатора, чтобы видеть и неоценённые
        Console.WriteLine($"Оценки: {student.Name}");
        foreach (var c in rubric.Criteria)
        {
            if (evaluations.TryGetValue(c.Id, out var e))
            {
                var date = e.Date.ToLocalTime().ToString("dd.MM.yyyy HH:mm");
                var comment = e.Comment is null ? "" : $" — {e.Comment}";
                Console.WriteLine($"  {c.Name}: {e.Score.Value:0.##} / {c.MaxScore} ({date}){comment}");
            }
            else
            {
                Console.WriteLine($"  {c.Name}: —");
            }
        }
    }

    private void CancelEvaluation()
    {
        var rubric = RequireRubric();
        if (rubric is null) return;

        var student = SelectStudent();
        if (student is null) return;

        var criterion = SelectCriterion(rubric);

        if (!ConsoleHelper.ReadYesNo($"Отменить оценку «{criterion.Name}» у {student.Name}?"))
            return;

        _facade.CancelEvaluation(student.Id, criterion.Id);
        Console.WriteLine("Оценка отменена");
    }

    // ───────────── Итоги ─────────────

    private void ShowFinalScore()
    {
        var student = SelectStudent();
        if (student is null) return;

        // Не всё оценено — домен перечислит недостающие критерии
        var total = _facade.GetFinalScore(student.Id);
        Console.WriteLine($"Итог {student.Name}: {total}");
    }

    private void ExportReports()
    {
        var input = ConsoleHelper.ReadString("Файл [data/report.csv]: ");

        // Относительный путь — от папки программы, как и данные; абсолютный Path.Combine оставит как есть
        var relative = string.IsNullOrEmpty(input) ? Path.Combine("data", "report.csv") : input;
        var path = Path.Combine(AppContext.BaseDirectory, relative);

        _facade.ExportReports(path);
        Console.WriteLine($"Сохранено: {Path.GetFullPath(path)}");
    }

    // ───────────── Выбор из списка ─────────────

    // Печатаем и выбираем из одного и того же списка, чтобы номер не съехал
    private Student? SelectStudent()
    {
        var students = _facade.GetStudents();
        if (students.Count == 0)
        {
            Console.WriteLine("Сначала добавьте студентов");
            return null;
        }

        PrintStudents(students);
        var n = ConsoleHelper.ReadInt("Номер студента: ", 1, students.Count);
        return students[n - 1];
    }

    private static Criterion SelectCriterion(Rubric rubric)
    {
        var criteria = rubric.Criteria.ToList();
        PrintCriteria(criteria);
        var n = ConsoleHelper.ReadInt("Номер критерия: ", 1, criteria.Count);
        return criteria[n - 1];
    }

    private Rubric? RequireRubric()
    {
        var rubric = _facade.GetRubric();
        if (rubric is null)
            Console.WriteLine("Рубрикатор не настроен");
        return rubric;
    }

    private static void PrintStudents(IReadOnlyList<Student> students)
    {
        for (var i = 0; i < students.Count; i++)
            Console.WriteLine($"{i + 1,4}. {students[i].Name}");
    }

    private static void PrintCriteria(IReadOnlyList<Criterion> criteria)
    {
        for (var i = 0; i < criteria.Count; i++)
        {
            var c = criteria[i];
            Console.WriteLine($"{i + 1,4}. {c.Name} (из {c.MaxScore}, вес {c.Weight.Value * 100:0.##}%)");
        }
    }
}

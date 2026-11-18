using Pos.Domain;
using Pos.Domain.Grading;
using Pos.Domain.Ports;
using Pos.Domain.Rubrics;
using Pos.Domain.Services;
using Pos.Domain.Students;
using System.Globalization;

namespace Pos.Application;

// Facade (GoF, структурный): единая точка входа для консоли
// Сценарии не размазываются по пунктам меню и переиспользуются демо-режимом и тестами.
// Правил здесь нет — только загрузка агрегатов, вызов домена и сохранение.
// Единственная проверка, которую делает фасад, — существование ДРУГОГО агрегата:
// это не инвариант, домен его проверить не может (агрегат не ходит в репозиторий).

public sealed class PosFacade
{
    // Все делаем синхронно, потому что единственный пользователь и локальные файлы; асинхронность дала бы инфраструктурную сложность без выигрыша

    private readonly IStudentRepository _studentRepo;
    private readonly IRubricRepository _rubricRepo;
    private readonly IGradebookRepository _gradebookRepo;
    private readonly FinalScoreCalculator _calculator;
    private readonly IReportExporter _exporter;

    public PosFacade(
        IStudentRepository studentRepo,
        IRubricRepository rubricRepo,
        IGradebookRepository gradebookRepo,
        FinalScoreCalculator calculator,
        IReportExporter exporter)
    {
        _studentRepo = studentRepo;
        _rubricRepo = rubricRepo;
        _gradebookRepo = gradebookRepo;
        _calculator = calculator;
        _exporter = exporter;
    }

    public StudentId AddStudent(string firstName, string lastName)
    {
        var student = Student.Create(new StudentName(firstName, lastName));
        _studentRepo.Save(student);
        return student.Id;
    }

    public IReadOnlyList<Student> GetStudents()
    {
        return [.. StudentsInOrder()];  // Упорядоченно по ФИ
    }

    // Сборка рубрикатора через Builder: вес приходит в процентах (так удобнее человеку),
    // домен хранит долю. Конвертация — здесь
    public void SetupRubric(IReadOnlyList<CriterionInput> criteriaInput)
    {
        var rubricBuilder = new RubricBuilder();
        foreach (var input in criteriaInput)
        {
            rubricBuilder.Add(input.Name, Weight.FromPercent(input.WeightPercent), input.MaxScore);
        }
        _rubricRepo.Save(rubricBuilder.Build());
    }

    public Rubric? GetRubric()
    {
        return _rubricRepo.Get();
    }

    // Ведомость создаётся на лету: до первой оценки её в хранилище нет.
    // Все инварианты проверяет gradebook.Grade — фасад в них не вмешивается
    public void GiveEvaluation(StudentId studentId, CriterionId criterionId, decimal rawScore, string? comment = null)
    {
        EnsureStudentExists(studentId);

        var rubric = _rubricRepo.Get() ?? throw new DomainException("Rubric not set up.");
        var gradebook = _gradebookRepo.GetFor(studentId) ?? Gradebook.CreateEmpty(studentId);
        gradebook.Grade(rubric, criterionId, rawScore, comment);
        _gradebookRepo.Save(gradebook);
    }

    // В отличие от GiveEvaluation ведомость не создаётся: исправлять нечего, если оценок нет
    public void CorrectEvaluation(StudentId studentId, CriterionId criterionId, decimal rawScore, string? comment = null)
    {
        EnsureStudentExists(studentId);

        var rubric = _rubricRepo.Get() ?? throw new DomainException("Rubric not set up.");
        var gradebook = _gradebookRepo.GetFor(studentId) ?? throw new DomainException($"Gradebook not found for student {studentId}.");
        gradebook.Regrade(rubric, criterionId, rawScore, comment);
        _gradebookRepo.Save(gradebook);
    }

    public void CancelEvaluation(StudentId studentId, CriterionId criterionId)
    {
        EnsureStudentExists(studentId);

        var gradebook = _gradebookRepo.GetFor(studentId) ?? throw new DomainException($"Gradebook not found for student {studentId}.");
        gradebook.Remove(criterionId);
        _gradebookRepo.Save(gradebook);
    }

    // Запрос для CLI: спросить до выставления, чтобы предложить перезапись
    // Нет ведомости — false (студент есть, оценок нет); нет студента — исключение
    public bool HasEvaluation(StudentId studentId, CriterionId criterionId)
    {
        EnsureStudentExists(studentId);

        var gradebook = _gradebookRepo.GetFor(studentId);
        return gradebook?.Has(criterionId) ?? false;
    }

    // Оценки студента по критериям; пустой словарь — оценок ещё нет
    public IReadOnlyDictionary<CriterionId, Evaluation> GetEvaluations(StudentId studentId)
    {
        EnsureStudentExists(studentId);
        var gradebook = _gradebookRepo.GetFor(studentId);
        return gradebook?.EvaluationsByCriterion ?? new Dictionary<CriterionId, Evaluation>();
    }

    // Итог нигде не хранится — он вычисляется из актуальных оценок и рубрикатора
    // Поэтому инвариант ТЗ «итог согласован с набором оценок» выполняется по построению
    public FinalScore GetFinalScore(StudentId studentId)
    {
        EnsureStudentExists(studentId);
        var rubric = _rubricRepo.Get() ?? throw new DomainException("Rubric not set up.");
        var gradebook = _gradebookRepo.GetFor(studentId) ?? throw new DomainException($"Gradebook not found for student {studentId}.");
        return _calculator.CalculateFinalScore(gradebook, rubric);
    }

    // Отчёты строятся только по полному набору оценок, поэтому сначала проверяем всех
    // и сообщаем сразу обо всех проблемных студентах, а не об одном первом.
    public IReadOnlyList<StudentReport> BuildReports()
    {
        var rubric = _rubricRepo.Get() ?? throw new DomainException("Rubric not set up.");

        var reports = new List<StudentReport>();
        var incomplete = new List<string>();

        foreach (var student in StudentsInOrder())
        {
            var gradebook = _gradebookRepo.GetFor(student.Id);

            if (gradebook is null || !gradebook.IsComplete(rubric))
            {
                incomplete.Add(student.Name.ToString());
                continue;
            }

            var finalScore = _calculator.CalculateFinalScore(gradebook, rubric);
            reports.Add(new StudentReport(student.Id, student.Name, finalScore, gradebook.EvaluationsByCriterion));
        }

        if (incomplete.Count > 0)
            throw new DomainException($"Not all criteria are graded for: {string.Join(", ", incomplete)}.");

        return reports;
    }

    // Экспорт отчётов, передаем infrastructure-слою
    public void ExportReports(string path)
    {
        var rubric = _rubricRepo.Get() ?? throw new DomainException("Rubric not set up.");
        _exporter.Export(rubric, BuildReports(), path);
    }

    private void EnsureStudentExists(StudentId studentId)
    {
        if (_studentRepo.GetById(studentId) is null)
            throw new DomainException($"Student {studentId} not found.");
    }

    // Порядок для человека: по фамилии, затем по имени
    // Один на все сценарии, чтобы список в меню и строки экспорта совпадали
    private static readonly StringComparer RuComparer =
        StringComparer.Create(CultureInfo.GetCultureInfo("ru-RU"), ignoreCase: true);

    private IEnumerable<Student> StudentsInOrder() =>
        _studentRepo.GetAll()
            .OrderBy(s => s.Name.LastName, RuComparer)
            .ThenBy(s => s.Name.FirstName, RuComparer);
}

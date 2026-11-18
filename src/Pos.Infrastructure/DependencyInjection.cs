using Microsoft.Extensions.DependencyInjection;
using Pos.Domain.Ports;
using Pos.Infrastructure.Export;
using Pos.Infrastructure.Persistence;

namespace Pos.Infrastructure;

// Регистрация адаптеров инфраструктуры в DI-контейнере
// Единственный публичный тип сборки: остальные классы internal,
// поэтому Program.cs физически не может сослаться на JSON-репозиторий по имени

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, string dataDir)
    {
        // Репозитории — синглтоны: у каждого кеш, он должен быть один на всю программу
        services.AddSingleton<IStudentRepository>(_ =>
            new JsonStudentRepository(Path.Combine(dataDir, "students.json")));
        services.AddSingleton<IRubricRepository>(_ =>
            new JsonRubricRepository(Path.Combine(dataDir, "rubric.json")));
        services.AddSingleton<IGradebookRepository>(_ =>
            new JsonGradebookRepository(Path.Combine(dataDir, "gradebooks.json")));

        // Конкретные экспортёры — по своему типу, чтобы диспетчер получил их через конструктор
        // Как IReportExporter зарегистрирован только диспетчер: фасад получит его
        services.AddSingleton<CsvReportExporter>();
        services.AddSingleton<JsonReportExporter>();
        services.AddSingleton<IReportExporter, ExtensionReportExporter>();

        return services;
    }
}

using Microsoft.Extensions.DependencyInjection;
using Pos.Application;
using Pos.Cli;
using Pos.Domain.Services;
using Pos.Infrastructure;

// Исправление кодировки - без него знаки вопроса (?) вместо букв
Console.OutputEncoding = System.Text.Encoding.UTF8;
Console.InputEncoding = System.Text.Encoding.UTF8;

// --demo: прогон сквозного сценария ТЗ вместо меню
var demo = args.Contains("--demo") || args.Contains("-d");

// Данные кладём рядом с exe, чтобы путь не зависел от того, откуда запустили
// Демо пишет в свою папку и каждый раз начинает с чистого листа: рабочие данные не трогаются
var dataDir = Path.Combine(AppContext.BaseDirectory, demo ? "data-demo" : "data");
if (demo && Directory.Exists(dataDir))
    Directory.Delete(dataDir, recursive: true);

// Composition root: единственное место, где собирается граф зависимостей
var services = new ServiceCollection();
services.AddInfrastructure(dataDir);

services.AddSingleton<FinalScoreCalculator>();
services.AddSingleton<PosFacade>();
services.AddSingleton<MenuRunner>();
services.AddSingleton(sp => new DemoRunner(sp.GetRequiredService<PosFacade>(), dataDir));

using var provider = services.BuildServiceProvider();

if (demo)
    provider.GetRequiredService<DemoRunner>().Run();
else
    provider.GetRequiredService<MenuRunner>().Run();

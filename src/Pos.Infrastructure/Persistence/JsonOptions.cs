using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;

namespace Pos.Infrastructure.Persistence;

// Единые настройки для всех JSON-файлов

internal static class JsonOptions
{
    public static readonly JsonSerializerOptions Default = new()
    {
        WriteIndented = true,  // Отступы в файле
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,  // Имена полей camelCase
        Encoder = JavaScriptEncoder.Create(UnicodeRanges.All)  // Кириллица без \uXXXX
    };
}

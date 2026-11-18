namespace Pos.Infrastructure.Persistence.Models;

// Форма студента в students.json
// Анемичность осознанна: это формат файла, а не предметная область

internal sealed record StudentRecord(
    Guid Id,
    string FirstName,
    string LastName
);

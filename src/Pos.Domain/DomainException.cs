namespace Pos.Domain;

// DomainException для обработки ошибок в доменной модели
// 1. Дает возможность написать один catch для ловли всех нарушений правил домена
// 2. Отличает ошибки программы от неверного ввода данных пользователем

public sealed class DomainException(string message) : Exception(message)
{
}

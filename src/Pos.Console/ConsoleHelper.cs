using System.Globalization;

namespace Pos.Cli;

// Ввод с повтором при ошибке: CLI сам переспрашивает, не бросая исключений
// Доменные правила (диапазон балла, сумма весов) здесь не проверяются — это дело домена

internal static class ConsoleHelper
{
    public static string ReadString(string prompt)
    {
        Console.Write(prompt);
        return Console.ReadLine()?.Trim() ?? "";
    }

    public static int ReadInt(string prompt, int min = int.MinValue, int max = int.MaxValue)
    {
        while (true)
        {
            var input = ReadString(prompt);
            if (int.TryParse(input, out var value) && value >= min && value <= max)
                return value;
            Console.WriteLine($"Введите целое число от {min} до {max}");
        }
    }

    // Принимает и «8,5», и «8.5»
    public static decimal ReadDecimal(string prompt)
    {
        while (true)
        {
            var input = ReadString(prompt).Replace(',', '.');
            if (decimal.TryParse(input, NumberStyles.Number, CultureInfo.InvariantCulture, out var value))
                return value;
            Console.WriteLine("Введите число, например 8 или 8.5");
        }
    }

    public static bool ReadYesNo(string prompt)
    {
        while (true)
        {
            var input = ReadString(prompt + " (y/n): ").ToLowerInvariant();
            if (input is "y" or "д") return true;
            if (input is "n" or "н") return false;
        }
    }
}


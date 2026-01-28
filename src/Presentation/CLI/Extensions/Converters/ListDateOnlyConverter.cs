using System.Globalization;

namespace CLI.Extensions.Converters;

public class ListDateOnlyConverter
{
    public object? Convert(string input, out string? errorMessage)
    {
        errorMessage = null;

        if (string.IsNullOrWhiteSpace(input))
            return null;

        var dates = new List<DateOnly>();
        var dateStrings = input.Split(';', StringSplitOptions.RemoveEmptyEntries);

        if (dateStrings.Length == 0)
        {
            errorMessage = "Не найдено дат для парсинга";
            return null;
        }

        foreach (var dateString in dateStrings)
        {
            var trimmedDate = dateString.Trim();

            if (DateOnly.TryParseExact(trimmedDate, "dd.MM.yy",
                    CultureInfo.InvariantCulture, DateTimeStyles.None, out var date))
            {
                dates.Add(date);
            }
            else
            {
                errorMessage = $"Неверный формат даты: '{trimmedDate}'. Ожидается dd.MM.yy (например: 15.01.26)";
                return null;
            }
        }

        return dates;
    }
}
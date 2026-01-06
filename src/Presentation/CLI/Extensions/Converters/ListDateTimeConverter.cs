using System.Globalization;

namespace CLI.Extensions.Converters;

public class ListDateTimeConverter
{
    public object? Convert(string input, out string? errorMessage)
    {
        errorMessage = null;
        
        if (string.IsNullOrWhiteSpace(input))
            return null;
        
        var startPoints = new List<DateTime>();
        var timeStrings = input.Split(';', StringSplitOptions.RemoveEmptyEntries);
        
        if (timeStrings.Length == 0)
        {
            errorMessage = "Не найдено времён для парсинга";
            return null;
        }
        
        foreach (var timeString in timeStrings)
        {
            var trimmedTime = timeString.Trim();
            
            if (DateTime.TryParseExact(trimmedTime, "HH:mm", 
                    CultureInfo.InvariantCulture, DateTimeStyles.None, out var time))
            {
                var dateTime = DateTime.Today.Add(time.TimeOfDay);
                startPoints.Add(dateTime);
            }
            else
            {
                errorMessage = $"Неверный формат времени: '{trimmedTime}'. Ожидается HH:mm (например: 09:30)";
                return null;
            }
        }
        
        return startPoints;
    }
}

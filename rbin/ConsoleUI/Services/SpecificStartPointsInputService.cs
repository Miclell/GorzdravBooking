namespace ConsoleUI.Services;

public static class SpecificStartPointsInputService
{
    public static List<DateTime> ReadModel()
    {
        var startPoints = new List<DateTime>();
        var currentDate = DateTime.Today;
        
        Console.WriteLine("Введите точки старта. Для завершения нажмите Enter");

        while (true)
        {
            Console.Write("Введите время старта (ЧЧ:ММ): ");
            var input = Console.ReadLine()?.Trim();

            if (string.IsNullOrEmpty(input))
                break;

            if (DateTime.TryParseExact(input, ["HH:mm", "H:mm"], null, System.Globalization.DateTimeStyles.None, out var dateTime))
            {
                startPoints.Add(currentDate.Date.Add(dateTime.TimeOfDay));
            }
            else
            {
                Console.WriteLine("Ошибка: Неверный формат времени. Используйте формат ЧЧ:ММ");
            }
        }

        return startPoints.OrderBy(t => t.TimeOfDay).ToList();
    }
}
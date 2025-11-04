using StatefulMenu.Core.Models;

namespace CLI;

public static class HeaderFactorySetup
{
    public static async Task<MenuHeaderOptions> SetupHeader()
    {
        return new MenuHeaderOptions
        {
            Separator = " | ",
            Segments = new List<Func<string>>
            {
                () => GetCountActiveAppointmentSearchRequests().Result,
                () => GetStatus().Result,
                () => GetTimeToNextRequest().Result,
                () => GetLastRequests().Result
            }
        };
    }
    
    // *Вкладка* | Активные здания: *count* | Running/Pauesed (•) | Следующий запрос через: ** |✓/×
    //
    // | Главное меню | 1 | ••• | 15м | ×××✓×
    //
    // • - подключение к серверу есть 
    // •• - запущен сервис 
    // ••• - задания выполняются

    private static async Task<string> GetCountActiveAppointmentSearchRequests()
    {
        return 1.ToString(); // TODO - бд
    }
    
    private static async Task<string> GetStatus()
    {
        return "•••";
        // 1 - есть коннект с горздравом 2 - сервис работает 3 - задания выполняются
    }

    private static async Task<string> GetTimeToNextRequest()
    {
        return "10";
    }

    private static async Task<string> GetLastRequests()
    {
        return "×××✓×";
    }
}
using ConsoleUI.Core;
using ConsoleUI.Core.Interfaces;
using ConsoleUI.Menus.AppointmentMenu.ShowAppointmentFlow.ShowActiveAppointmentsFlow;
using ConsoleUI.Menus.AppointmentMenu.ShowAppointmentFlow.ShowUpcomingAppointmentsFlow;
using ConsoleUI.Services;
using Microsoft.Extensions.DependencyInjection;

namespace ConsoleUI.Menus.AppointmentMenu.ShowAppointmentFlow;

public class RunShowAppointmentFlowProvider(IServiceProvider serviceProvider) : IMenuProvider
{
    public async Task<MenuState> CreateMenuAsync()
    {
        // Активные запросы на запись
        // Предстоящие посещения
        var navigation = serviceProvider.GetRequiredService<NavigationService>();
        
        return new MenuState()
        {
            Title = "Просмотр и редактирование записей",
            Commands = 
            [
                navigation.CreateCommand<ShowActiveAppointmentsCommand>(),
                navigation.CreateCommand<ShowUpcomingAppointmentsCommand>(),
                new BackCommand()
            ]
        };
    }
}
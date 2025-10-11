using ConsoleUI.Core;
using ConsoleUI.Core.Interfaces;
using ConsoleUI.Menus.AppointmentMenu.CreateAppointmentFlow;
using ConsoleUI.Menus.AppointmentMenu.ShowAppointmentFlow;
using ConsoleUI.Menus.AppointmentMenu.TimePreferencesMenu;
using ConsoleUI.Services;
using Microsoft.Extensions.DependencyInjection;

namespace ConsoleUI.Menus.AppointmentMenu;

public class ShowAppointmentMenuProvider(IServiceProvider serviceProvider) : IMenuProvider
{
    public async Task<MenuState> CreateMenuAsync()
    {
        var navigation = serviceProvider.GetRequiredService<NavigationService>();
        
        return new MenuState()
        {
            Title = "Выберите вариант",
            Commands = 
            [
                navigation.CreateCommand<RunCreateAppointmentFlowCommand>(),
                navigation.CreateCommand<RunShowAppointmentFlowCommand>(),
                navigation.CreateCommand<RunTimePreferencesMenuCommand>(),
                new BackCommand()
            ]
        };
    }
}
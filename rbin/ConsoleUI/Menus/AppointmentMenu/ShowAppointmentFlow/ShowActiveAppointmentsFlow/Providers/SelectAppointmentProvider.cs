using ConsoleUI.Core;
using ConsoleUI.Core.Interfaces;
using ConsoleUI.Menus.AppointmentMenu.ShowAppointmentFlow.ShowActiveAppointmentsFlow.Commands;
using ConsoleUI.Services;
using Microsoft.Extensions.DependencyInjection;

namespace ConsoleUI.Menus.AppointmentMenu.ShowAppointmentFlow.ShowActiveAppointmentsFlow.Providers;

public class SelectAppointmentProvider(IServiceProvider serviceProvider) : IMenuProvider
{
    public async Task<MenuState> CreateMenuAsync()
    {
        var navigation = serviceProvider.GetRequiredService<NavigationService>();
        
        return new MenuState()
        {
            Title = "Выберите действие",
            Commands = 
            [
                navigation.CreateCommand<SelectTimePreferencesCommand>(),
                navigation.CreateCommand<DeleteAppointmentCommand>(),
                new BackCommand()
            ]
        };
    }
}
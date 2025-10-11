using ConsoleUI.Core;
using ConsoleUI.Core.Interfaces;
using ConsoleUI.Menus.AppointmentMenu;
using ConsoleUI.Menus.Commands;
using ConsoleUI.Menus.Patient.Command;
using ConsoleUI.Menus.Patient.Providers;
using ConsoleUI.Services;
using Microsoft.Extensions.DependencyInjection;

namespace ConsoleUI.Menus.Providers;

public class MainMenuProvider(IServiceProvider serviceProvider) : IMenuProvider
{
    public Task<MenuState> CreateMenuAsync()
    {
        var navigation = serviceProvider.GetRequiredService<NavigationService>();
        
        return Task.FromResult(new MenuState
        {
            Title = "Главное меню",
            Commands =
            [
                navigation.CreateCommand<ShowAppointmentMenuCommand>(),
                navigation.CreateCommand<ShowPatientMenuCommand>(),
                navigation.CreateCommand<ShowDistrictMenuCommand>(),
                navigation.CreateCommand<ExitCommand>()
            ]
        });
    }
}
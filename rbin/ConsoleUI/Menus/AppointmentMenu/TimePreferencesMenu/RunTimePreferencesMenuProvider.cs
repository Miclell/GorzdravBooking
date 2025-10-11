using ConsoleUI.Core;
using ConsoleUI.Core.Interfaces;
using ConsoleUI.Menus.AppointmentMenu.TimePreferencesMenu.Commands;
using ConsoleUI.Services;
using Microsoft.Extensions.DependencyInjection;

namespace ConsoleUI.Menus.AppointmentMenu.TimePreferencesMenu;

public class RunTimePreferencesMenuProvider(IServiceProvider serviceProvider) : IMenuProvider
{
    public async Task<MenuState> CreateMenuAsync()
    {
        var navigation = serviceProvider.GetRequiredService<NavigationService>();
        
        return new MenuState()
        {
            Title = "Просмотр и создание временных предпочтений",
            Commands =
            [
                navigation.CreateCommand<CreateTimePreferencesCommand>(),
                navigation.CreateCommand<ShowTimePreferencesCommand>(),
                new BackCommand()
            ]
        };
    }
}
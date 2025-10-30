using CLI.Menus.AppointmentMenu;
using CLI.Menus.PatientMenu;
using CLI.Menus.TimePreferencesMenu;
using Microsoft.Extensions.DependencyInjection;
using StatefulMenu.Commands.BuiltIn;
using StatefulMenu.Commands.Interfaces;
using StatefulMenu.Core.Models;
using StatefulMenu.Infrastructure.Services;

namespace CLI.Menus;

public class MainMenuProvider(IServiceProvider serviceProvider) : IMenuProvider
{
    public Task<MenuState> CreateMenuAsync(CancellationToken cancellationToken = default)
    {
        var commands = new IMenuCommand[]
        {
            serviceProvider.GetRequiredService<ShowAppointmentMenuCommand>(),
            serviceProvider.GetRequiredService<ShowPatientMenuCommand>(),
            serviceProvider.GetRequiredService<ShowTimePreferencesMenuCommand>(),
            serviceProvider.GetRequiredService<ExitCommand>()
        };

        var items = commands
            .Select(c => c is ExitCommand
                ? new MenuItem(c.Title, _ => c.ExecuteAsync(cancellationToken), hotkey: ConsoleKey.E)
                : new MenuItem(c.Title, _ => c.ExecuteAsync(cancellationToken)))
            .ToList();

        return Task.FromResult(new MenuState("Главное меню", items));
    }
}
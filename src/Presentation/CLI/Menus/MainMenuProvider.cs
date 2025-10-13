using Microsoft.Extensions.DependencyInjection;
using StatefulMenu.Commands.Interfaces;
using StatefulMenu.Core.Models;
using StatefulMenu.Infrastructure.Services;

namespace CLI.Menus;

public class MainMenuProvider(IServiceProvider serviceProvider) : IMenuProvider
{
    public Task<MenuState> CreateMenuAsync(CancellationToken ct = new CancellationToken())
    {
        var commands = new IMenuCommand[]
        {
            serviceProvider.GetRequiredService<Commands.ShowAppointmentMenuCommand>(),
            serviceProvider.GetRequiredService<Commands.ShowPatientMenuCommand>(),
            serviceProvider.GetRequiredService<Commands.ShowDistrictMenuCommand>(),
            serviceProvider.GetRequiredService<Commands.ExitCommand>()
        };

        var items = commands
            .Select(c => c is Commands.ExitCommand
                ? new MenuItem(c.Title, _ => c.ExecuteAsync(ct), hotkey: ConsoleKey.E)
                : new MenuItem(c.Title, _ => c.ExecuteAsync(ct)))
            .ToList();

        return Task.FromResult(new MenuState("Главное меню", items));
    }
}
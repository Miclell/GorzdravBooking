using ConsoleUI.Core;
using ConsoleUI.Core.Interfaces;
using ConsoleUI.Menus.AppointmentMenu.TimePreferencesMenu.Providers;
using Microsoft.Extensions.DependencyInjection;

namespace ConsoleUI.Menus.AppointmentMenu.TimePreferencesMenu.Commands;

public class CreateTimePreferencesCommand(IServiceProvider serviceProvider) : IMenuCommand
{
    public string Title { get; } = "Создать пресет";
    public async Task<MenuState?> ExecuteAsync()
    {
        var createTimePreferencesProvider = serviceProvider.GetRequiredService<CreateTimePreferencesProvider>();
        return await createTimePreferencesProvider.CreateMenuAsync();
    }
}
using ConsoleUI.Core;
using ConsoleUI.Core.Interfaces;
using ConsoleUI.Menus.AppointmentMenu.TimePreferencesMenu.Providers;
using Microsoft.Extensions.DependencyInjection;

namespace ConsoleUI.Menus.AppointmentMenu.TimePreferencesMenu.Commands;

public class ShowTimePreferencesCommand(IServiceProvider serviceProvider) : IMenuCommand
{
    public string Title { get; } = "Просмотр и удаление временных пресетов";
    public async Task<MenuState?> ExecuteAsync()
    {
        var showTimePreferencesProvider = serviceProvider.GetRequiredService<ShowTimePreferencesProvider>();
        return await showTimePreferencesProvider.CreateMenuAsync();
    }
}
using ConsoleUI.Core;
using ConsoleUI.Core.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace ConsoleUI.Menus.AppointmentMenu.TimePreferencesMenu;

public class RunTimePreferencesMenuCommand(IServiceProvider serviceProvider) : IMenuCommand
{
    public string Title { get; } = "Просмотр и создание временных предпочтений";
    public async Task<MenuState?> ExecuteAsync()
    {
        var runTimePreferencesMenuProvider = serviceProvider.GetRequiredService<RunTimePreferencesMenuProvider>();
        return await runTimePreferencesMenuProvider.CreateMenuAsync();
    }
}
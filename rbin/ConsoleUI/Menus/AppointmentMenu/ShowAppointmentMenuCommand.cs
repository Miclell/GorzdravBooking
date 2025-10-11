using ConsoleUI.Core;
using ConsoleUI.Core.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace ConsoleUI.Menus.AppointmentMenu;

public class ShowAppointmentMenuCommand(IServiceProvider serviceProvider) : IMenuCommand
{
    public string Title { get; } = "Запись";
    public async Task<MenuState?> ExecuteAsync()
    {
        var showAppointmentMenuProvider = serviceProvider.GetRequiredService<ShowAppointmentMenuProvider>();
        return await showAppointmentMenuProvider.CreateMenuAsync();
    }
}
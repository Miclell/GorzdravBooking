using ConsoleUI.Core;
using ConsoleUI.Core.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace ConsoleUI.Menus.AppointmentMenu.ShowAppointmentFlow.ShowActiveAppointmentsFlow;

public class ShowActiveAppointmentsCommand(IServiceProvider serviceProvider) : IMenuCommand
{
    public string Title { get; } = "Активные запросы на запись";
    public async Task<MenuState?> ExecuteAsync()
    {
        var showActiveAppointmentsProvider = serviceProvider.GetRequiredService<ShowActiveAppointmentsProvider>();
        return await showActiveAppointmentsProvider.CreateMenuAsync();
    }
}
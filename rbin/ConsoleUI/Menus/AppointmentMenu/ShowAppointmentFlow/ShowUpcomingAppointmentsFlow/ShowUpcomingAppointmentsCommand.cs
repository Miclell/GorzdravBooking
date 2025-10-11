using ConsoleUI.Core;
using ConsoleUI.Core.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace ConsoleUI.Menus.AppointmentMenu.ShowAppointmentFlow.ShowUpcomingAppointmentsFlow;

public class ShowUpcomingAppointmentsCommand(IServiceProvider serviceProvider) : IMenuCommand
{
    public string Title { get; } = "Предстоящие посещения";
    public async Task<MenuState?> ExecuteAsync()
    {
        var showUpcomingAppointmentsProvider = serviceProvider.GetRequiredService<ShowUpcomingAppointmentsProvider>();
        return await showUpcomingAppointmentsProvider.CreateMenuAsync();
    }
}
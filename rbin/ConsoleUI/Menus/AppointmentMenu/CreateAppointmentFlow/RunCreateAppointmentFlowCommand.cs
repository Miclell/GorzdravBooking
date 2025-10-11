using ConsoleUI.Core;
using ConsoleUI.Core.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace ConsoleUI.Menus.AppointmentMenu.CreateAppointmentFlow;

public class RunCreateAppointmentFlowCommand(IServiceProvider serviceProvider) : IMenuCommand
{
    public string Title { get; } = "Создать запрос на запись";
    public async Task<MenuState?> ExecuteAsync()
    {
        var runCreateAppointmentFlowProvider = serviceProvider.GetRequiredService<RunCreateAppointmentFlowProvider>();
        return await runCreateAppointmentFlowProvider.CreateMenuAsync();
    }
}
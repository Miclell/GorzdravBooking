using ConsoleUI.Core;
using ConsoleUI.Core.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace ConsoleUI.Menus.AppointmentMenu.ShowAppointmentFlow;

public class RunShowAppointmentFlowCommand(IServiceProvider serviceProvider) : IMenuCommand
{
    public string Title { get; } = "Просмотр и редактирование записей";
    public async Task<MenuState?> ExecuteAsync()
    {
        var runShowAppointmentFlowProvider = serviceProvider.GetRequiredService<RunShowAppointmentFlowProvider>();
        return await runShowAppointmentFlowProvider.CreateMenuAsync();
    }
}
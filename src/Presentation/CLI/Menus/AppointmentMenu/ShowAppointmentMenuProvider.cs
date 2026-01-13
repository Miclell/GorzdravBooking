using CLI.Helpers;
using CLI.Menus.AppointmentMenu.CreateAppointmentFlow;
using CLI.Menus.AppointmentMenu.CreateReferralAppointmentFlow;
using CLI.Menus.AppointmentMenu.ShowActiveAppointmentsFlow;
using CLI.Menus.AppointmentMenu.ShowAppointmentsFlow;
using Microsoft.Extensions.DependencyInjection;
using StatefulMenu.Commands.BuiltIn;
using StatefulMenu.Commands.Interfaces;
using StatefulMenu.Core.Models;

namespace CLI.Menus.AppointmentMenu;

public class ShowAppointmentMenuProvider(IServiceProvider serviceProvider) : IMenuProvider
{
    public Task<MenuState> CreateMenuAsync(CancellationToken cancellationToken = default)
    {
        var commands = new IMenuCommand[]
        {
            serviceProvider.GetRequiredService<RunCreateAppointmentFlowCommand>(),
            serviceProvider.GetRequiredService<RunCreateReferralAppointmentFlowCommand>(),
            serviceProvider.GetRequiredService<RunShowAppointmentsFlowCommand>(),
            serviceProvider.GetRequiredService<RunShowActiveAppointmentsFlowCommand>(),
            serviceProvider.GetRequiredService<BackCommand>()
        };

        var items = commands
            .Select(c =>
                new MenuItem(c.Title, _ => c.ExecuteAsync(cancellationToken)))
            .ToList();

        return Task.FromResult(new MenuState("Меню записи", items, header: HeaderFactorySetup.SetupHeader()));
    }
}
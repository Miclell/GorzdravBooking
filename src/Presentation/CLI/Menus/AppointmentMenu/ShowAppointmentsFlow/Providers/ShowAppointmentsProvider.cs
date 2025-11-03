using Application.Services.Interfaces;
using CLI.Menus.AppointmentMenu.ShowActiveAppointmentsFlow.Commands;
using CLI.Menus.AppointmentMenu.ShowAppointmentsFlow.Commands;
using StatefulMenu.Commands.BuiltIn;
using StatefulMenu.Commands.Interfaces;
using StatefulMenu.Core.Interfaces;
using StatefulMenu.Core.Models;

namespace CLI.Menus.AppointmentMenu.ShowAppointmentsFlow.Providers;

public class ShowAppointmentsProvider(
    IAppointmentService appointmentService,
    IAppSettingsService appSettingsService,
    IServiceProvider serviceProvider) : IMenuProvider
{
    public async Task<MenuState> CreateMenuAsync(CancellationToken cancellationToken = default)
    {
        var appointments = await appointmentService.GetByUserAsync(await appSettingsService.GetDefaultUserIdAsync(), cancellationToken);
        
        var commands = appointments.Value
            .Select(asr => new AppointmentSelectionCommand(asr, serviceProvider))
            .Cast<IMenuCommand>()
            .Append(new BackCommand())
            .ToList();
        
        var items = commands
            .Select(c => 
                new MenuItem(c.Title, _ => c.ExecuteAsync(cancellationToken)))
            .ToList();

        return new MenuState("Выберите запипсь для отмены", items);
    }
}
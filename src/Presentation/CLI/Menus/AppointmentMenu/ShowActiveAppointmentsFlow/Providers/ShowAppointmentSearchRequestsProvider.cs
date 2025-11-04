using Application.Services.Interfaces;
using CLI.Helpers;
using CLI.Menus.AppointmentMenu.ShowActiveAppointmentsFlow.Commands;
using StatefulMenu.Commands.BuiltIn;
using StatefulMenu.Commands.Interfaces;
using StatefulMenu.Core.Models;

namespace CLI.Menus.AppointmentMenu.ShowActiveAppointmentsFlow.Providers;

public class ShowAppointmentSearchRequestsProvider(
    IAppointmentSearchRequestService appointmentSearchRequestService,
    IAppSettingsService appSettingsService,
    IServiceProvider serviceProvider) : IMenuProvider
{
    public async Task<MenuState> CreateMenuAsync(CancellationToken cancellationToken = default)
    {
        var appointments =
            await appointmentSearchRequestService.GetActiveByUserAsync(await appSettingsService.GetDefaultUserIdAsync(),
                cancellationToken);

        var commands = appointments.Value
            .Select(asr => new AppointmentSearchRequestSelectionCommand(asr, serviceProvider))
            .Cast<IMenuCommand>()
            .Append(new BackCommand())
            .ToList();

        var items = commands
            .Select(c =>
                new MenuItem(c.Title, _ => c.ExecuteAsync(cancellationToken)))
            .ToList();

        return new MenuState("Выберите запрос для отмены", items, header: HeaderFactorySetup.SetupHeader());
    }
}
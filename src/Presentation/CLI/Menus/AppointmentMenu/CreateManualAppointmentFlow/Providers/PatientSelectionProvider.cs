using Application.Services.Interfaces;
using CLI.Helpers;
using CLI.Menus.AppointmentMenu.CreateManualAppointmentFlow.Commands;
using StatefulMenu.Commands.BuiltIn;
using StatefulMenu.Commands.Interfaces;
using StatefulMenu.Core.Models;

namespace CLI.Menus.AppointmentMenu.CreateManualAppointmentFlow.Providers;

public class PatientSelectionProvider(
    IServiceProvider serviceProvider,
    IPatientService patientService,
    IAppSettingsService appSettingsService) : IMenuProvider
{
    public async Task<MenuState> CreateMenuAsync(CancellationToken cancellationToken = default)
    {
        var patients =
            await patientService.GetByUser(await appSettingsService.GetDefaultUserIdAsync(), cancellationToken);

        if (!patients.Value.Any())
            Console.WriteLine("Нет пациентов для записи, создайте пациента!");

        var commands = patients.Value
            .Select(p => new PatientSelectionCommand(p, serviceProvider))
            .Cast<IMenuCommand>()
            .Append(new BackCommand())
            .ToList();

        var items = commands
            .Select(c =>
                new MenuItem(c.Title, _ => c.ExecuteAsync(cancellationToken)))
            .ToList();

        return new MenuState("Выберите пациента для записи", items, header: HeaderFactorySetup.SetupHeader());
    }
}
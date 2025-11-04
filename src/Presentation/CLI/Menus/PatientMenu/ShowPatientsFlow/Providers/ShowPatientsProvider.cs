using Application.Services.Interfaces;
using CLI.Helpers;
using CLI.Menus.PatientMenu.ShowPatientsFlow.Commands;
using Microsoft.Extensions.DependencyInjection;
using StatefulMenu.Commands.BuiltIn;
using StatefulMenu.Commands.Interfaces;
using StatefulMenu.Core.Models;

namespace CLI.Menus.PatientMenu.ShowPatientsFlow.Providers;

public class ShowPatientsProvider(IServiceProvider serviceProvider, IPatientService patientService) : IMenuProvider
{
    public async Task<MenuState> CreateMenuAsync(CancellationToken cancellationToken = default)
    {
        var appSettingService = serviceProvider.GetRequiredService<IAppSettingsService>();
        var patients =
            await patientService.GetByUser(await appSettingService.GetDefaultUserIdAsync(), cancellationToken);

        var commands = patients.Value
            .Select(p => new PatientSelectionCommand(p, serviceProvider))
            .Cast<IMenuCommand>()
            .Append(new BackCommand())
            .ToList();

        var items = commands
            .Select(c =>
                new MenuItem(c.Title, _ => c.ExecuteAsync(cancellationToken)))
            .ToList();

        return new MenuState("Выберите пациента для удаления", items, header: HeaderFactorySetup.SetupHeader());
    }
}
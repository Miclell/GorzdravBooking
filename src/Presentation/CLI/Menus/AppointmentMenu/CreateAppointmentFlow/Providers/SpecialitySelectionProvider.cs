using Application.DTOs.Patient;
using CLI.Helpers;
using CLI.Menus.AppointmentMenu.CreateAppointmentFlow.Commands;
using Core.Interfaces.Services;
using StatefulMenu.Commands.BuiltIn;
using StatefulMenu.Commands.Interfaces;
using StatefulMenu.Core.Interfaces;
using StatefulMenu.Core.Models;

namespace CLI.Menus.AppointmentMenu.CreateAppointmentFlow.Providers;

public class SpecialitySelectionProvider(
    IServiceProvider serviceProvider,
    IDataService dataService,
    IExternalSpecialtyService specialtyService) : IMenuProvider
{
    public async Task<MenuState> CreateMenuAsync(CancellationToken cancellationToken = default)
    {
        dataService.TryGet<BasePatientProfileDto>(nameof(BasePatientProfileDto), out var patient);

        var specalties = await specialtyService.GetByLpuAsync(int.Parse(patient!.LpuId));

        var commands = specalties
            .Select(s => new SpecialitySelectionCommand(s, serviceProvider))
            .Cast<IMenuCommand>()
            .Append(new BackCommand())
            .ToList();

        var items = commands
            .Select(c =>
                new MenuItem(c.Title, _ => c.ExecuteAsync(cancellationToken)))
            .ToList();

        return new MenuState("Выберите специальность", items, header: HeaderFactorySetup.SetupHeader());
    }
}
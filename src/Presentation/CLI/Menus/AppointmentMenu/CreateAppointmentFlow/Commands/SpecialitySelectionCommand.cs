using CLI.Menus.AppointmentMenu.CreateAppointmentFlow.Providers;
using Core.Models;
using Microsoft.Extensions.DependencyInjection;
using StatefulMenu.Commands.Interfaces;
using StatefulMenu.Core.Interfaces;
using StatefulMenu.Core.Models;

namespace CLI.Menus.AppointmentMenu.CreateAppointmentFlow.Commands;

public class SpecialitySelectionCommand(
    MedicalSpeciality speciality,
    IServiceProvider serviceProvider) : IMenuCommand
{
    public string Title { get; } = $"{speciality.Name}";

    public async Task<MenuResult> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        var dataService = serviceProvider.GetRequiredService<IDataService>();
        dataService.Set(nameof(MedicalSpeciality), speciality);

        var doctorSelectionProvider = serviceProvider.GetRequiredService<DoctorSelectionProvider>();
        return MenuResult.Push(await doctorSelectionProvider.CreateMenuAsync(cancellationToken));
    }
}
using Application.DTOs.Patient;
using CLI.Menus.AppointmentMenu.CreateAppointmentFlow.Commands;
using Core.Interfaces.Services;
using Core.Models;
using Infrastructure.Services;
using StatefulMenu.Commands.BuiltIn;
using StatefulMenu.Commands.Interfaces;
using StatefulMenu.Core.Interfaces;
using StatefulMenu.Core.Models;

namespace CLI.Menus.AppointmentMenu.CreateAppointmentFlow.Providers;

public class DoctorSelectionProvider(
    IExternalDoctorService doctorService,
    IDataService dataService,
    IServiceProvider serviceProvider) : IMenuProvider
{
    public async Task<MenuState> CreateMenuAsync(CancellationToken cancellationToken = default)
    {
        dataService.TryGet<BasePatientProfileDto>(nameof(BasePatientProfileDto), out var patient);
        dataService.TryGet<MedicalSpeciality>(nameof(MedicalSpeciality), out var speciality);
        var doctors = await doctorService.GetBySpecialtyAsync(int.Parse(patient!.LpuId), speciality!.Id);
        
        var commands = doctors
            .Select(d => new DoctorSelectionCommand(d, serviceProvider))
            .Cast<IMenuCommand>()
            .Append(new BackCommand())
            .ToList();
        
        var items = commands
            .Select(c => 
                new MenuItem(c.Title, _ => c.ExecuteAsync(cancellationToken)))
            .ToList();

        return new MenuState("Выберите доктора", items);
    }
}
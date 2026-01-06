using Application.DTOs.Patient;
using CLI.Helpers;
using CLI.Menus.AppointmentMenu.CreateAppointmentFlow.Commands;
using Core.Interfaces.Services;
using Core.Models;
using Microsoft.Extensions.DependencyInjection;
using StatefulMenu.Commands.BuiltIn;
using StatefulMenu.Commands.Interfaces;
using StatefulMenu.Core.Attributes;
using StatefulMenu.Core.Interfaces;
using StatefulMenu.Core.Models;

namespace CLI.Menus.AppointmentMenu.CreateAppointmentFlow.Providers;

public class DoctorSelectionProvider(
    IExternalDoctorService doctorService,
    IDataService dataService,
    IConsoleInputService inputService,
    IServiceProvider serviceProvider) : IMenuProvider
{
    public async Task<MenuState> CreateMenuAsync(CancellationToken cancellationToken = default)
    {
        List<Doctor>? doctors;
        if (!dataService.TryGet<bool>("IsAnotherDoctor", out _))
        {
            var isAnyOfSpeciality = (await inputService
                    .ReadModelAsync<AnyOfSpecialityInputModel>(cancellationToken))!
                    .IsAnyOfSpeciality;
            dataService.Set(nameof(isAnyOfSpeciality), isAnyOfSpeciality!);
            if (isAnyOfSpeciality)
            {
                var selectTimePreferencesProvider = serviceProvider.GetRequiredService<SelectTimePreferencesProvider>();
                return await selectTimePreferencesProvider.CreateMenuAsync(cancellationToken);
            }
            
            dataService.TryGet<BasePatientProfileDto>(nameof(BasePatientProfileDto), out var patient);
            dataService.TryGet<MedicalSpeciality>(nameof(MedicalSpeciality), out var speciality);
            doctors = await doctorService.GetBySpecialtyAsync(int.Parse(patient!.LpuId), speciality!.Id);
            
            dataService.Set("ToChoseDoctors", doctors);
        }
        else
        {
            dataService.TryGet("ToChoseDoctors", out doctors);
        }
        
        var commands = doctors!
            .Select(d => new DoctorSelectionCommand(d, serviceProvider))
            .Cast<IMenuCommand>()
            .Append(new BackCommand())
            .ToList();

        var items = commands
            .Select(c =>
                new MenuItem(c.Title, _ => c.ExecuteAsync(cancellationToken)))
            .ToList();

        return new MenuState("Выберите доктора", items, header: HeaderFactorySetup.SetupHeader());
    }

    [InputModel("определения типа поиска")]
    private record AnyOfSpecialityInputModel(
        [property: InputField("Любой врач по специальности (да/нет)")]bool IsAnyOfSpeciality);
}
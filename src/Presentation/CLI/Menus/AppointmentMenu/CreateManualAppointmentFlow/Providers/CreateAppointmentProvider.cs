using Application.DTOs.AppointmentSearchRequest;
using Application.DTOs.Patient;
using CLI.Helpers;
using CLI.Menus.AppointmentMenu.CreateManualAppointmentFlow.Commands;
using Core.Enums;
using Core.Models;
using Microsoft.Extensions.DependencyInjection;
using StatefulMenu.Commands.BuiltIn;
using StatefulMenu.Commands.Interfaces;
using StatefulMenu.Core.Interfaces;
using StatefulMenu.Core.Models;

namespace CLI.Menus.AppointmentMenu.CreateManualAppointmentFlow.Providers;

public class CreateAppointmentProvider(IServiceProvider serviceProvider) : IMenuProvider
{
    public Task<MenuState> CreateMenuAsync(CancellationToken cancellationToken = default)
    {
        var dataService = serviceProvider.GetRequiredService<IDataService>();
        dataService.TryGet<BasePatientProfileDto>(nameof(BasePatientProfileDto), out var patient);
        dataService.TryGet<List<Doctor>>("SelectedDoctors", out var doctors);
        dataService.TryGet<MedicalSpeciality>(nameof(MedicalSpeciality), out var speciality);
        dataService.TryGet<CreateAppointmentSearchRequestDto>(nameof(CreateAppointmentSearchRequestDto),
            out var createAppointmentSearchRequestDto);

        dataService.TryGet<bool>("IsAnyOfSpeciality", out var isAnyOfSpeciality);

        createAppointmentSearchRequestDto = new CreateAppointmentSearchRequestDto
        {
            PatientProfileId = patient!.Id,
            LpuName = patient.LpuShortName,
            Speciality = speciality!.Name,
            DoctorMode = isAnyOfSpeciality
                ? DoctorSelectionMode.AnyOfSpeciality
                : DoctorSelectionMode.SpecificDoctorOrRange,
            DoctorIds = isAnyOfSpeciality ? null : doctors!.Select(d => d.Id).ToList(),
            DoctorNames = isAnyOfSpeciality ? null : doctors!.Select(d => d.Name).ToList(),
            TimePreferencesPresetName = createAppointmentSearchRequestDto!.TimePreferencesPresetName,
            SearchInterval = createAppointmentSearchRequestDto.SearchInterval,
            SpecificStartPoints = createAppointmentSearchRequestDto.SpecificStartPoints,
            MaxDaysToSearch = createAppointmentSearchRequestDto.MaxDaysToSearch,
            ViewOnly = createAppointmentSearchRequestDto.ViewOnly
        };

        var commands = new List<IMenuCommand>
        {
            new CreateAppointmentCommand(createAppointmentSearchRequestDto, serviceProvider),
            serviceProvider.GetRequiredService<BackCommand>()
        };

        var items = commands
            .Select(c =>
                new MenuItem(c.Title, _ => c.ExecuteAsync(cancellationToken)))
            .ToList();

        return Task.FromResult(new MenuState("Подтвердите создание запроса", items,
            header: HeaderFactorySetup.SetupHeader()));
    }
}
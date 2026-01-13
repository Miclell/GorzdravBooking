using Application.DTOs.AppointmentSearchRequest;
using Application.DTOs.Patient;
using Application.DTOs.TimePreferences;
using Application.DTOs.UseCases;
using CLI.Helpers;
using CLI.Menus.AppointmentMenu.CreateReferralAppointmentFlow.Commands;
using Core.Enums;
using Core.Models.Referral;
using Microsoft.Extensions.DependencyInjection;
using StatefulMenu.Commands.BuiltIn;
using StatefulMenu.Commands.Interfaces;
using StatefulMenu.Core.Interfaces;
using StatefulMenu.Core.Models;

namespace CLI.Menus.AppointmentMenu.CreateReferralAppointmentFlow.Providers;

public class CreateReferralAppointmentProvider(IServiceProvider serviceProvider) : IMenuProvider
{
    public Task<MenuState> CreateMenuAsync(CancellationToken cancellationToken = default)
    {
        var dataService = serviceProvider.GetRequiredService<IDataService>();
        dataService.TryGet<ReferralValidationRequest>(nameof(ReferralValidationRequest), out var referralValidationRequest);
        dataService.TryGet<BasePatientProfileDto>(nameof(BasePatientProfileDto), out var patient);
        dataService.TryGet<bool>("IsAnyOfSpeciality", out var isAnyOfSpeciality);
        dataService.TryGet<ReferralSpeciality>(nameof(ReferralSpeciality), out var speciality);
        dataService.TryGet<List<ReferralDoctor>>("SelectedDoctors", out var doctors);
        dataService.TryGet<CreateAppointmentSearchRequestDto>(nameof(CreateAppointmentSearchRequestDto),
            out var createAppointmentSearchRequestDto);
        
        createAppointmentSearchRequestDto = new CreateAppointmentSearchRequestDto
        {
            ReferralNumber = referralValidationRequest!.ReferralNumber,
            PatientProfileId = patient!.Id,
            LpuName = patient.LpuShortName,
            Speciality = isAnyOfSpeciality ? "" : speciality!.Name,
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
            new CreateReferralAppointmentCommand(createAppointmentSearchRequestDto, serviceProvider),
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
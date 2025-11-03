using Application.DTOs.AppointmentSearchRequest;
using Application.DTOs.Patient;
using CLI.Menus.AppointmentMenu.CreateAppointmentFlow.Commands;
using Core.Models;
using Microsoft.Extensions.DependencyInjection;
using StatefulMenu.Commands.BuiltIn;
using StatefulMenu.Commands.Interfaces;
using StatefulMenu.Core.Interfaces;
using StatefulMenu.Core.Models;

namespace CLI.Menus.AppointmentMenu.CreateAppointmentFlow.Providers;

public class CreateAppointmentProvider(IServiceProvider serviceProvider) : IMenuProvider
{
    public Task<MenuState> CreateMenuAsync(CancellationToken cancellationToken = default)
    {
        var dataService = serviceProvider.GetRequiredService<IDataService>();
        dataService.TryGet<BasePatientProfileDto>(nameof(BasePatientProfileDto), out var patient);
        dataService.TryGet<Doctor>(nameof(Doctor), out var doctor);
        dataService.TryGet<CreateAppointmentSearchRequestDto>(nameof(CreateAppointmentSearchRequestDto),
            out var createAppointmentSearchRequestDto);

        createAppointmentSearchRequestDto = new CreateAppointmentSearchRequestDto()
        {
            PatientProfileId = patient!.Id,
            LpuName = patient.LpuShortName,
            DoctorId = doctor!.Id,
            DoctorName = doctor.Name,
            SearchInterval = createAppointmentSearchRequestDto!.SearchInterval,
            SpecificStartPoints = createAppointmentSearchRequestDto.SpecificStartPoints,
            TimePreferencesPresetName = createAppointmentSearchRequestDto.TimePreferencesPresetName,
            ViewOnly = createAppointmentSearchRequestDto.ViewOnly,
            MaxDaysToSearch = createAppointmentSearchRequestDto.MaxDaysToSearch
        };
        
        var commands = new List<IMenuCommand>()
        {
            new CreateAppointmentCommand(createAppointmentSearchRequestDto, serviceProvider),
            serviceProvider.GetRequiredService<BackCommand>()
        };
        
        var items = commands
            .Select(c => 
                new MenuItem(c.Title, _ => c.ExecuteAsync(cancellationToken)))
            .ToList();

        return Task.FromResult(new MenuState("Подтвердите создание запроса", items));
    }
}
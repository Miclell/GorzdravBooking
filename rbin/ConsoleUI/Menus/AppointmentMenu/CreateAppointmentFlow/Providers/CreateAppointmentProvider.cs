using Application.Services;
using Application.UseCases.AppointmentSearchRequest;
using Application.UseCases.AppointmentSearchRequest.Commands;
using Application.UseCases.TimePreferences;
using ConsoleUI.Core;
using ConsoleUI.Core.Interfaces;
using ConsoleUI.Menus.AppointmentMenu.CreateAppointmentFlow.Commands;
using ConsoleUI.Menus.AppointmentMenu.TimePreferencesMenu.Commands;
using ConsoleUI.Services;
using ConsoleUI.Services.Interfaces;
using Core.Entities;
using Core.Models;
using Microsoft.Extensions.DependencyInjection;

namespace ConsoleUI.Menus.AppointmentMenu.CreateAppointmentFlow.Providers;

public class CreateAppointmentProvider(IServiceProvider serviceProvider) : IMenuProvider
{
    public async Task<MenuState> CreateMenuAsync()
    {
        var navigation = serviceProvider.GetRequiredService<NavigationService>();

        var doctor = navigation.GetSharedData<Doctor>(nameof(Doctor));
        var patient = navigation.GetSharedData<PatientProfile>(nameof(PatientProfile));

        var inputService = serviceProvider.GetRequiredService<IConsoleInputService>();
        var createAppointmentSearchRequestCommand =
            await inputService.ReadModelAsync<CreateAppointmentSearchRequestCommand>();

        createAppointmentSearchRequestCommand = createAppointmentSearchRequestCommand! with
        {
            PatientProfileId = patient.Id,
            LpuName = patient.LpuShortName,
            DoctorId = doctor.Id,
            DoctorName = doctor.Name,
            SpecificStartPoints = SpecificStartPointsInputService.ReadModel()
        };

        navigation.SetSharedData(nameof(CreateAppointmentSearchRequestCommand), createAppointmentSearchRequestCommand);

        var appSettingsService = serviceProvider.GetService<AppSettingsService>();

        var getTimePreferencesUseCase = serviceProvider.GetRequiredService<GetTimePreferencesUseCase>();
        var timePreferences =
            await getTimePreferencesUseCase.ExecuteAsync(await appSettingsService.GetDefaultUserIdAsync());

        var commands = timePreferences.Select(tp => new SelectTimePreferencesCommand(tp, serviceProvider))
            .Cast<IMenuCommand>()
            .Append(new BackCommand())
            .ToList();

        return new MenuState()
        {
            Title = "Выберите временной пресет",
            Commands = commands
        };
    }
}
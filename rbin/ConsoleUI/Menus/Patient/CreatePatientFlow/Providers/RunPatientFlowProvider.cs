using Application.Services;
using Application.UseCases.Patient;
using Application.UseCases.Patient.Commands;
using ConsoleUI.Core;
using ConsoleUI.Core.Interfaces;
using ConsoleUI.Menus.Patient.CreatePatientFlow.Commands;
using ConsoleUI.Menus.Providers;
using ConsoleUI.Services;
using ConsoleUI.Services.Interfaces;
using Core.Interfaces.Services;
using Core.Models;
using Microsoft.Extensions.DependencyInjection;

namespace ConsoleUI.Menus.Patient.CreatePatientFlow.Providers;

public class RunPatientFlowProvider(IServiceProvider serviceProvider) : IMenuProvider
{
    public async Task<MenuState> CreateMenuAsync()
    {
        var navigation = serviceProvider.GetRequiredService<NavigationService>();

        /*var inputService = serviceProvider.GetRequiredService<IConsoleInputService>();
        var createPatientCommand = await inputService.ReadModelAsync<CreatePatientCommand>();
        var patientIdSearchRequest = new PatientIdSearchRequest()
        {
            LpuId = navigation.GetSharedData<Lpu>(nameof(Lpu)).Id.ToString(),
            LastName = createPatientCommand.PatientLastName,
            FirstName = createPatientCommand.PatientFirstName,
            MiddleName = createPatientCommand.PatientMiddleName,
            BirthDate = createPatientCommand.PatientBirthdate
        };
        
        var appSettingService = serviceProvider.GetRequiredService<AppSettingsService>();
        var patientService = serviceProvider.GetRequiredService<IPatientService>();
        createPatientCommand = createPatientCommand! with
        {
            UserId = await appSettingService.GetDefaultUserIdAsync(),
            LpuId = navigation.GetSharedData<Lpu>(nameof(Lpu)).Id.ToString(),
            PatientId = await patientService.GetPatientIdAsync(patientIdSearchRequest)
        };

        var createPatientUseCase = serviceProvider.GetRequiredService<CreatePatientUseCase>();
        await createPatientUseCase.ExecuteAsync(createPatientCommand);*/

        return new MenuState()
        {
            Title = "Выбор района",
            Commands = [new RunPatientFlowCommand(serviceProvider)]
        };
    }
}
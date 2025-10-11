using Application.Services;
using Application.UseCases.Patient;
using ConsoleUI.Core;
using ConsoleUI.Core.Interfaces;
using ConsoleUI.Menus.Patient.CreatePatientFlow.Providers;
using ConsoleUI.Menus.Patient.Providers;
using ConsoleUI.Services;
using ConsoleUI.Services.Interfaces;
using Core.Interfaces.Services;
using Core.Models;
using Microsoft.Extensions.DependencyInjection;

namespace ConsoleUI.Menus.Patient.CreatePatientFlow.Commands;

public class CreatePatientCommand(IServiceProvider serviceProvider) : IMenuCommand
{
    public string Title { get; } = "Ввести данные пациента";
    public async Task<MenuState?> ExecuteAsync()
    {
        var navigation = serviceProvider.GetRequiredService<NavigationService>();
        
        var inputService = serviceProvider.GetRequiredService<IConsoleInputService>();
        var createPatientCommand = await inputService.ReadModelAsync<Application.UseCases.Patient.Commands.CreatePatientCommand>();
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
            LpuShortName = navigation.GetSharedData<Lpu>(nameof(Lpu)).LpuShortName,
            LpuAddress = navigation.GetSharedData<Lpu>(nameof(Lpu)).Address,
            PatientId = await patientService.GetPatientIdAsync(patientIdSearchRequest)
        };

        var createPatientUseCase = serviceProvider.GetRequiredService<CreatePatientUseCase>();
        await createPatientUseCase.ExecuteAsync(createPatientCommand);

        Console.WriteLine("Пациент успешно создан! Нажмите клавишу чтобы продолжить");
        Console.ReadKey();
        
        var showPatientMenuProvider = serviceProvider.GetRequiredService<ShowPatientMenuProvider>();
        
        return await showPatientMenuProvider.CreateMenuAsync();
    }
}
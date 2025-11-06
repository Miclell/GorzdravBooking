using Application.DTOs.Patient;
using Application.Services.Interfaces;
using Core.Interfaces.Services;
using Core.Models;
using Microsoft.Extensions.DependencyInjection;
using StatefulMenu.Commands.Interfaces;
using StatefulMenu.Core.Interfaces;
using StatefulMenu.Core.Models;

namespace CLI.Menus.PatientMenu.CreatePatientFlow.Commands;

// TODO  Ошибка при получении id пациента: реализовать обработку
public class CreatePatientCommand(IServiceProvider serviceProvider) : IMenuCommand
{
    public string Title { get; } = "Ввести данные пациента";

    public async Task<MenuResult> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        var inputService = serviceProvider.GetRequiredService<IConsoleInputService>();
        var dataService = serviceProvider.GetRequiredService<IDataService>();
        var patientService = serviceProvider.GetRequiredService<IPatientService>();

        var createPatientDto = await inputService.ReadModelAsync<CreatePatientDto>(cancellationToken);
        dataService.TryGet<Lpu>(nameof(Lpu), out var lpu);

        var externalPatientService = serviceProvider.GetRequiredService<IExternalPatientService>();
        var patientIdSearchRequest = new PatientIdSearchRequest
        {
            LpuId = lpu!.Id.ToString(),
            LastName = createPatientDto!.PatientLastName,
            FirstName = createPatientDto.PatientFirstName,
            MiddleName = createPatientDto.PatientMiddleName,
            BirthDate = createPatientDto.PatientBirthdate
        };

        var appSettingService = serviceProvider.GetRequiredService<IAppSettingsService>();
        createPatientDto = createPatientDto! with
        {
            UserId = await appSettingService.GetDefaultUserIdAsync(),
            LpuId = lpu!.Id.ToString(),
            LpuShortName = lpu!.LpuShortName,
            LpuAddress = lpu!.Address,
            PatientId = await externalPatientService.GetPatientIdAsync(patientIdSearchRequest)
        };

        await patientService.Create(createPatientDto!, cancellationToken);

        Console.WriteLine("Пациент успешно создан! Нажмите клавишу чтобы продолжить..");
        Console.ReadKey();

        dataService.Remove(nameof(Lpu));

        var mainMenuProvider = serviceProvider.GetRequiredService<MainMenuProvider>();
        return MenuResult.Push(mainMenuProvider.CreateMenuAsync(cancellationToken).Result);
    }
}
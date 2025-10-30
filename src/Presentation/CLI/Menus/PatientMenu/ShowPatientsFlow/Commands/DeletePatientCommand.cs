using Application.DTOs.Patient;
using Application.Services.Interfaces;
using StatefulMenu.Commands.Interfaces;
using StatefulMenu.Core.Interfaces;
using StatefulMenu.Core.Models;

namespace CLI.Menus.PatientMenu.ShowPatientsFlow.Commands;

public class DeletePatientCommand(IDataService dataService, IPatientService patientService, MainMenuProvider mainMenuProvider) : IMenuCommand
{
    public string Title { get; } = "Удалить пациента";
    public async Task<MenuResult> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        dataService.TryGet<BasePatientProfileDto>(nameof(BasePatientProfileDto), out var patient);

        await patientService.Delete(patient!.Id, cancellationToken);

        dataService.Remove(nameof(BasePatientProfileDto));

        Console.WriteLine($"Пациент {patient.LpuShortName} | {patient.PatientFirstName} {patient.PatientLastName} успешно удален!\nНажмите любую клавишу для продолжения..");
        Console.ReadKey();
        
        return MenuResult.Push(mainMenuProvider.CreateMenuAsync(cancellationToken).Result);
    }
}
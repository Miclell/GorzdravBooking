using Application.UseCases.Patient;
using ConsoleUI.Core;
using ConsoleUI.Core.Interfaces;
using ConsoleUI.Menus.Commands;
using ConsoleUI.Menus.Patient.Command;
using ConsoleUI.Services;
using Core.Entities;
using Microsoft.Extensions.DependencyInjection;

namespace ConsoleUI.Menus.Patient.ShowPatientsFlow.Providers;

public class DeletePatientProvider(IServiceProvider serviceProvider) : IMenuProvider
{
    public async Task<MenuState> CreateMenuAsync()
    {
        var navigation = serviceProvider.GetRequiredService<NavigationService>();
        var patient = navigation.GetSharedData<PatientProfile>(nameof(PatientProfile));

        var deletePatientUseCase = serviceProvider.GetRequiredService<DeletePatientUseCase>();
        await deletePatientUseCase.Execute(patient.Id);
        
        navigation.DeleteSharedData(nameof(PatientProfile));

        Console.WriteLine($"Пациент {patient.LpuShortName} | {patient.PatientFirstName} {patient.PatientLastName} успешно удален!\nНажмите любую клавишу для продолжения..");
        Console.ReadKey();
        
        return new MenuState()
        {
            Title = "Редактирование пациента",
            Commands = 
            [
                navigation.CreateCommand<ShowPatientMenuCommand>()
            ]
        };
    }
}
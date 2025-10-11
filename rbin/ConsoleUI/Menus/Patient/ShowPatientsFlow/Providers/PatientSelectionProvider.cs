using ConsoleUI.Core;
using ConsoleUI.Core.Interfaces;
using ConsoleUI.Menus.Patient.ShowPatientsFlow.Commands;
using ConsoleUI.Services;
using Core.Entities;
using Microsoft.Extensions.DependencyInjection;

namespace ConsoleUI.Menus.Patient.ShowPatientsFlow.Providers;

public class PatientSelectionProvider(IServiceProvider serviceProvider) : IMenuProvider
{
    public Task<MenuState> CreateMenuAsync()
    {
        var navigation = serviceProvider.GetRequiredService<NavigationService>();
        
        return Task.FromResult(new MenuState()
        {
            Title = "Редактирование пациента",
            Commands = 
            [
                navigation.CreateCommand<DeletePatientCommand>(),
                new BackCommand()
            ]
        });
    }
}
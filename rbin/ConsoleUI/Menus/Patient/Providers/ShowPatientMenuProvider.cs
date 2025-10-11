using ConsoleUI.Core;
using ConsoleUI.Core.Interfaces;
using ConsoleUI.Menus.Patient.Command;
using ConsoleUI.Menus.Patient.CreatePatientFlow.Commands;
using ConsoleUI.Menus.Patient.ShowPatientsFlow.Commands;
using ConsoleUI.Services;
using Microsoft.Extensions.DependencyInjection;

namespace ConsoleUI.Menus.Patient.Providers;

public class ShowPatientMenuProvider(IServiceProvider serviceProvider) : IMenuProvider
{
    public async Task<MenuState> CreateMenuAsync()
    {
        var navigation = serviceProvider.GetRequiredService<NavigationService>();
        
        return new MenuState()
        {
            Title = "Пациент",
            Commands = 
            [
                navigation.CreateCommand<ShowPatientsCommand>(),
                navigation.CreateCommand<RunPatientFlowCommand>(),
                navigation.CreateCommand<BackCommand>()
            ]
        };
    }
}
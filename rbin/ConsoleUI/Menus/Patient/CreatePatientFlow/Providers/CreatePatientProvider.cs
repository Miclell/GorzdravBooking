using ConsoleUI.Core;
using ConsoleUI.Core.Interfaces;
using ConsoleUI.Menus.Patient.CreatePatientFlow.Commands;
using ConsoleUI.Services;
using Microsoft.Extensions.DependencyInjection;

namespace ConsoleUI.Menus.Patient.CreatePatientFlow.Providers;

public class CreatePatientProvider(IServiceProvider serviceProvider) : IMenuProvider
{
    public async Task<MenuState> CreateMenuAsync()
    {
        return new MenuState
        {
            Title = "Ввод данных пациента",
            Commands = 
            [
                new CreatePatientCommand(serviceProvider),
                new BackCommand()
            ]
        };
    }
}
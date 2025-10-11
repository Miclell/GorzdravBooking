using System.Text;
using Application.Services;
using Application.UseCases.Patient;
using ConsoleUI.Core;
using ConsoleUI.Core.Interfaces;
using ConsoleUI.Menus.Patient.ShowPatientsFlow.Commands;
using Core.Entities;
using Microsoft.Extensions.DependencyInjection;

namespace ConsoleUI.Menus.Patient.ShowPatientsFlow.Providers;

public class ShowPatientsProvider(IServiceProvider serviceProvider) : IMenuProvider
{
    public async Task<MenuState> CreateMenuAsync()
    {
        var appSettingService = serviceProvider.GetRequiredService<AppSettingsService>();
        var getPatientsByUserUseCase = serviceProvider.GetRequiredService<GetPatientsByUserUseCase>();
        var patients = await getPatientsByUserUseCase.ExecuteAsync(await appSettingService.GetDefaultUserIdAsync());
        
        var commands = patients
            .Select(p => new PatientSelectionCommand(p, serviceProvider))
            .Cast<IMenuCommand>()
            .Append(new BackCommand())
            .ToList();

        return new MenuState()
        {
            Title = "Выберите пациента",
            Commands = commands
        };
    }
}
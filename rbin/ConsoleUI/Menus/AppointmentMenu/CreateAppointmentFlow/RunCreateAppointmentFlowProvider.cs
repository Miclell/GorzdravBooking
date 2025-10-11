using Application.Services;
using Application.UseCases.Patient;
using ConsoleUI.Core;
using ConsoleUI.Core.Interfaces;
using ConsoleUI.Menus.AppointmentMenu.CreateAppointmentFlow.Commands;
using Microsoft.Extensions.DependencyInjection;

namespace ConsoleUI.Menus.AppointmentMenu.CreateAppointmentFlow;

public class RunCreateAppointmentFlowProvider(IServiceProvider serviceProvider) : IMenuProvider
{
    // Кого вы хотите записать
    // SelectPatientCommand -> SelectPatientProvider (shared) -> CreateAppointmentSearchRequestCommand -> SpecialitySelectionProvider (shared)
    public async Task<MenuState> CreateMenuAsync()
    {
        var appSettings = serviceProvider.GetRequiredService<AppSettingsService>();
        
        var getPatientsByUserUseCase = serviceProvider.GetRequiredService<GetPatientsByUserUseCase>();
        var patients = await getPatientsByUserUseCase.ExecuteAsync(await appSettings.GetDefaultUserIdAsync());

        var commands = patients.Select(p => new SelectPatientCommand(p, serviceProvider))
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
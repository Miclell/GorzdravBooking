using Application.Services;
using Application.UseCases.Patient;
using Application.UseCases.TimePreferences;
using Application.UseCases.TimePreferences.Commands;
using ConsoleUI.Core;
using ConsoleUI.Core.Interfaces;
using ConsoleUI.Menus.Providers;
using ConsoleUI.Services;
using ConsoleUI.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace ConsoleUI.Menus.AppointmentMenu.TimePreferencesMenu.Providers;

public class CreateTimePreferencesProvider(IServiceProvider serviceProvider) : IMenuProvider
{
    public async Task<MenuState> CreateMenuAsync()
    {
        var appSetting = serviceProvider.GetRequiredService<AppSettingsService>();
        var getPatientsByUserUseCase = serviceProvider.GetRequiredService<GetPatientsByUserUseCase>();
        var patients = await getPatientsByUserUseCase.ExecuteAsync(await appSetting.GetDefaultUserIdAsync());
        Console.WriteLine(patients.First().UserId);
        var inputService = new TimePreferencesInputService();
        var createTimePreferencesCommand = inputService.ReadModel(patients.First().Id);
        
        var createTimePreferencesUseCase = serviceProvider.GetRequiredService<CreateTimePreferencesUseCase>();
        await createTimePreferencesUseCase.ExecuteAsync(createTimePreferencesCommand);

        Console.WriteLine("Пресет успешно создан! Нажмите клавишу для продолжения..");
        Console.ReadKey();

        var mainMenuProvider = serviceProvider.GetRequiredService<IMenuProvider>();
        return await mainMenuProvider.CreateMenuAsync();
    }
}
using Application.UseCases.Appointment;
using ConsoleUI.Core;
using ConsoleUI.Core.Interfaces;
using ConsoleUI.Menus.Providers;
using ConsoleUI.Services;
using Core.Interfaces.Services;
using Core.Models;
using Microsoft.Extensions.DependencyInjection;
using Appointment = Core.Entities.Appointment;

namespace ConsoleUI.Menus.AppointmentMenu.ShowAppointmentFlow.ShowUpcomingAppointmentsFlow.Providers;

public class AppointmentCancelProvider(IServiceProvider serviceProvider) : IMenuProvider
{
    public async Task<MenuState> CreateMenuAsync()
    {
        var navigation = serviceProvider.GetRequiredService<NavigationService>();
        var appointment = navigation.GetSharedData<Appointment>(nameof(Appointment));

        var deleteAppointmentUseCase = serviceProvider.GetRequiredService<DeleteAppointmentUseCase>();
        var isSuccess = await deleteAppointmentUseCase.ExecuteAsync(appointment);

        Console.WriteLine(isSuccess ? "Запись успешно отменена!" : "Ошибка при отмене записи!");
        
        navigation.DeleteSharedData(nameof(Appointment));

        var mainMenuProvider = serviceProvider.GetRequiredService<MainMenuProvider>();
        return await mainMenuProvider.CreateMenuAsync();
    }
}
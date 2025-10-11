using Application.UseCases.AppointmentSearchRequest;
using ConsoleUI.Core;
using ConsoleUI.Core.Interfaces;
using ConsoleUI.Menus.Providers;
using ConsoleUI.Services;
using Core.Entities;
using Microsoft.Extensions.DependencyInjection;

namespace ConsoleUI.Menus.AppointmentMenu.ShowAppointmentFlow.ShowActiveAppointmentsFlow.Commands;

public class DeleteAppointmentCommand(IServiceProvider serviceProvider) : IMenuCommand
{
    public string Title { get; } = "Удалить запрос";
    public async Task<MenuState?> ExecuteAsync()
    {
        var navigation = serviceProvider.GetRequiredService<NavigationService>();
        var appointmentSearchRequest = navigation.GetSharedData<AppointmentSearchRequest>(nameof(AppointmentSearchRequest));

        var deleteAppointmentSearchRequest = serviceProvider.GetRequiredService<DeleteAppointmentSearchRequestUseCase>();
        await deleteAppointmentSearchRequest.ExecuteAsync(appointmentSearchRequest.Id);

        Console.WriteLine("Запрос на запись успешно удален. Нажмите клавишу чтобы продолжить..");
        Console.ReadKey();

        var mainMenuProvider = serviceProvider.GetRequiredService<MainMenuProvider>();
        return await mainMenuProvider.CreateMenuAsync();
    }
}
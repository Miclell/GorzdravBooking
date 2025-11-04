using Application.DTOs.AppointmentSearchRequest;
using Application.Services.Interfaces;
using StatefulMenu.Commands.Interfaces;
using StatefulMenu.Core.Interfaces;
using StatefulMenu.Core.Models;

namespace CLI.Menus.AppointmentMenu.ShowActiveAppointmentsFlow.Commands;

public class DeleteAppointmentSearchRequestCommand(
    IDataService dataService,
    IAppointmentSearchRequestService appointmentSearchRequestService,
    MainMenuProvider mainMenuProvider) : IMenuCommand
{
    public string Title { get; } = "Удалить";

    public async Task<MenuResult> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        dataService.TryGet<AppointmentSearchRequestDto>(nameof(AppointmentSearchRequestDto), out var appointment);

        await appointmentSearchRequestService.DeleteAsync(appointment!.Id, cancellationToken);

        dataService.Remove(nameof(AppointmentSearchRequestDto));

        Console.WriteLine($"Запрос " +
                          $"{appointment.LpuName} " +
                          $"{appointment.DoctorName} |" +
                          $"{appointment.TimePreferencesPresetName} успешно удален!\n" +
                          $"Нажмите любую клавишу для продолжения..");
        Console.ReadKey();

        return MenuResult.Push(mainMenuProvider.CreateMenuAsync(cancellationToken).Result);
    }
}
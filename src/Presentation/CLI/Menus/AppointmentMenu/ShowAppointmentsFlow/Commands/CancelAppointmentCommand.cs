using Application.DTOs.Appointment;
using Application.Services.Interfaces;
using StatefulMenu.Commands.Interfaces;
using StatefulMenu.Core.Interfaces;
using StatefulMenu.Core.Models;

namespace CLI.Menus.AppointmentMenu.ShowActiveAppointmentsFlow.Commands;

public class CancelAppointmentCommand(
    IDataService dataService,
    IAppointmentService appointmentService,
    MainMenuProvider mainMenuProvider) : IMenuCommand
{
    public string Title { get; } = "Удалить";

    public async Task<MenuResult> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        dataService.TryGet<AppointmentListItemDto>(nameof(AppointmentListItemDto), out var appointment);

        await appointmentService.DeleteAsync(appointment!.Id, cancellationToken);

        dataService.Remove(nameof(AppointmentListItemDto));

        Console.WriteLine($"Запись " +
                          $"{appointment!.LpuShortName} " +
                          $"{appointment.PatientFullName} | " +
                          $"{appointment.VisitStart} успешно отменена!\n" +
                          $"Нажмите любую клавишу для продолжения..");
        Console.ReadKey();

        return MenuResult.Push(mainMenuProvider.CreateMenuAsync(cancellationToken).Result);
    }
}
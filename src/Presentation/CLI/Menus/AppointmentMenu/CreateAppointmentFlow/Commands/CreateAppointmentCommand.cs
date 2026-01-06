using Application.DTOs.AppointmentSearchRequest;
using Application.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using StatefulMenu.Commands.Interfaces;
using StatefulMenu.Core.Models;

namespace CLI.Menus.AppointmentMenu.CreateAppointmentFlow.Commands;

public class CreateAppointmentCommand(
    CreateAppointmentSearchRequestDto createAppointmentSearchRequestDto,
    IServiceProvider serviceProvider) : IMenuCommand
{
    public string Title { get; } = $"{createAppointmentSearchRequestDto.LpuName} " +
                                   $"{createAppointmentSearchRequestDto.DoctorNames} | " +
                                   $"{createAppointmentSearchRequestDto.TimePreferencesPresetName}";

    public async Task<MenuResult> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        var appointmentSearchRequestService = serviceProvider.GetRequiredService<IAppointmentSearchRequestService>();
        var result =
            await appointmentSearchRequestService.CreateAsync(createAppointmentSearchRequestDto, cancellationToken);

        if (result.IsSuccess)
        {
            Console.WriteLine("Запрос успешно создан!\nНажмите любую клавишу чтобы продолжить..");
            Console.ReadLine();
        }
        else
        {
            Console.WriteLine("Ошибка при создании запроса!");
        }

        var mainMenuProvider = serviceProvider.GetRequiredService<MainMenuProvider>();
        return MenuResult.Push(mainMenuProvider.CreateMenuAsync(cancellationToken).Result);
    }
}
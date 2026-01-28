using Application.DTOs.AppointmentSearchRequest;
using Application.DTOs.Patient;
using Application.DTOs.TimePreferences;
using Application.DTOs.UseCases;
using Application.Extensions;
using Application.Services.Interfaces;
using Core.Models.Referral;
using Microsoft.Extensions.DependencyInjection;
using StatefulMenu.Commands.Interfaces;
using StatefulMenu.Core.Interfaces;
using StatefulMenu.Core.Models;

namespace CLI.Menus.AppointmentMenu.CreateReferralAppointmentFlow.Commands;

public class CreateReferralAppointmentCommand(
    CreateAppointmentSearchRequestDto createAppointmentSearchRequestDto,
    IServiceProvider serviceProvider) : IMenuCommand
{
    public string Title { get; } = createAppointmentSearchRequestDto.GetDisplayTitle();

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

        DisposeFlowData(serviceProvider.GetRequiredService<IDataService>());

        var mainMenuProvider = serviceProvider.GetRequiredService<MainMenuProvider>();
        return MenuResult.Push(mainMenuProvider.CreateMenuAsync(cancellationToken).Result);
    }

    private static void DisposeFlowData(IDataService dataService)
    {
        dataService.Remove(nameof(ReferralValidationRequest));
        dataService.Remove(nameof(BasePatientProfileDto));
        dataService.Remove("IsAnyOfSpeciality");
        dataService.Remove(nameof(ReferralSpeciality));
        dataService.Remove("IsAnotherDoctor");
        dataService.Remove("ToChoseDoctors");
        dataService.Remove("SelectedDoctors");
        dataService.Remove(nameof(TimePreferencesPresetDto));
        dataService.Remove(nameof(CreateAppointmentSearchRequestDto));
    }
}
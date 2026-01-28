using Application.DTOs.Patient;
using Application.DTOs.UseCases;
using Application.Services.Interfaces;
using Application.UseCases;
using CLI.Helpers;
using CLI.Menus.AppointmentMenu.CreateReferralAppointmentFlow.Commands;
using Microsoft.Extensions.DependencyInjection;
using StatefulMenu.Commands.BuiltIn;
using StatefulMenu.Commands.Interfaces;
using StatefulMenu.Core.Attributes;
using StatefulMenu.Core.Interfaces;
using StatefulMenu.Core.Models;

namespace CLI.Menus.AppointmentMenu.CreateReferralAppointmentFlow.Providers;

public class ReferralInputProvider(
    IAppSettingsService appSettingsService,
    IConsoleInputService inputService,
    IServiceProvider serviceProvider) : IMenuProvider
{
    public async Task<MenuState> CreateMenuAsync(CancellationToken cancellationToken = default)
    {
        var referral = await inputService.ReadModelAsync<ReferralValidationRequest>(cancellationToken);
        referral = referral! with
        {
            UserId = await appSettingsService.GetDefaultUserIdAsync()
        };

        var useCase = serviceProvider.GetRequiredService<ProcessReferralUseCase>();
        var result = await useCase.Execute(referral, cancellationToken);

        if (result.IsSuccess)
        {
            var dataService = serviceProvider.GetRequiredService<IDataService>();

            dataService.Set(nameof(ReferralValidationRequest), referral);
            dataService.Set(nameof(BasePatientProfileDto), result.Value.PatientProfile);

            var isAnyOfSpeciality = (await inputService.ReadModelAsync<AnyOfSpecialityInputModel>(cancellationToken))!
                .IsAnyOfSpeciality;
            if (isAnyOfSpeciality)
            {
                dataService.Set("IsAnyOfSpeciality", isAnyOfSpeciality);
                var timePreferenceSelectionProvider =
                    serviceProvider.GetRequiredService<TimePreferenceSelectionProvider>();
                return await timePreferenceSelectionProvider.CreateMenuAsync(cancellationToken);
            }

            if (result.Value.Specialities.FirstOrDefault() == null)
            {
                Console.WriteLine("В данный момент нет списка врачей, " +
                                  "поэтому будет выбран режим \"Любой врач по " +
                                  "специальности!\"" +
                                  "\nНажмите любую клавишу для продолжения..");
                Console.ReadKey();
                dataService.Set("IsAnyOfSpeciality", true);
                var timePreferenceSelectionProvider =
                    serviceProvider.GetRequiredService<TimePreferenceSelectionProvider>();
                return await timePreferenceSelectionProvider.CreateMenuAsync(cancellationToken);
            }

            dataService.Set("IsAnyOfSpeciality", isAnyOfSpeciality);

            var commands = result.Value.Specialities
                .Select(s => new SpecialitySelectionCommand(s, serviceProvider))
                .Cast<IMenuCommand>()
                .Append(new BackCommand())
                .ToList();

            var items = commands
                .Select(c =>
                    new MenuItem(c.Title, _ => c.ExecuteAsync(cancellationToken)))
                .ToList();

            return new MenuState("Выберите специальность", items, header: HeaderFactorySetup.SetupHeader());
        }

        Console.WriteLine(result.Error.Description + "Нажмите любую клавишу для продолжения..");
        Console.ReadLine();

        var mainMenuProvider = serviceProvider.GetRequiredService<MainMenuProvider>();
        return await mainMenuProvider.CreateMenuAsync(cancellationToken);
    }

    [InputModel("определения типа поиска")]
    private record AnyOfSpecialityInputModel(
        [property: InputField("Любой врач по специальности (да/нет)")]
        bool IsAnyOfSpeciality);
}
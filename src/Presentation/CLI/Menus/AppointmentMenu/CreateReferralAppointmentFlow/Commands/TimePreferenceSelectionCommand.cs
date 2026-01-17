using Application.DTOs.AppointmentSearchRequest;
using Application.DTOs.TimePreferences;
using CLI.Extensions.Converters;
using CLI.Menus.AppointmentMenu.CreateReferralAppointmentFlow.Providers;
using Microsoft.Extensions.DependencyInjection;
using StatefulMenu.Commands.Interfaces;
using StatefulMenu.Core.Attributes;
using StatefulMenu.Core.Interfaces;
using StatefulMenu.Core.Models;

namespace CLI.Menus.AppointmentMenu.CreateReferralAppointmentFlow.Commands;

public class TimePreferenceSelectionCommand(
    TimePreferencesPresetDto timePreferencesPresetDto,
    IServiceProvider serviceProvider) : IMenuCommand
{
    public string Title { get; } = $"{timePreferencesPresetDto.Name}";

    public async Task<MenuResult> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        var dataService = serviceProvider.GetRequiredService<IDataService>();
        dataService.Set(nameof(TimePreferencesPresetDto), timePreferencesPresetDto);

        var inputService = serviceProvider.GetRequiredService<IConsoleInputService>();
        var createAppointmentSearchRequestDto =
            await inputService.ReadModelAsync<CreateAppointmentSearchRequestDto>(cancellationToken);

        createAppointmentSearchRequestDto!.TimePreferencesPresetName = timePreferencesPresetDto.Name;
        createAppointmentSearchRequestDto.SpecificStartPoints =
            (await inputService.ReadModelAsync<SpecificStartPointsInputModel>(cancellationToken))!.SpecificStartPoints;
        dataService.Set(nameof(CreateAppointmentSearchRequestDto), createAppointmentSearchRequestDto);

        var createReferralAppointmentProvider = serviceProvider.GetRequiredService<CreateReferralAppointmentProvider>();
        return MenuResult.Push(await createReferralAppointmentProvider.CreateMenuAsync(cancellationToken));
    }

    [InputModel("стартовых точек")] // TODO вынести главный рекорд
    private record SpecificStartPointsInputModel(
        [property: InputField("Введите стартовые точки HH:mm через ;",
            IsRequired = false,
            Converters = [typeof(ListDateTimeConverter)])]
        List<DateTime>? SpecificStartPoints);
}
using System.Globalization;
using Application.DTOs.AppointmentSearchRequest;
using Application.DTOs.TimePreferences;
using CLI.Extensions.Converters;
using CLI.Menus.AppointmentMenu.CreateAppointmentFlow.Providers;
using Microsoft.Extensions.DependencyInjection;
using StatefulMenu.Commands.Interfaces;
using StatefulMenu.Core.Attributes;
using StatefulMenu.Core.Interfaces;
using StatefulMenu.Core.Models;

namespace CLI.Menus.AppointmentMenu.CreateAppointmentFlow.Commands;

public class SelectTimePreferencesCommand(
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
        createAppointmentSearchRequestDto.SpecificStartPoints = (await inputService.ReadModelAsync<SpecificStartPointsInputModel>(cancellationToken))!.SpecificStartPoints;
        dataService.Set(nameof(CreateAppointmentSearchRequestDto), createAppointmentSearchRequestDto);

        var createAppointmentProvider = serviceProvider.GetRequiredService<CreateAppointmentProvider>();
        return MenuResult.Push(await createAppointmentProvider.CreateMenuAsync(cancellationToken));
    }
    
    [InputModel("стартовых точек")]
    private record SpecificStartPointsInputModel(
        [property: InputField("Введите стартовые точки HH:mm через ;", 
            IsRequired = false, 
            Converters = [typeof(ListDateTimeConverter)])]
        List<DateTime>? SpecificStartPoints);
}
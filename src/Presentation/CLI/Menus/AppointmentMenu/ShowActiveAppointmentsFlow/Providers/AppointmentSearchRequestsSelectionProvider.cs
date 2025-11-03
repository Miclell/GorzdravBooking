using Application.DTOs.AppointmentSearchRequest;
using CLI.Menus.AppointmentMenu.ShowActiveAppointmentsFlow.Commands;
using Microsoft.Extensions.DependencyInjection;
using StatefulMenu.Commands.BuiltIn;
using StatefulMenu.Commands.Interfaces;
using StatefulMenu.Core.Interfaces;
using StatefulMenu.Core.Models;

namespace CLI.Menus.AppointmentMenu.ShowActiveAppointmentsFlow.Providers;

public class AppointmentSearchRequestsSelectionProvider(IServiceProvider serviceProvider) : IMenuProvider
{
    public Task<MenuState> CreateMenuAsync(CancellationToken cancellationToken = default)
    {
        var commands = new IMenuCommand[]
        {
            serviceProvider.GetRequiredService<DeleteAppointmentSearchRequestCommand>(),
            serviceProvider.GetRequiredService<BackCommand>()
        };
        
        var items = commands
            .Select(c => 
                new MenuItem(c.Title, _ => c.ExecuteAsync(cancellationToken)))
            .ToList();

        var dataService = serviceProvider.GetRequiredService<IDataService>();
        dataService.TryGet<AppointmentSearchRequestDto>(nameof(AppointmentSearchRequestDto), out var appointment);
        return Task.FromResult(new MenuState($"Выберите действие для запроса " +
                                             $"{appointment!.LpuName}" +
                                             $"{appointment.DoctorName} | " +
                                             $"{appointment.TimePreferencesPresetName}", items));
    }
}
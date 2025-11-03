using CLI.Menus.AppointmentMenu.CreateAppointmentFlow.Providers;
using Core.Models;
using Microsoft.Extensions.DependencyInjection;
using StatefulMenu.Commands.Interfaces;
using StatefulMenu.Core.Interfaces;
using StatefulMenu.Core.Models;

namespace CLI.Menus.AppointmentMenu.CreateAppointmentFlow.Commands;

public class DoctorSelectionCommand(Doctor doctor, IServiceProvider serviceProvider) : IMenuCommand
{
    public string Title { get; } = doctor.Name;
    public async Task<MenuResult> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        var dataService = serviceProvider.GetRequiredService<IDataService>();
        dataService.Set(nameof(Doctor), doctor);

        var selectTimePreferencesProvider = serviceProvider.GetRequiredService<SelectTimePreferencesProvider>();
        return MenuResult.Push(await selectTimePreferencesProvider.CreateMenuAsync(cancellationToken));
    }
}
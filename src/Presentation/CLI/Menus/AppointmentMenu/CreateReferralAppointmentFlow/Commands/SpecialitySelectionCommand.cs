using CLI.Menus.AppointmentMenu.CreateReferralAppointmentFlow.Providers;
using Core.Models.Referral;
using Microsoft.Extensions.DependencyInjection;
using StatefulMenu.Commands.Interfaces;
using StatefulMenu.Core.Interfaces;
using StatefulMenu.Core.Models;

namespace CLI.Menus.AppointmentMenu.CreateReferralAppointmentFlow.Commands;

public class SpecialitySelectionCommand(ReferralSpeciality speciality, IServiceProvider serviceProvider) : IMenuCommand
{
    public string Title { get; } = speciality.Name;
    
    public async Task<MenuResult> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        var dataService = serviceProvider.GetRequiredService<IDataService>();
        dataService.Set(nameof(ReferralSpeciality), speciality);

        var doctorSelectionProvider = serviceProvider.GetRequiredService<DoctorSelectionProvider>();
        return MenuResult.Push(await doctorSelectionProvider.CreateMenuAsync(cancellationToken));
    }
}
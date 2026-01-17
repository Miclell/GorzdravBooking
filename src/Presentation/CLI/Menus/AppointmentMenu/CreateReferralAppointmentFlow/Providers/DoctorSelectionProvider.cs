using Application.DTOs.Patient;
using CLI.Helpers;
using CLI.Menus.AppointmentMenu.CreateReferralAppointmentFlow.Commands;
using Core.Models.Referral;
using StatefulMenu.Commands.BuiltIn;
using StatefulMenu.Commands.Interfaces;
using StatefulMenu.Core.Interfaces;
using StatefulMenu.Core.Models;

namespace CLI.Menus.AppointmentMenu.CreateReferralAppointmentFlow.Providers;

public class DoctorSelectionProvider(
    IDataService dataService,
    IServiceProvider serviceProvider) : IMenuProvider
{
    public async Task<MenuState> CreateMenuAsync(CancellationToken cancellationToken = default)
    {
        List<ReferralDoctor>? doctors;
        if (!dataService.TryGet<bool>("IsAnotherDoctor", out _))
        {
            dataService.TryGet<BasePatientProfileDto>(nameof(BasePatientProfileDto), out var patient);
            dataService.TryGet<ReferralSpeciality>(nameof(ReferralSpeciality), out var speciality);
            doctors = speciality.Doctors; // TODO 
            dataService.Set("ToChoseDoctors", doctors);
        }
        else
        {
            dataService.TryGet("ToChoseDoctors", out doctors);
        }

        var commands = doctors
            .Select(d => new DoctorSelectionCommand(d, serviceProvider))
            .Cast<IMenuCommand>()
            .Append(new BackCommand())
            .ToList();

        var items = commands
            .Select(c =>
                new MenuItem(c.Title, _ => c.ExecuteAsync(cancellationToken)))
            .ToList();

        return new MenuState("Выберите специальность", items, header: HeaderFactorySetup.SetupHeader());
    }
}
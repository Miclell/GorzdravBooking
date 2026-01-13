using CLI.Menus.AppointmentMenu.CreateReferralAppointmentFlow.Providers;
using Core.Models;
using Core.Models.Referral;
using Microsoft.Extensions.DependencyInjection;
using StatefulMenu.Commands.Interfaces;
using StatefulMenu.Core.Attributes;
using StatefulMenu.Core.Interfaces;
using StatefulMenu.Core.Models;

namespace CLI.Menus.AppointmentMenu.CreateReferralAppointmentFlow.Commands;

public class DoctorSelectionCommand(
    ReferralDoctor doctor, 
    IServiceProvider serviceProvider) : IMenuCommand
{
    public string Title { get; } = $"{doctor.Name}";
    
    public async Task<MenuResult> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        var dataService = serviceProvider.GetRequiredService<IDataService>();
        if (!dataService.TryGet<List<ReferralDoctor>>("SelectedDoctors", out var selectedDoctors))
            dataService.Set("SelectedDoctors", new List<ReferralDoctor> { doctor });
        else
            selectedDoctors!.Add(doctor);
        
        dataService.TryGet<List<ReferralDoctor>>("ToChoseDoctors", out var doctors);
        doctors!.Remove(doctor);

        if (doctors.Count != 0)
        {
            var inputService = serviceProvider.GetRequiredService<IConsoleInputService>();
            var isAnotherDoctor = (await inputService
                    .ReadModelAsync<AnotherDoctorInputModel>(cancellationToken))!
                .IsAnotherDoctor;

            if (isAnotherDoctor)
            {
                dataService.Set("IsAnotherDoctor", isAnotherDoctor);
                var doctorSelectionProvider = serviceProvider.GetRequiredService<DoctorSelectionProvider>();
                return MenuResult.Push(await doctorSelectionProvider.CreateMenuAsync(cancellationToken));
            }
        }
        
        var timePreferencesSelectionProvider = serviceProvider.GetRequiredService<TimePreferenceSelectionProvider>();
        return MenuResult.Push(await timePreferencesSelectionProvider.CreateMenuAsync(cancellationToken));
    }
    
    [InputModel("выбор нескольких врачей")]
    private record AnotherDoctorInputModel(
        [property: InputField("Выбрать еще одного врача (да/нет)")]
        bool IsAnotherDoctor);
}
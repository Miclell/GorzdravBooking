using Application.Common.Results;
using Application.Coordinators.Interfaces;
using Application.DTOs.Appointment;
using Application.DTOs.TimePreferences;
using Application.Services;
using Core.Interfaces.Services;
using Core.Models;

namespace Application.Coordinators.Implementation;

public class AppointmentCoordinator(IAppointmentService externalAppointmentService,
    AppointmentService appointmentService) : IAppointmentCoordinator
{
    public async Task<Result<bool>> CreateCompleteAppointmentAsync(Core.Entities.AppointmentSearchRequest request, CancellationToken cancellationToken)
    {
        var appointments = await externalAppointmentService.GetByDoctorAsync(
            int.Parse(request.PatientProfile.LpuId),
            request.DoctorId);
        
        
        // TODO получить предпочтения дауна
        var tmp = new List<TimePreferencesPresetDto>();

        var appointment = TryGetPreferAppointment(appointments, tmp);
        
        if (appointment == null)
            return false;

        var createRequest = new AppointmentCreateRequest
        { 
            EsiaId = null,
            LpuId = request.PatientProfile.LpuId, 
            PatientId = request.PatientProfile.PatientId,
            AppointmentId = appointment.Id,
            ReferralId = null,
            IpmpiCardId = null,
            RecipientEmail = request.PatientProfile.RecipientEmail,
            PatientLastName = request.PatientProfile.PatientLastName,
            PatientFirstName = request.PatientProfile.PatientFirstName,
            PatientMiddleName = request.PatientProfile.PatientMiddleName,
            PatientBirthdate = request.PatientProfile.PatientBirthdate,
            Room = appointment.Room,
            Address = appointment.Address!,
            VisitDate = new DateTime()
        };

        var isSuccess = await externalAppointmentService.CreateAppointmentAsync(createRequest);

        var dto = new CreateAppointmentDto
        {
            PatientProfileId = request.PatientProfileId,
            AppointmentId = appointment.Id,
            VisitStart = new DateTime(),
            VisitEnd = new DateTime(),
            Address = appointment.Address,
            Number = appointment.Number,
            Room = appointment.Room
        };

        //if (!isSuccess) return false; // TODO Сучка дрючка там вернет false хз почему затестить на Сане
        await appointmentService.CreateAsync(dto, cancellationToken);
        return true;
    }

    private Appointment? TryGetPreferAppointment(List<Appointment> appointments, List<TimePreferencesPresetDto> timePreferences)
    {
        if (timePreferences.Count == 0 || appointments.Count == 0)
            return null;

        var preset = timePreferences[0];
    
        if (preset.AnyTime)
            return appointments[0];

        return appointments.FirstOrDefault(appointment => 
            preset.Preferences.Any(preference => 
                (!preference.Day.HasValue || 
                 preference.Day.Value == appointment.VisitStart.DayOfWeek) &&
                (!preference.From.HasValue || 
                 !preference.To.HasValue || 
                 (TimeOnly.FromDateTime(appointment.VisitStart) >= preference.From.Value &&
                  TimeOnly.FromDateTime(appointment.VisitStart) <= preference.To.Value))
            )
        );
    }
}
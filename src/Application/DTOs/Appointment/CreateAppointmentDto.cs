namespace Application.DTOs.Appointment;

using StatefulMenu.Core.Attributes;

public class CreateAppointmentDto
{
    [InputField("Профиль пациента Id")]
    public Guid PatientProfileId { get; set; }
    
    [InputField("Идентификатор записи")]
    public string AppointmentId { get; set; } = null!;
    
    [InputField("Начало визита")]
    public DateTime VisitStart { get; set; }
    
    [InputField("Окончание визита")]
    public DateTime VisitEnd { get; set; }
    
    [InputField("Адрес", IsRequired = false)]
    public string? Address { get; set; }
    
    [InputField("Номер", IsRequired = false)]
    public string? Number { get; set; }
    
    [InputField("Кабинет", IsRequired = false)]
    public string? Room { get; set; }
}
namespace Core.Models;

public class AppointmentСancelRequest
{
    public string AppointmentId { get; set; }
    public string LpuId { get; set; }
    public string PatientId { get; set; }
    public string EsiaId { get; set; } = "";
    public string AppointmentType { get; set; }
}
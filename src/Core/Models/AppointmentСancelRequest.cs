namespace Core.Models;

public class AppointmentСancelRequest
{
    public string AppointmentId { get; set; } = null!;
    public string LpuId { get; set; } = null!;
    public string PatientId { get; set; } = null!;
    public string EsiaId { get; set; } = "";
}
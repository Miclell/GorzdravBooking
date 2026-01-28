namespace Core.Models;

public class Appointment
{
    public string Id { get; set; } = null!;
    public DateTime VisitStart { get; set; }
    public DateTime VisitEnd { get; set; }
    public string? Address { get; set; }
    public string? Number { get; set; }
    public string Room { get; set; } = null!;
}
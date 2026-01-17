namespace Core.Models;

public class Doctor
{
    public string AriaNumber { get; set; } = null!;
    public string? AriaType { get; set; }
    public string Comment { get; set; } = null!;
    public int FreeParticipantCount { get; set; }
    public int FreeTicketCount { get; set; }
    public string Id { get; set; } = null!;
    public DateTime? LastDate { get; set; }
    public string Name { get; set; } = null!;
    public string? LastName { get; set; }
    public string? FirstName { get; set; }
    public string? MiddleName { get; set; }
    public DateTime? NearestDate { get; set; }
}
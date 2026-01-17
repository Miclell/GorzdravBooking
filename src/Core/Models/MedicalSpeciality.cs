namespace Core.Models;

public class MedicalSpeciality
{
    public string Id { get; set; } = null!;
    public string FerId { get; set; } = null!;
    public string Name { get; set; } = null!;
    public int CountFreeParticipant { get; set; }
    public int CountFreeTicket { get; set; }
    public string LastDate { get; set; } = null!;
    public string NearestDate { get; set; } = null!;
}
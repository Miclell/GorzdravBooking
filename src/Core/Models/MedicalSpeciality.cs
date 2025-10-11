namespace Core.Models;

public class MedicalSpeciality
{
    public string Id { get; set; }
    public string FerId { get; set; }
    public string Name { get; set; }
    public int CountFreeParticipant { get; set; }
    public int CountFreeTicket { get; set; }
    public string LastDate { get; set; }
    public string NearestDate { get; set; }
}

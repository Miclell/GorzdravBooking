namespace Core.Models;

public class Doctor
{
    public string AriaNumber { get; set; }
    public string? AriaType { get; set; }
    public string Comment { get; set; }
    public int FreeParticipantCount { get; set; }
    public int FreeTicketCount { get; set; }
    public string Id { get; set; }
    public DateTime? LastDate { get; set; }
    public string Name { get; set; }
    public string? LastName { get; set; }
    public string? FirstName { get; set; }
    public string? MiddleName { get; set; }
    public DateTime? NearestDate { get; set; }
}
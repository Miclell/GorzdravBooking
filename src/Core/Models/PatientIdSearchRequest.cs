namespace Core.Models;

public class PatientIdSearchRequest
{
    public string LpuId { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public string FirstName { get; set; } = null!;
    public string? MiddleName { get; set; }
    public DateTime BirthDate { get; set; }
    public string BirthDateValue => BirthDate.ToString("dd.MM.yyyy");
}
namespace Core.Models;

public class Lpu
{
    public int? CovidVaccinationTicketCount { get; set; }
    public DateTime? CovidVaccinationTicketReceiveTime { get; set; }
    public int Id { get; set; }
    public string Description { get; set; } = null!;
    public int District { get; set; }
    public int DistrictId { get; set; }
    public string DistrictName { get; set; } = null!;
    public bool IsActive { get; set; }
    public string LpuFullName { get; set; } = null!;
    public string LpuShortName { get; set; } = null!;
    public string LpuType { get; set; } = null!;
    public string? Oid { get; set; }
    public string? PartOf { get; set; }
    public string HeadOrganization { get; set; } = null!;
    public string Organization { get; set; } = null!;
    public string Address { get; set; } = null!;
    public string Phone { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string Longitude { get; set; } = null!;
    public string Latitude { get; set; } = null!;
    public bool CovidVaccination { get; set; }
    public bool InDepthExamination { get; set; }
    public string? Subdivision { get; set; }
}
namespace Core.Models;

public class Lpu
{
    public int? CovidVaccinationTicketCount { get; set; }
    public DateTime? CovidVaccinationTicketReceiveTime { get; set; }
    public int Id { get; set; }
    public string Description { get; set; }
    public int District { get; set; }
    public int DistrictId { get; set; }
    public string DistrictName { get; set; }
    public bool IsActive { get; set; }
    public string LpuFullName { get; set; }
    public string LpuShortName { get; set; }
    public string LpuType { get; set; }
    public string? Oid { get; set; }
    public string? PartOf { get; set; }
    public string HeadOrganization { get; set; }
    public string Organization { get; set; }
    public string Address { get; set; }
    public string Phone { get; set; }
    public string Email { get; set; }
    public string Longitude { get; set; }
    public string Latitude { get; set; }
    public bool CovidVaccination { get; set; }
    public bool InDepthExamination { get; set; }
    public string? Subdivision { get; set; }
}
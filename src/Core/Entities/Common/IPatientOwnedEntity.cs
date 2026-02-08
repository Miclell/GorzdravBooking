namespace Core.Entities.Common;

public interface IPatientOwnedEntity
{
    Guid Id { get; }
    Guid PatientProfileId { get; }
    PatientProfile PatientProfile { get; }
}
using Application.Abstract;
using Application.Common.Results;
using Application.DTOs.Patient;
using Application.Services.Interfaces;
using Core.Entities;
using Core.Interfaces.Repositories;
using Microsoft.Extensions.Logging;

namespace Application.Services.Implementation;

public class PatientService(IPatientRepository patientRepository, 
    ILogger<PatientService> logger) : IAppService, IPatientService
{
    public async Task<Result<Guid>> Create(CreatePatientDto createPatientDto, CancellationToken cancellationToken = default)
    {
        try
        {
            var patient = new PatientProfile
            {
                UserId = createPatientDto.UserId,
                LpuId = createPatientDto.LpuId,
                PatientId = createPatientDto.PatientId,
                LpuShortName = createPatientDto.LpuShortName,
                LpuAddress = createPatientDto.LpuAddress,
                PatientLastName = createPatientDto.PatientLastName,
                PatientFirstName = createPatientDto.PatientFirstName,
                PatientMiddleName = createPatientDto.PatientMiddleName,
                PatientBirthdate = createPatientDto.PatientBirthdate,
                RecipientEmail = createPatientDto.RecipientEmail,
                MobilePhoneNumber = createPatientDto.MobilePhoneNumber
            };

            await patientRepository.AddAsync(patient, cancellationToken);

            return patient.Id;
        }
        catch (Exception e)
        {
            logger.LogError(e, "Ошибка при добавлении PatientProfile для User с id {UserId} - {e}", createPatientDto.UserId, e);
            return Error.Failure($"{e}", "Ошибка");
        }
    }

    public async Task<Result> Delete(Guid patientId, CancellationToken cancellationToken = default)
    {
        try
        {
            await patientRepository.DeleteAsync(patientId, cancellationToken);
            return Result.Success();
        }
        catch (Exception e)
        {
            logger.LogError(e, "Ошибка при удалении пациента с id {PatientId}", patientId);
            return Error.Failure(e.ToString(), "Ошибка");
        }
    }
    
    public async Task<Result<IEnumerable<BasePatientProfileDto>>> GetByUser(Guid userId, CancellationToken cancellationToken = default)
    {
        try
        {
            var patientProfiles = await patientRepository.GetByUserIdAsync(userId, cancellationToken);
        
            var dto = patientProfiles.Select(patient => new BasePatientProfileDto(
                Id: patient.Id,
                LpuShortName: patient.LpuShortName,
                PatientLastName: patient.PatientLastName,
                PatientFirstName: patient.PatientFirstName,
                PatientMiddleName: patient.PatientMiddleName,
                PatientBirthdate: patient.PatientBirthdate,
                RecipientEmail: patient.RecipientEmail,
                MobilePhoneNumber: patient.MobilePhoneNumber
            ));
        
            return Result.Success(dto);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Ошибка при получении пациентов для пользователя с id {UserId}", userId);
            return Error.Failure(e.ToString(), "Ошибка");
        }
    }

    public async Task<Result> Update(BasePatientProfileDto patientProfile, CancellationToken cancellationToken = default)
    {
        try
        {
            var existingPatient = await patientRepository.GetByIdAsync(patientProfile.Id, cancellationToken);
            if (existingPatient == null)
                return Error.NotFound("Patient.Not.Found","Patient not found");
            
            existingPatient.LpuShortName = patientProfile.LpuShortName;
            existingPatient.PatientLastName = patientProfile.PatientLastName;
            existingPatient.PatientFirstName = patientProfile.PatientFirstName;
            existingPatient.PatientMiddleName = patientProfile.PatientMiddleName;
            existingPatient.PatientBirthdate = patientProfile.PatientBirthdate;
            existingPatient.RecipientEmail = patientProfile.RecipientEmail;
            existingPatient.MobilePhoneNumber = patientProfile.MobilePhoneNumber;

            await patientRepository.UpdateAsync(existingPatient, cancellationToken);
            
            return Result.Success();
        }
        catch (Exception e)
        {
            logger.LogError(e, "Ошибка при обновлении данных пациента с id {PatientProfile}", patientProfile.Id);
            return Error.Failure(e.ToString(), "Ошибка");
        }
    }
}
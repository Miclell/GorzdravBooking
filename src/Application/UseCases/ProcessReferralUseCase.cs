using Application.Abstract;
using Application.Common.Results;
using Application.DTOs.UseCases;
using Application.Extensions;
using Application.Services.Interfaces;
using Core.Exceptions;
using Core.Interfaces.Services;
using Microsoft.Extensions.Logging;

namespace Application.UseCases;

public class ProcessReferralUseCase(
    IPatientService patientService,
    IExternalAppointmentService externalAppointmentService,
    ILogger<ProcessReferralUseCase> logger) : IAppUseCase
{
    public async Task<Result<ReferralValidationResult>> Execute(
        ReferralValidationRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var referralResult = await externalAppointmentService
                .GetByReferralAsync(request.ReferralNumber, request.LastName);

            var patients = await patientService.GetByUser(request.UserId, cancellationToken);
            if (!patients.IsSuccess)
                return Error.Failure("Failed.To.Get.Patients", $"Ошибка при получении пациентов для {request.UserId}");

            var patient = patients.Value
                .FirstOrDefault(p
                    => p.PatientId == referralResult.PatId &&
                       p.LpuShortName == referralResult.LpuShortName);

            if (patient != null)
                return new ReferralValidationResult(patient, referralResult.Specialities);

            var patientId = await patientService
                .Create(referralResult.ToCreatePatientDto(request.UserId), cancellationToken);

            if (!patientId.IsSuccess)
                return patientId.Error;

            return new ReferralValidationResult(
                (await patientService.GetById(patientId.Value, cancellationToken)).Value,
                referralResult.Specialities);
        }
        catch (ReferralNotFoundException e)
        {
            return Error.NotFound("Invalid.Referral", e.Message);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Ошибка проверки валидности запроса на запись по направлению");
            return Error.Failure(e.ToString(), "Failed to validate referral request");
        }
    }
}
using Application.Abstract;
using Application.Common.Results;
using Application.DTOs.AppointmentSearchRequest;
using Application.Extensions;
using Application.Services.Interfaces;
using Core.Entities;
using Core.Interfaces.Repositories;
using Microsoft.Extensions.Logging;

namespace Application.Services.Implementation;

public class AppointmentSearchRequestService(
    IAppointmentSearchRequestRepository appointmentSearchRequestRepository,
    ILogger<AppointmentSearchRequestService> logger) : IAppService, IAppointmentSearchRequestService
{
    public async Task<Result<Guid>> CreateAsync(
        CreateAppointmentSearchRequestDto createDto,
        CancellationToken cancellationToken = default)
    {
        try
        {
            AppointmentSearchRequest request;
            if (createDto.ReferralNumber != null)
                request = new ReferralSearchRequest
                {
                    ReferralNumber = createDto.ReferralNumber
                };
            else
                request = new ManualSearchRequest();

            request.PatientProfileId = createDto.PatientProfileId;
            request.LpuName = createDto.LpuName;
            request.Speciality = createDto.Speciality;
            request.DoctorMode = createDto.DoctorMode;
            request.DoctorIds = createDto.DoctorIds;
            request.DoctorNames = createDto.DoctorNames;
            request.TimePreferencesPresetName = createDto.TimePreferencesPresetName;
            request.SearchInterval = createDto.SearchInterval;
            request.SpecificStartPoints = createDto.SpecificStartPoints;
            request.ViewOnly = createDto.ViewOnly;
            request.MaxDaysToSearch = createDto.MaxDaysToSearch;

            await appointmentSearchRequestRepository.AddAsync(request, cancellationToken);

            return Result.Success(request.Id);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Ошибка при создании запроса на поиск записи для пациента {PatientProfileId}",
                createDto.PatientProfileId);
            return Error.Failure("UnexpectedError", "Failed to create search request");
        }
    }

    public async Task<Result> DeleteAsync(Guid requestId, CancellationToken cancellationToken = default)
    {
        try
        {
            await appointmentSearchRequestRepository.DeleteAsync(requestId, cancellationToken);
            return Result.Success();
        }
        catch (Exception e)
        {
            logger.LogError(e, "Ошибка при удалении запроса на поиск записи {RequestId}", requestId);
            return Error.Failure("UnexpectedError", "Failed to delete search request");
        }
    }

    public async Task<Result> UpdateTimePreferencesAsync(
        UpdatePreferencesDto updateDto,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var request = await appointmentSearchRequestRepository.GetByIdAsync(updateDto.RequestId, cancellationToken);
            if (request == null)
                return Error.Failure("SearchRequest.NotFound", "Search request not found");

            request.TimePreferencesPresetName = updateDto.TimePreferencesName;
            request.SpecificStartPoints = updateDto.SpecificStartPoints;
            request.SearchInterval = updateDto.SearchInterval;
            request.MaxDaysToSearch = updateDto.MaxDaysToSearch;
            request.ViewOnly = updateDto.ViewOnly;
            await appointmentSearchRequestRepository.UpdateAsync(request, cancellationToken);

            return Result.Success();
        }
        catch (Exception e)
        {
            logger.LogError(e, "Ошибка при обновлении временных предпочтений для запроса {RequestId}",
                updateDto.RequestId);
            return Error.Failure("UnexpectedError", "Failed to update time preferences");
        }
    }

    public async Task<Result<IEnumerable<AppointmentSearchRequestDto>>> GetActiveByUserAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var requests = await appointmentSearchRequestRepository.GetActiveByUserIdAsync(userId, cancellationToken);

            var dtos = requests.Select(request => request.ToDto());

            return Result.Success(dtos);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Ошибка при получении активных запросов пользователя {UserId}", userId);
            return Error.Failure("UnexpectedError", "Failed to get active search requests");
        }
    }

    public async Task<Result<IEnumerable<AppointmentSearchRequestDto>>> GetByPatientAsync(
        Guid patientProfileId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var requests =
                await appointmentSearchRequestRepository.GetByPatientProfileIdAsync(patientProfileId,
                    cancellationToken);

            var dtos = requests.Select(request => request.ToDto());

            return Result.Success(dtos);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Ошибка при получении запросов пациента {PatientProfileId}", patientProfileId);
            return Error.Failure("UnexpectedError", "Failed to get patient search requests");
        }
    }
}
using Application.Abstract;
using Application.Common.Results;
using Application.DTOs.AppointmentSearchRequest;
using Application.Services.Interfaces;
using Core.Entities;
using Core.Enums;
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
            var request = new AppointmentSearchRequest
            {
                PatientProfileId = createDto.PatientProfileId,
                LpuName = createDto.LpuName,
                DoctorId = createDto.DoctorId,
                DoctorName = createDto.DoctorName,
                SearchInterval = createDto.SearchInterval,
                SpecificStartPoints = createDto.SpecificStartPoints,
                TimePreferencesPresetName = createDto.TimePreferencesPresetName,
                ViewOnly = createDto.ViewOnly,
                MaxDaysToSearch = createDto.MaxDaysToSearch,
                Status = SearchRequestStatus.Pending
            };

            await appointmentSearchRequestRepository.AddAsync(request, cancellationToken);

            return Result.Success(request.Id);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Ошибка при создании запроса на поиск записи для пациента {PatientProfileId}", createDto.PatientProfileId);
            return Error.Failure(e.ToString(), "Failed to create search request");
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
            return Error.Failure(e.ToString(), "Failed to delete search request");
        }
    }

    public async Task<Result> UpdateTimePreferencesAsync(
        UpdateTimePreferencesDto updateDto,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var request = await appointmentSearchRequestRepository.GetByIdAsync(updateDto.RequestId, cancellationToken);
            if (request == null)
                return Error.Failure("SearchRequest.NotFound", "Search request not found");

            request.TimePreferencesPresetName = updateDto.TimePreferencesName;
            await appointmentSearchRequestRepository.UpdateAsync(request, cancellationToken);

            return Result.Success();
        }
        catch (Exception e)
        {
            logger.LogError(e, "Ошибка при обновлении временных предпочтений для запроса {RequestId}", updateDto.RequestId);
            return Error.Failure(e.ToString(), "Failed to update time preferences");
        }
    }

    public async Task<Result<IEnumerable<AppointmentSearchRequestDto>>> GetActiveByUserAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var requests = await appointmentSearchRequestRepository.GetActiveByUserIdAsync(userId, cancellationToken);

            var dtos = requests.Select(request => new AppointmentSearchRequestDto(
                Id: request.Id,
                PatientProfileId: request.PatientProfileId,
                LpuName: request.LpuName,
                DoctorName: request.DoctorName,
                SearchInterval: request.SearchInterval,
                SpecificStartPoints: request.SpecificStartPoints,
                TimePreferencesPresetName: request.TimePreferencesPresetName,
                ViewOnly: request.ViewOnly,
                MaxDaysToSearch: request.MaxDaysToSearch,
                CreatedAt: request.CreatedAt,
                LastSearchAttempt: request.LastSearchAttempt,
                AttemptCount: request.AttemptCount,
                Status: request.Status.ToString(),
                PatientFullName: $"{request.PatientProfile.PatientLastName} {request.PatientProfile.PatientFirstName} {request.PatientProfile.PatientMiddleName}"
            ));

            return Result.Success(dtos);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Ошибка при получении активных запросов пользователя {UserId}", userId);
            return Error.Failure(e.ToString(), "Failed to get active search requests");
        }
    }

    // Дополнительный метод - получить все запросы пациента
    public async Task<Result<IEnumerable<AppointmentSearchRequestDto>>> GetByPatientAsync(
        Guid patientProfileId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var requests = await appointmentSearchRequestRepository.GetByPatientProfileIdAsync(patientProfileId, cancellationToken);

            var dtos = requests.Select(request => new AppointmentSearchRequestDto(
                Id: request.Id,
                PatientProfileId: request.PatientProfileId,
                LpuName: request.LpuName,
                DoctorName: request.DoctorName,
                SearchInterval: request.SearchInterval,
                SpecificStartPoints: request.SpecificStartPoints,
                TimePreferencesPresetName: request.TimePreferencesPresetName,
                ViewOnly: request.ViewOnly,
                MaxDaysToSearch: request.MaxDaysToSearch,
                CreatedAt: request.CreatedAt,
                LastSearchAttempt: request.LastSearchAttempt,
                AttemptCount: request.AttemptCount,
                Status: request.Status.ToString(),
                PatientFullName: $"{request.PatientProfile.PatientLastName} {request.PatientProfile.PatientFirstName} {request.PatientProfile.PatientMiddleName}"
            ));

            return Result.Success(dtos);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Ошибка при получении запросов пациента {PatientProfileId}", patientProfileId);
            return Error.Failure(e.ToString(), "Failed to get patient search requests");
        }
    }
}
using Application.Abstract;
using Application.Coordinators.Interfaces;
using Core.Entities;
using Core.Enums;
using Core.Interfaces.Repositories;
using Microsoft.Extensions.Logging;

namespace Application.UseCases;

public class CheckAppointmentSearchRequestsUseCase(
    IAppointmentSearchRequestRepository appointmentSearchRequestRepository,
    IAppointmentCoordinator appointmentCoordinator,
    ILogger<CheckAppointmentSearchRequestsUseCase> logger) : IAppUseCase
{
    public async Task ExecuteAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var now = DateTime.UtcNow;
            var requests = await appointmentSearchRequestRepository.GetActiveAsync(cancellationToken);

            var appointmentSearchRequests = requests.ToList();
            logger.LogDebug("Найдено {Count} активных запросов", appointmentSearchRequests.Count);

            foreach (var request in appointmentSearchRequests
                         .Where(request => IsTimeToCheck(request, now) || true))
                try
                {
                    logger.LogDebug("Обработка запроса {RequestId} для пациента {PatientId}", request.Id,
                        request.PatientProfileId);

                    var result =
                        await appointmentCoordinator.CreateCompleteAppointmentAsync(request, cancellationToken);

                    request.LastSearchAttempt = now;
                    request.AttemptCount++;
                    if (request.Status == SearchRequestStatus.Pending)
                        request.Status = SearchRequestStatus.InProgress;

                    if (result is { IsSuccess: true, Value: true })
                        request.Status = SearchRequestStatus.Completed;

                    await appointmentSearchRequestRepository.UpdateAsync(request, cancellationToken);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Ошибка при обработке запроса {RequestId}", request.Id);
                }
        }
        catch (Exception ex)
        {
            logger.LogCritical(ex, "Ошибка при выполнении CheckAppointmentSearchRequestsUseCase");
        }
    }

    private static bool IsTimeToCheck(AppointmentSearchRequest request, DateTime now)
    {
        if (request.SpecificStartPoints.Count != 0)
        {
            var nearest = request.SpecificStartPoints.Where(t => t > (request.LastSearchAttempt ?? request.CreatedAt))
                .OrderBy(t => t)
                .FirstOrDefault();

            if (nearest == default) return false;
            return now >= nearest;
        }

        if (request.LastSearchAttempt == null) return true;
        return now - request.LastSearchAttempt >= request.SearchInterval;
    }
}
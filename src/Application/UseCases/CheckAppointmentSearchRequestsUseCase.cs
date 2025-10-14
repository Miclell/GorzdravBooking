using Application.Coordinators.Interfaces;
using Application.DTOs.Appointment;
using Application.Services;
using Core.Enums;
using Core.Interfaces.Repositories;
using Core.Interfaces.Services;
using Core.Models;
using Microsoft.Extensions.Logging;

namespace Application.UseCases;

public class CheckAppointmentSearchRequestsUseCase(
    IAppointmentSearchRequestRepository appointmentSearchRequestRepository,
    IAppointmentCoordinator appointmentCoordinator,
    ILogger<CheckAppointmentSearchRequestsUseCase> logger)
{
    public async Task ExecuteAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var now = DateTime.UtcNow;
            var requests = await appointmentSearchRequestRepository.GetActiveAsync(cancellationToken);
            
            logger.LogDebug("Найдено {Count} активных запросов", requests.Count());
            
            foreach (var request in requests)
            {
                if (!IsTimeToCheck(request, now))
                    continue;

                try
                {
                    logger.LogDebug("Обработка запроса {RequestId} для пациента {PatientId}", request.Id, request.PatientProfileId);
                    
                    await ProcessRequestAsync(request, cancellationToken);

                    request.LastSearchAttempt = now;
                    request.AttemptCount++;
                    await appointmentSearchRequestRepository.UpdateAsync(request, cancellationToken);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Ошибка при обработке запроса {RequestId}", request.Id);
                    // можно здесь менять статус на ошибку или делать retry logic
                }
            }
        }
        catch (Exception ex)
        {
            logger.LogCritical(ex, "Ошибка при выполнении CheckAppointmentSearchRequestsUseCase");
        }
    }

    private bool IsTimeToCheck(Core.Entities.AppointmentSearchRequest request, DateTime now)
    {
        // Если есть конкретные временные точки
        if (request.SpecificStartPoints.Count != 0)
        {
            var nearest = request.SpecificStartPoints.Where(t => t > (request.LastSearchAttempt ?? request.CreatedAt))
                .OrderBy(t => t)
                .FirstOrDefault();

            if (nearest == default) return false;
            return now >= nearest;
        }

        // Проверяем интервал
        if (request.LastSearchAttempt == null) return true; // первый запуск
        return now - request.LastSearchAttempt >= request.SearchInterval;
    }

    private async Task ProcessRequestAsync(Core.Entities.AppointmentSearchRequest request, CancellationToken cancellationToken)
    {
        var result = await appointmentCoordinator.CreateCompleteAppointmentAsync(request, cancellationToken);
        
        
        
        // Здесь логика записи в систему ЛПУ
        // Например: дергаем API, проверяем свободные слоты
        // Если все ок — меняем статус запроса
        request.Status = SearchRequestStatus.Completed;

        // await ExternalApi.BookAppointmentAsync(request, cancellationToken);
    }
}

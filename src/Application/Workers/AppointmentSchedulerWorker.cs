using Application.UseCases;
using Core.Events;
using Core.Events.Common;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Application.Workers;

public class AppointmentSchedulerWorker(
    IServiceProvider serviceProvider,
    ILogger<AppointmentSchedulerWorker> logger,
    IEventBus eventBus) : BackgroundService
{
    private static readonly TimeSpan SearchInterval = TimeSpan.FromMinutes(1);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await eventBus.PublishAsync(new SearchServiceStatusChanged(true, true), stoppingToken);

        logger.LogDebug("Сервис запущен в {Time}", DateTime.Now);

        while (!stoppingToken.IsCancellationRequested)
        {
            var startTime = DateTime.UtcNow;
            logger.LogDebug("Запуск проверки записей в {Time}", startTime);

            try
            {
                using var scope = serviceProvider.CreateScope();

                var checkAppointmentsUseCase =
                    scope.ServiceProvider.GetRequiredService<CheckAppointmentSearchRequestsUseCase>();

                await checkAppointmentsUseCase.ExecuteAsync(stoppingToken);

                var elapsed = DateTime.UtcNow - startTime;
                logger.LogDebug("Проверка записей завершена за {ElapsedMs}ms", elapsed.TotalMilliseconds);
            }
            catch (Exception e)
            {
                logger.LogError(e, "Ошибка в планировщике");
            }

            await eventBus.PublishAsync(new NextSearchScheduled(DateTime.UtcNow + SearchInterval), stoppingToken);
            await Task.Delay(SearchInterval, stoppingToken);
        }
    }
}
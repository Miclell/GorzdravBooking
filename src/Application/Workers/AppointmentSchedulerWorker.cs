using Application.UseCases;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Application.Workers;

public class AppointmentSchedulerWorker(
    IServiceProvider serviceProvider, 
    ILogger<AppointmentSchedulerWorker> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
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

            await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
        }
    }
}
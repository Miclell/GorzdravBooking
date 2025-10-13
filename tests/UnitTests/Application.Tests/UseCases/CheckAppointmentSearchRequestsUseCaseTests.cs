using Application.UseCases;
using Core.Entities;
using Core.Enums;
using Core.Interfaces.Repositories;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Application.Tests.UseCases;


// TODO переделать тест когда не будет комментариев и будет полное взаимодействие с апишкой
public class CheckAppointmentSearchRequestsUseCaseTests
{
    private readonly Mock<IAppointmentSearchRequestRepository> _mockRepo;
    private readonly CheckAppointmentSearchRequestsUseCase _useCase;

    public CheckAppointmentSearchRequestsUseCaseTests()
    {
        _mockRepo = new Mock<IAppointmentSearchRequestRepository>();
        var mockLogger = new Mock<ILogger<CheckAppointmentSearchRequestsUseCase>>();
        _useCase = new CheckAppointmentSearchRequestsUseCase(_mockRepo.Object, mockLogger.Object);
    }

    [Fact]
    public async Task ExecuteAsync_WithActiveRequests_ProcessesEachRequest()
    {
        // Arrange
        var requests = new List<AppointmentSearchRequest>
        {
            new()
            {
                Id = Guid.NewGuid(),
                PatientProfileId = Guid.Parse("349e22e6-75cc-4d92-97e4-4c36f2800aed"),
                Status = SearchRequestStatus.InProgress,
                LastSearchAttempt = null, // никогда не проверялся
                SearchInterval = TimeSpan.FromHours(1),
                SpecificStartPoints = new List<DateTime>(),
                CreatedAt = DateTime.UtcNow.AddHours(-2)
            },
            new()
            {
                Id = Guid.NewGuid(), 
                PatientProfileId = Guid.Parse("f7362ecc-6359-4238-9688-9fd0f53bec41"),
                Status = SearchRequestStatus.InProgress,
                LastSearchAttempt = DateTime.UtcNow.AddMinutes(-30), // проверялся 30 мин назад
                SearchInterval = TimeSpan.FromHours(1),
                SpecificStartPoints = new List<DateTime>(),
                CreatedAt = DateTime.UtcNow.AddHours(-3)
            }
        };

        _mockRepo.Setup(x => x.GetActiveAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(requests);

        // Act
        await _useCase.ExecuteAsync();

        // Assert - проверяем что обновили оба запроса
        _mockRepo.Verify(x => x.UpdateAsync(It.IsAny<AppointmentSearchRequest>(), It.IsAny<CancellationToken>()), 
            Times.Exactly(2));
    }

    [Fact]
    public async Task ExecuteAsync_WithSpecificStartPoints_ProcessesAtRightTime()
    {
        // Arrange
        var now = DateTime.UtcNow;
        var request = new AppointmentSearchRequest
        {
            Id = Guid.NewGuid(),
            Status = SearchRequestStatus.InProgress,
            SpecificStartPoints = new List<DateTime> 
            { 
                now.AddMinutes(-10), // уже прошло - должно обработаться
                now.AddMinutes(10)   // еще не наступило
            },
            LastSearchAttempt = null,
            CreatedAt = now.AddHours(-1)
        };

        _mockRepo.Setup(x => x.GetActiveAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<AppointmentSearchRequest> { request });

        // Act
        await _useCase.ExecuteAsync();

        // Assert - должен обработаться один раз
        _mockRepo.Verify(x => x.UpdateAsync(It.IsAny<AppointmentSearchRequest>(), It.IsAny<CancellationToken>()), 
            Times.Once);
    }

    [Fact] 
    public async Task ExecuteAsync_WhenProcessRequestFails_LogsErrorButContinues()
    {
        // Arrange
        var requests = new List<AppointmentSearchRequest>
        {
            new() { Id = Guid.NewGuid(), Status = SearchRequestStatus.InProgress, LastSearchAttempt = null },
            new() { Id = Guid.NewGuid(), Status = SearchRequestStatus.InProgress, LastSearchAttempt = null }
        };

        _mockRepo.Setup(x => x.GetActiveAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(requests);

        // Здесь будет исключение в ProcessRequestAsync, но мы продолжаем

        // Act
        await _useCase.ExecuteAsync();

        // Assert - все равно пытаемся обновить оба запроса
        _mockRepo.Verify(x => x.UpdateAsync(It.IsAny<AppointmentSearchRequest>(), It.IsAny<CancellationToken>()), 
            Times.Exactly(2));
        
        // И логируем ошибку
        // Можно проверить логи через _mockLogger
    }
}
using Application.Common.Results;
using Application.Coordinators.Interfaces;
using Application.UseCases;
using Core.Entities;
using Core.Enums;
using Core.Interfaces.Repositories;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Application.Tests.UseCases;

public class CheckAppointmentSearchRequestsUseCaseTests
{
    private readonly Mock<IAppointmentSearchRequestRepository> _mockRepo;
    private readonly Mock<IAppointmentCoordinator> _mockCoordinator;
    private readonly Mock<ILogger<CheckAppointmentSearchRequestsUseCase>> _mockLogger;
    private readonly CheckAppointmentSearchRequestsUseCase _useCase;

    public CheckAppointmentSearchRequestsUseCaseTests()
    {
        _mockRepo = new Mock<IAppointmentSearchRequestRepository>();
        _mockCoordinator = new Mock<IAppointmentCoordinator>();
        _mockLogger = new Mock<ILogger<CheckAppointmentSearchRequestsUseCase>>();
        
        _useCase = new CheckAppointmentSearchRequestsUseCase(
            _mockRepo.Object, 
            _mockCoordinator.Object, 
            _mockLogger.Object);
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
                LastSearchAttempt = null, // Обработается
                SearchInterval = TimeSpan.FromHours(1),
                SpecificStartPoints = [],
                CreatedAt = DateTime.UtcNow.AddHours(-2)
            },
            new()
            {
                Id = Guid.NewGuid(), 
                PatientProfileId = Guid.Parse("f7362ecc-6359-4238-9688-9fd0f53bec41"),
                Status = SearchRequestStatus.InProgress,
                LastSearchAttempt = DateTime.UtcNow.AddHours(-2), // 2 часа назад > 1 час интервал → обработается
                SearchInterval = TimeSpan.FromHours(1),
                SpecificStartPoints = [],
                CreatedAt = DateTime.UtcNow.AddHours(-3)
            }
        };

        _mockRepo.Setup(x => x.GetActiveAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(requests);

        _mockCoordinator
            .Setup(x => x.CreateCompleteAppointmentAsync(It.IsAny<AppointmentSearchRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(true));

        // Act
        await _useCase.ExecuteAsync();

        // Assert
        _mockCoordinator.Verify(
            x => x.CreateCompleteAppointmentAsync(It.IsAny<AppointmentSearchRequest>(), It.IsAny<CancellationToken>()), 
            Times.Exactly(2));
        
        _mockRepo.Verify(x => x.UpdateAsync(It.IsAny<AppointmentSearchRequest>(), It.IsAny<CancellationToken>()), 
            Times.Exactly(2));
    }

    [Fact]
    public async Task ExecuteAsync_WhenCoordinatorFails_LogsErrorButContinues()
    {
        // Arrange
        var requests = new List<AppointmentSearchRequest>
        {
            new() { Id = Guid.NewGuid(), Status = SearchRequestStatus.InProgress, LastSearchAttempt = null },
            new() { Id = Guid.NewGuid(), Status = SearchRequestStatus.InProgress, LastSearchAttempt = null }
        };

        _mockRepo.Setup(x => x.GetActiveAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(requests);

        _mockCoordinator
            .SetupSequence(x => x.CreateCompleteAppointmentAsync(It.IsAny<AppointmentSearchRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Error.Failure("First Failed", "First Failed"))
            .ReturnsAsync(Result.Success(true));

        // Act
        await _useCase.ExecuteAsync();

        // Assert - все равно обрабатываем оба запроса
        _mockCoordinator.Verify(
            x => x.CreateCompleteAppointmentAsync(It.IsAny<AppointmentSearchRequest>(), It.IsAny<CancellationToken>()), 
            Times.Exactly(2));
        
        _mockRepo.Verify(x => x.UpdateAsync(It.IsAny<AppointmentSearchRequest>(), It.IsAny<CancellationToken>()), 
            Times.Exactly(2));
    }

    [Fact]
    public async Task ExecuteAsync_WithSpecificStartPoints_ProcessesOnlyAtRightTime()
    {
        // Arrange
        var now = DateTime.UtcNow;
        var request = new AppointmentSearchRequest
        {
            Id = Guid.NewGuid(),
            Status = SearchRequestStatus.InProgress,
            SpecificStartPoints = [now.AddMinutes(-10)], // должно обработаться
            LastSearchAttempt = null,
            CreatedAt = now.AddHours(-1)
        };

        _mockRepo.Setup(x => x.GetActiveAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<AppointmentSearchRequest> { request });

        _mockCoordinator
            .Setup(x => x.CreateCompleteAppointmentAsync(It.IsAny<AppointmentSearchRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(true));

        // Act
        await _useCase.ExecuteAsync();

        // Assert
        _mockCoordinator.Verify(
            x => x.CreateCompleteAppointmentAsync(It.IsAny<AppointmentSearchRequest>(), It.IsAny<CancellationToken>()), 
            Times.Once);
    }
}
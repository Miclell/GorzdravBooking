using Application.Common.Results;
using Application.DTOs.AppointmentSearchRequest;
using Application.Services.Implementation;
using Core.Entities;
using Core.Enums;
using Core.Interfaces.Repositories;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Application.Tests.Services;

public class AppointmentSearchRequestServiceTests
{
    private readonly Mock<IAppointmentSearchRequestRepository> _mockRepository;
    private readonly Mock<ILogger<AppointmentSearchRequestService>> _mockLogger;
    private readonly AppointmentSearchRequestService _service;

    public AppointmentSearchRequestServiceTests()
    {
        _mockRepository = new Mock<IAppointmentSearchRequestRepository>();
        _mockLogger = new Mock<ILogger<AppointmentSearchRequestService>>();
        _service = new AppointmentSearchRequestService(_mockRepository.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task CreateAsync_WithValidData_CreatesSearchRequest()
    {
        // Arrange
        var createDto = new CreateAppointmentSearchRequestDto
        {
            PatientProfileId = Guid.NewGuid(),
            LpuName = "Test Hospital",
            DoctorId = "doc123",
            DoctorName = "Dr. Smith",
            SearchInterval = TimeSpan.FromHours(2),
            SpecificStartPoints = new List<DateTime> { DateTime.Now.AddDays(1) },
            TimePreferencesPresetName = "WorkDays",
            ViewOnly = false,
            MaxDaysToSearch = 7
        };

        var expectedRequestId = Guid.NewGuid();

        _mockRepository
            .Setup(x => x.AddAsync(It.IsAny<AppointmentSearchRequest>(), It.IsAny<CancellationToken>()))
            .Callback<AppointmentSearchRequest, CancellationToken>((request, _) => request.Id = expectedRequestId);

        // Act
        var result = await _service.CreateAsync(createDto, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(expectedRequestId, result.Value);
        
        _mockRepository.Verify(x => x.AddAsync(It.IsAny<AppointmentSearchRequest>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_WhenRepositoryThrowsException_ReturnsFailure()
    {
        // Arrange
        var createDto = new CreateAppointmentSearchRequestDto
        {
            PatientProfileId = Guid.NewGuid(),
            LpuName = "Test Hospital",
            DoctorName = "Dr. Smith"
        };

        var exception = new Exception("Database error");

        _mockRepository
            .Setup(x => x.AddAsync(It.IsAny<AppointmentSearchRequest>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(exception);

        // Act
        var result = await _service.CreateAsync(createDto, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains($"Ошибка при создании запроса на поиск записи для пациента {createDto.PatientProfileId}")),
                exception,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_WhenRequestExists_DeletesRequest()
    {
        // Arrange
        var requestId = Guid.NewGuid();

        _mockRepository
            .Setup(x => x.DeleteAsync(requestId, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _service.DeleteAsync(requestId, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        
        _mockRepository.Verify(x => x.DeleteAsync(requestId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_WhenRepositoryThrowsException_ReturnsFailure()
    {
        // Arrange
        var requestId = Guid.NewGuid();
        var exception = new Exception("Database error");

        _mockRepository
            .Setup(x => x.DeleteAsync(requestId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(exception);

        // Act
        var result = await _service.DeleteAsync(requestId, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains($"Ошибка при удалении запроса на поиск записи {requestId}")),
                exception,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task UpdateTimePreferencesAsync_WhenRequestExists_UpdatesPreferences()
    {
        // Arrange
        var requestId = Guid.NewGuid();
        var updateDto = new UpdateTimePreferencesDto(
            RequestId: requestId,
            TimePreferencesName: "UpdatedPreset");

        var existingRequest = new AppointmentSearchRequest
        {
            Id = requestId,
            TimePreferencesPresetName = "OldPreset"
        };

        _mockRepository
            .Setup(x => x.GetByIdAsync(requestId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingRequest);

        _mockRepository
            .Setup(x => x.UpdateAsync(existingRequest, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _service.UpdateTimePreferencesAsync(updateDto, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("UpdatedPreset", existingRequest.TimePreferencesPresetName);
        
        _mockRepository.Verify(x => x.GetByIdAsync(requestId, It.IsAny<CancellationToken>()), Times.Once);
        _mockRepository.Verify(x => x.UpdateAsync(existingRequest, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateTimePreferencesAsync_WhenRequestNotFound_ReturnsFailure()
    {
        // Arrange
        var updateDto = new UpdateTimePreferencesDto(
            RequestId: Guid.NewGuid(),
            TimePreferencesName: "UpdatedPreset");

        _mockRepository
            .Setup(x => x.GetByIdAsync(updateDto.RequestId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((AppointmentSearchRequest)null);

        // Act
        var result = await _service.UpdateTimePreferencesAsync(updateDto, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("SearchRequest.NotFound", result.Error.Code);
        
        _mockRepository.Verify(x => x.UpdateAsync(It.IsAny<AppointmentSearchRequest>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task UpdateTimePreferencesAsync_WhenRepositoryThrowsException_ReturnsFailure()
    {
        // Arrange
        var updateDto = new UpdateTimePreferencesDto(
            RequestId: Guid.NewGuid(), 
            TimePreferencesName: "UpdatedPreset");

        var existingRequest = new AppointmentSearchRequest { Id = updateDto.RequestId };
        var exception = new Exception("Database error");

        _mockRepository
            .Setup(x => x.GetByIdAsync(updateDto.RequestId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingRequest);

        _mockRepository
            .Setup(x => x.UpdateAsync(existingRequest, It.IsAny<CancellationToken>()))
            .ThrowsAsync(exception);

        // Act
        var result = await _service.UpdateTimePreferencesAsync(updateDto, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains($"Ошибка при обновлении временных предпочтений для запроса {updateDto.RequestId}")),
                exception,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task GetActiveByUserAsync_WhenRequestsExist_ReturnsActiveRequests()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var requests = new List<AppointmentSearchRequest>
        {
            new()
            {
                Id = Guid.NewGuid(),
                PatientProfileId = Guid.NewGuid(),
                LpuName = "Hospital 1",
                DoctorName = "Dr. Ivanov",
                SearchInterval = TimeSpan.FromHours(1),
                SpecificStartPoints = new List<DateTime>(),
                TimePreferencesPresetName = "WorkDays",
                ViewOnly = false,
                MaxDaysToSearch = 7,
                CreatedAt = DateTime.UtcNow.AddDays(-1),
                LastSearchAttempt = DateTime.UtcNow.AddHours(-2),
                AttemptCount = 3,
                Status = SearchRequestStatus.InProgress,
                PatientProfile = new PatientProfile
                {
                    PatientLastName = "Ivanov",
                    PatientFirstName = "Ivan",
                    PatientMiddleName = "Ivanovich"
                }
            },
            new()
            {
                Id = Guid.NewGuid(),
                PatientProfileId = Guid.NewGuid(),
                LpuName = "Hospital 2",
                DoctorName = "Dr. Petrov",
                SearchInterval = TimeSpan.FromHours(2),
                SpecificStartPoints = new List<DateTime>(),
                TimePreferencesPresetName = "Weekend",
                ViewOnly = true,
                MaxDaysToSearch = 14,
                CreatedAt = DateTime.UtcNow.AddDays(-2),
                LastSearchAttempt = null,
                AttemptCount = 0,
                Status = SearchRequestStatus.Pending,
                PatientProfile = new PatientProfile
                {
                    PatientLastName = "Petrov",
                    PatientFirstName = "Petr",
                    PatientMiddleName = "Petrovich"
                }
            }
        };

        _mockRepository
            .Setup(x => x.GetActiveByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(requests);

        // Act
        var result = await _service.GetActiveByUserAsync(userId, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        var resultList = result.Value.ToList();
        
        Assert.Equal(2, resultList.Count);
        
        Assert.Equal("Hospital 1", resultList[0].LpuName);
        Assert.Equal("Dr. Ivanov", resultList[0].DoctorName);
        Assert.Equal("WorkDays", resultList[0].TimePreferencesPresetName);
        Assert.Equal("Ivanov Ivan Ivanovich", resultList[0].PatientFullName);
        Assert.Equal(SearchRequestStatus.InProgress.ToString(), resultList[0].Status);
        
        Assert.Equal("Hospital 2", resultList[1].LpuName);
        Assert.Equal("Dr. Petrov", resultList[1].DoctorName);
        Assert.Equal("Weekend", resultList[1].TimePreferencesPresetName);
        Assert.Equal("Petrov Petr Petrovich", resultList[1].PatientFullName);
        Assert.Equal(SearchRequestStatus.Pending.ToString(), resultList[1].Status);
        
        _mockRepository.Verify(x => x.GetActiveByUserIdAsync(userId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetActiveByUserAsync_WhenNoActiveRequests_ReturnsEmptyList()
    {
        // Arrange
        var userId = Guid.NewGuid();

        _mockRepository
            .Setup(x => x.GetActiveByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<AppointmentSearchRequest>());

        // Act
        var result = await _service.GetActiveByUserAsync(userId, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Empty(result.Value);
        
        _mockRepository.Verify(x => x.GetActiveByUserIdAsync(userId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetActiveByUserAsync_WhenRepositoryThrowsException_ReturnsFailure()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var exception = new Exception("Database error");

        _mockRepository
            .Setup(x => x.GetActiveByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(exception);

        // Act
        var result = await _service.GetActiveByUserAsync(userId, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains($"Ошибка при получении активных запросов пользователя {userId}")),
                exception,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task GetByPatientAsync_WhenRequestsExist_ReturnsPatientRequests()
    {
        // Arrange
        var patientProfileId = Guid.NewGuid();
        var requests = new List<AppointmentSearchRequest>
        {
            new()
            {
                Id = Guid.NewGuid(),
                PatientProfileId = patientProfileId,
                LpuName = "Test Hospital",
                DoctorName = "Dr. Smith",
                SearchInterval = TimeSpan.FromHours(1),
                SpecificStartPoints = new List<DateTime>(),
                TimePreferencesPresetName = "WorkDays",
                ViewOnly = false,
                MaxDaysToSearch = 7,
                CreatedAt = DateTime.UtcNow.AddDays(-1),
                LastSearchAttempt = DateTime.UtcNow.AddHours(-2),
                AttemptCount = 5,
                Status = SearchRequestStatus.Completed,
                PatientProfile = new PatientProfile
                {
                    PatientLastName = "Sidorov",
                    PatientFirstName = "Sidor",
                    PatientMiddleName = "Sidorovich"
                }
            }
        };

        _mockRepository
            .Setup(x => x.GetByPatientProfileIdAsync(patientProfileId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(requests);

        // Act
        var result = await _service.GetByPatientAsync(patientProfileId, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        var resultList = result.Value.ToList();
        
        Assert.Single(resultList);
        Assert.Equal(patientProfileId, resultList[0].PatientProfileId);
        Assert.Equal("Test Hospital", resultList[0].LpuName);
        Assert.Equal("Dr. Smith", resultList[0].DoctorName);
        Assert.Equal("Sidorov Sidor Sidorovich", resultList[0].PatientFullName);
        Assert.Equal(SearchRequestStatus.Completed.ToString(), resultList[0].Status);
        
        _mockRepository.Verify(x => x.GetByPatientProfileIdAsync(patientProfileId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetByPatientAsync_WhenNoRequests_ReturnsEmptyList()
    {
        // Arrange
        var patientProfileId = Guid.NewGuid();

        _mockRepository
            .Setup(x => x.GetByPatientProfileIdAsync(patientProfileId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<AppointmentSearchRequest>());

        // Act
        var result = await _service.GetByPatientAsync(patientProfileId, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Empty(result.Value);
        
        _mockRepository.Verify(x => x.GetByPatientProfileIdAsync(patientProfileId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetByPatientAsync_WhenRepositoryThrowsException_ReturnsFailure()
    {
        // Arrange
        var patientProfileId = Guid.NewGuid();
        var exception = new Exception("Database error");

        _mockRepository
            .Setup(x => x.GetByPatientProfileIdAsync(patientProfileId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(exception);

        // Act
        var result = await _service.GetByPatientAsync(patientProfileId, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains($"Ошибка при получении запросов пациента {patientProfileId}")),
                exception,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }
}
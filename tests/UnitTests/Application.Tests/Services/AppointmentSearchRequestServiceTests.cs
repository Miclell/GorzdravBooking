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
    private readonly Mock<ILogger<AppointmentSearchRequestService>> _loggerMock;
    private readonly Mock<IAppointmentSearchRequestRepository> _repositoryMock;
    private readonly AppointmentSearchRequestService _sut; // System Under Test

    public AppointmentSearchRequestServiceTests()
    {
        _repositoryMock = new Mock<IAppointmentSearchRequestRepository>();
        _loggerMock = new Mock<ILogger<AppointmentSearchRequestService>>();
        _sut = new AppointmentSearchRequestService(_repositoryMock.Object, _loggerMock.Object);
    }

    #region CreateAsync Tests

    [Fact]
    public async Task CreateAsync_ValidDto_ReturnsSuccessWithId()
    {
        // Arrange
        var dto = CreateValidDto();
        var expectedId = Guid.NewGuid();

        _repositoryMock
            .Setup(x => x.AddAsync(It.IsAny<AppointmentSearchRequest>(), default))
            .Callback<AppointmentSearchRequest, CancellationToken>((req, _) => req.Id = expectedId);

        // Act
        var result = await _sut.CreateAsync(dto);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(expectedId, result.Value);
        _repositoryMock.Verify(x => x.AddAsync(It.IsAny<AppointmentSearchRequest>(), default), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_RepositoryThrows_ReturnsFailure()
    {
        // Arrange
        var dto = CreateValidDto();
        _repositoryMock
            .Setup(x => x.AddAsync(It.IsAny<AppointmentSearchRequest>(), default))
            .ThrowsAsync(new Exception("DB Error"));

        // Act
        var result = await _sut.CreateAsync(dto);

        // Assert
        Assert.True(result.IsFailure);
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task CreateAsync_CreatesManualSearchRequest_WhenDoctorModeIsSpecific()
    {
        // Arrange
        var dto = CreateValidDto(DoctorSelectionMode.SpecificDoctorOrRange);
        ManualSearchRequest? capturedRequest = null;

        _repositoryMock
            .Setup(x => x.AddAsync(It.IsAny<AppointmentSearchRequest>(), default))
            .Callback<AppointmentSearchRequest, CancellationToken>((req, _) =>
            {
                capturedRequest = req as ManualSearchRequest;
            });

        // Act
        await _sut.CreateAsync(dto);

        // Assert
        Assert.NotNull(capturedRequest);
        Assert.Equal(dto.LpuName, capturedRequest.LpuName);
        Assert.Equal(dto.DoctorNames, capturedRequest.DoctorNames);
    }

    #endregion

    #region DeleteAsync Tests

    [Fact]
    public async Task DeleteAsync_ValidId_ReturnsSuccess()
    {
        // Arrange
        var id = Guid.NewGuid();

        // Act
        var result = await _sut.DeleteAsync(id);

        // Assert
        Assert.True(result.IsSuccess);
        _repositoryMock.Verify(x => x.DeleteAsync(id, default), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_RepositoryThrows_ReturnsFailure()
    {
        // Arrange
        var id = Guid.NewGuid();
        _repositoryMock
            .Setup(x => x.DeleteAsync(id, default))
            .ThrowsAsync(new Exception("DB Error"));

        // Act
        var result = await _sut.DeleteAsync(id);

        // Assert
        Assert.True(result.IsFailure);
    }

    #endregion

    #region UpdateTimePreferencesAsync Tests

    [Fact]
    public async Task UpdateTimePreferencesAsync_RequestExists_UpdatesAndReturnsSuccess()
    {
        // Arrange
        var requestId = Guid.NewGuid();
        var dto = new UpdateTimePreferencesDto(requestId, "NewPreset");
        var existingRequest = CreateTestRequest(requestId, presetName: "OldPreset");

        _repositoryMock
            .Setup(x => x.GetByIdAsync(requestId, default))
            .ReturnsAsync(existingRequest);

        // Act
        var result = await _sut.UpdateTimePreferencesAsync(dto);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("NewPreset", existingRequest.TimePreferencesPresetName);
        _repositoryMock.Verify(x => x.UpdateAsync(existingRequest, default), Times.Once);
    }

    [Fact]
    public async Task UpdateTimePreferencesAsync_RequestNotFound_ReturnsNotFoundError()
    {
        // Arrange
        var dto = new UpdateTimePreferencesDto(Guid.NewGuid(), "NewPreset");
        _repositoryMock
            .Setup(x => x.GetByIdAsync(dto.RequestId, default))
            .ReturnsAsync((AppointmentSearchRequest?)null);

        // Act
        var result = await _sut.UpdateTimePreferencesAsync(dto);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("SearchRequest.NotFound", result.Error.Code);
        _repositoryMock.Verify(x => x.UpdateAsync(It.IsAny<AppointmentSearchRequest>(), default), Times.Never);
    }

    [Fact]
    public async Task UpdateTimePreferencesAsync_RepositoryThrowsOnUpdate_ReturnsFailure()
    {
        // Arrange
        var dto = new UpdateTimePreferencesDto(Guid.NewGuid(), "NewPreset");
        var existingRequest = CreateTestRequest(dto.RequestId);

        _repositoryMock.Setup(x => x.GetByIdAsync(dto.RequestId, default)).ReturnsAsync(existingRequest);
        _repositoryMock.Setup(x => x.UpdateAsync(existingRequest, default)).ThrowsAsync(new Exception("DB Error"));

        // Act
        var result = await _sut.UpdateTimePreferencesAsync(dto);

        // Assert
        Assert.True(result.IsFailure);
    }

    #endregion

    #region GetActiveByUserAsync Tests

    [Fact]
    public async Task GetActiveByUserAsync_RequestsExist_ReturnsAllRequests()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var requests = new List<AppointmentSearchRequest>
        {
            CreateTestRequest(status: SearchRequestStatus.InProgress),
            CreateTestRequest(status: SearchRequestStatus.Pending)
        };

        _repositoryMock
            .Setup(x => x.GetActiveByUserIdAsync(userId, default))
            .ReturnsAsync(requests);

        // Act
        var result = await _sut.GetActiveByUserAsync(userId);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Value.Count());
    }

    [Fact]
    public async Task GetActiveByUserAsync_NoRequests_ReturnsEmptyCollection()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _repositoryMock
            .Setup(x => x.GetActiveByUserIdAsync(userId, default))
            .ReturnsAsync(new List<AppointmentSearchRequest>());

        // Act
        var result = await _sut.GetActiveByUserAsync(userId);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Empty(result.Value);
    }

    [Fact]
    public async Task GetActiveByUserAsync_RepositoryThrows_ReturnsFailure()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _repositoryMock
            .Setup(x => x.GetActiveByUserIdAsync(userId, default))
            .ThrowsAsync(new Exception("DB Error"));

        // Act
        var result = await _sut.GetActiveByUserAsync(userId);

        // Assert
        Assert.True(result.IsFailure);
    }

    #endregion

    #region GetByPatientAsync Tests

    [Fact]
    public async Task GetByPatientAsync_RequestsExist_ReturnsPatientRequests()
    {
        // Arrange
        var patientId = Guid.NewGuid();
        var requests = new List<AppointmentSearchRequest>
        {
            CreateTestRequest(patientProfileId: patientId)
        };

        _repositoryMock
            .Setup(x => x.GetByPatientProfileIdAsync(patientId, default))
            .ReturnsAsync(requests);

        // Act
        var result = await _sut.GetByPatientAsync(patientId);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Single(result.Value);
    }

    [Fact]
    public async Task GetByPatientAsync_NoRequests_ReturnsEmptyCollection()
    {
        // Arrange
        var patientId = Guid.NewGuid();
        _repositoryMock
            .Setup(x => x.GetByPatientProfileIdAsync(patientId, default))
            .ReturnsAsync(new List<AppointmentSearchRequest>());

        // Act
        var result = await _sut.GetByPatientAsync(patientId);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Empty(result.Value);
    }

    [Fact]
    public async Task GetByPatientAsync_RepositoryThrows_ReturnsFailure()
    {
        // Arrange
        var patientId = Guid.NewGuid();
        _repositoryMock
            .Setup(x => x.GetByPatientProfileIdAsync(patientId, default))
            .ThrowsAsync(new Exception("DB Error"));

        // Act
        var result = await _sut.GetByPatientAsync(patientId);

        // Assert
        Assert.True(result.IsFailure);
    }

    #endregion

    #region Test Data Helpers

    private static CreateAppointmentSearchRequestDto CreateValidDto(
        DoctorSelectionMode doctorMode = DoctorSelectionMode.SpecificDoctorOrRange)
    {
        return new CreateAppointmentSearchRequestDto
        {
            PatientProfileId = Guid.NewGuid(),
            LpuName = "Test Hospital",
            Speciality = "Test Speciality",
            DoctorMode = doctorMode,
            DoctorIds = ["doc123"],
            DoctorNames = ["Dr. Smith"],
            TimePreferencesPresetName = "WorkDays",
            SearchInterval = TimeSpan.FromHours(2),
            SpecificStartPoints = [DateTime.Now.AddDays(1)],
            MaxDaysToSearch = 7,
            ViewOnly = false
        };
    }

    private static ManualSearchRequest CreateTestRequest(
        Guid? id = null,
        Guid? patientProfileId = null,
        string? presetName = null,
        SearchRequestStatus status = SearchRequestStatus.Pending)
    {
        return new ManualSearchRequest
        {
            Id = id ?? Guid.NewGuid(),
            PatientProfileId = patientProfileId ?? Guid.NewGuid(),
            LpuName = "Test Hospital",
            DoctorNames = ["Dr. Test"],
            TimePreferencesPresetName = presetName ?? "TestPreset",
            SearchInterval = TimeSpan.FromHours(1),
            SpecificStartPoints = [],
            ViewOnly = false,
            MaxDaysToSearch = 7,
            Status = status,
            PatientProfile = new PatientProfile
            {
                PatientLastName = "Test",
                PatientFirstName = "User",
                PatientMiddleName = "Middle"
            }
        };
    }

    #endregion
}
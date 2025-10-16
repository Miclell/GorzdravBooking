using Application.Common.Results;
using Application.Coordinators.Implementation;
using Application.DTOs.Appointment;
using Application.DTOs.TimePreferences;
using Application.Services.Interfaces;
using Core.Models;
using Moq;
using Xunit;
using IAppointmentService = Application.Services.Interfaces.IAppointmentService;

namespace Application.Tests.Coordinators;

public class AppointmentCoordinatorTests
{
    private readonly Mock<Core.Interfaces.Services.IAppointmentService> _mockExternalService;
    private readonly Mock<ITimePreferencesService> _mockTimePreferencesService;
    private readonly Mock<IAppointmentService> _mockAppointmentService;
    private readonly AppointmentCoordinator _coordinator;

    public AppointmentCoordinatorTests()
    {
        _mockExternalService = new Mock<Core.Interfaces.Services.IAppointmentService>();
        _mockTimePreferencesService = new Mock<ITimePreferencesService>();
        _mockAppointmentService = new Mock<IAppointmentService>();
        
        _coordinator = new AppointmentCoordinator(
            _mockExternalService.Object,
            _mockTimePreferencesService.Object,
            _mockAppointmentService.Object);
    }

    [Fact]
    public async Task CreateCompleteAppointmentAsync_WhenTimePreferencesNotFound_ReturnsFailure()
    {
        // Arrange
        var request = CreateTestRequest();
        _mockTimePreferencesService
            .Setup(x => x.GetByPresetAsync(request.PatientProfileId, request.TimePreferencesPresetName, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Error.Failure("Not found", "Not Found"));

        // Act
        var result = await _coordinator.CreateCompleteAppointmentAsync(request, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        _mockExternalService.Verify(x => x.GetByDoctorAsync(It.IsAny<int>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task CreateCompleteAppointmentAsync_WhenAppointmentFound_CreatesInBothServices()
    {
        // Arrange
        var request = CreateTestRequest();
        var timePreferences = new TimePreferencesPresetDto("test", request.PatientProfileId, false, 
            new List<TimePreferenceDto> { new(DayOfWeek.Monday, null, null) });
        
        var appointments = new List<Appointment> { CreateTestAppointment() };

        _mockTimePreferencesService
            .Setup(x => x.GetByPresetAsync(request.PatientProfileId, request.TimePreferencesPresetName, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(timePreferences));

        _mockExternalService
            .Setup(x => x.GetByDoctorAsync(It.IsAny<int>(), request.DoctorId))
            .ReturnsAsync(appointments);

        _mockExternalService
            .Setup(x => x.CreateAppointmentAsync(It.IsAny<AppointmentCreateRequest>()))
            .ReturnsAsync(true);

        // Act
        var result = await _coordinator.CreateCompleteAppointmentAsync(request, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.True(result.Value); // возвращает true когда запись создана
        
        _mockExternalService.Verify(x => x.CreateAppointmentAsync(It.IsAny<AppointmentCreateRequest>()), Times.Once);
        _mockAppointmentService.Verify(x => x.CreateAsync(It.IsAny<CreateAppointmentDto>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    private static Core.Entities.AppointmentSearchRequest CreateTestRequest() => new()
    {
        PatientProfileId = Guid.NewGuid(),
        TimePreferencesPresetName = "test",
        DoctorId = "doc123",
        PatientProfile = new Core.Entities.PatientProfile
        {
            LpuId = "123",
            PatientId = "patient123",
            RecipientEmail = "test@test.com",
            PatientLastName = "Ivanov",
            PatientFirstName = "Ivan", 
            PatientMiddleName = "Ivanovich",
            PatientBirthdate = new DateTime(1990, 1, 1)
        }
    };

    private static Appointment CreateTestAppointment() => new()
    {
        Id = "app123",
        VisitStart = new DateTime(2024, 1, 1, 10, 0, 0), // Monday
        VisitEnd = new DateTime(2024, 1, 1, 11, 0, 0),
        Address = "Test Address",
        Room = "101",
        Number = "A101"
    };
}
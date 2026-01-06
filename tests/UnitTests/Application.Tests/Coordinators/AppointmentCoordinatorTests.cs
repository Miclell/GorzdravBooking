using Application.Common.Results;
using Application.Coordinators.Implementation;
using Application.DTOs.Appointment;
using Application.DTOs.TimePreferences;
using Application.Services.Interfaces;
using Core.Models;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using IAppointmentService = Application.Services.Interfaces.IAppointmentService;

namespace Application.Tests.Coordinators;

public class AppointmentCoordinatorTests
{
    private readonly Mock<Core.Interfaces.Services.IExternalAppointmentService> _mockExternalService;
    private readonly Mock<ITimePreferencesService> _mockTimePreferencesService;
    private readonly Mock<IAppointmentService> _mockAppointmentService;
    private readonly Mock<ILogger<AppointmentCoordinator>> _mockLogger;
    private readonly AppointmentCoordinator _coordinator;

    public AppointmentCoordinatorTests()
    {
        _mockExternalService = new Mock<Core.Interfaces.Services.IExternalAppointmentService>();
        _mockTimePreferencesService = new Mock<ITimePreferencesService>();
        _mockAppointmentService = new Mock<IAppointmentService>();
        _mockLogger = new Mock<ILogger<AppointmentCoordinator>>();
        _mockLogger
            .Setup(x => x.Log(
                It.IsAny<LogLevel>(),
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()))
            .Callback(new InvocationAction(invocation =>
            {
                var logLevel = (LogLevel)invocation.Arguments[0];
                var eventId = (EventId)invocation.Arguments[1];
                var state = invocation.Arguments[2];
                var exception = (Exception)invocation.Arguments[3];
                var formatter = invocation.Arguments[4];
            
                var formattedMessage = formatter.GetType().GetMethod("Invoke")?
                    .Invoke(formatter, new[] { state, exception }) as string;
                
                Console.WriteLine($"{DateTime.Now:HH:mm:ss} [{logLevel}] {formattedMessage}");
            }));
        
        _coordinator = new AppointmentCoordinator(
            _mockExternalService.Object,
            _mockTimePreferencesService.Object,
            _mockAppointmentService.Object,
            _mockLogger.Object);
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
            .Setup(x => x.GetByPresetAsync(request.PatientProfile.UserId, request.TimePreferencesPresetName, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(timePreferences));

        _mockExternalService
            .Setup(x => x.GetByDoctorAsync(It.IsAny<int>(), request.DoctorIds))
            .ReturnsAsync(appointments);

        _mockExternalService
            .Setup(x => x.CreateAppointmentAsync(It.IsAny<AppointmentCreateRequest>()))
            .ReturnsAsync((true, 0));

        // Act
        var result = await _coordinator.CreateCompleteAppointmentAsync(request, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.True(result.Value); // возвращает true когда запись создана
        
        _mockExternalService.Verify(x => x.CreateAppointmentAsync(It.IsAny<AppointmentCreateRequest>()), Times.Once);
        _mockAppointmentService.Verify(x => x.CreateAsync(It.IsAny<CreateAppointmentDto>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    private static Core.Entities.ManualSearchRequest CreateTestRequest() => new()
    {
        PatientProfileId = Guid.NewGuid(),
        TimePreferencesPresetName = "test",
        DoctorIds = "doc123",
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
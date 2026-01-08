using Application.Common.Results;
using Application.Coordinators.Implementation;
using Application.DTOs.Appointment;
using Application.DTOs.TimePreferences;
using Application.Services.Interfaces;
using Core.Entities;
using Core.Enums;
using Core.Interfaces.Services;
using Core.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Time.Testing;
using Moq;
using Xunit;
using Appointment = Core.Models.Appointment;

namespace Application.Tests.Coordinators;

public class AppointmentCoordinatorTests
{
    private readonly Mock<IAppointmentService> _appointmentServiceMock;
    private readonly Mock<IExternalAppointmentService> _externalServiceMock;
    private readonly AppointmentCoordinator _sut;
    private readonly Mock<ITimePreferencesService> _timePreferencesServiceMock;

    public AppointmentCoordinatorTests()
    {
        _externalServiceMock = new Mock<IExternalAppointmentService>();
        _timePreferencesServiceMock = new Mock<ITimePreferencesService>();
        _appointmentServiceMock = new Mock<IAppointmentService>();
        var timeProviderMock = new FakeTimeProvider(
            new DateTimeOffset(2024, 1, 1, 8, 0, 0, TimeSpan.FromHours(3))
        );
        var loggerMock = new Mock<ILogger<AppointmentCoordinator>>();

        _sut = new AppointmentCoordinator(
            _externalServiceMock.Object,
            _timePreferencesServiceMock.Object,
            _appointmentServiceMock.Object,
            timeProviderMock,
            loggerMock.Object);
    }

    [Fact]
    public async Task CreateCompleteAppointmentAsync_TimePreferencesNotFound_ReturnsFailure()
    {
        // Arrange
        var request = CreateTestRequest();
        _timePreferencesServiceMock
            .Setup(x => x.GetByPresetAsync(request.PatientProfile.UserId, request.TimePreferencesPresetName, default))
            .ReturnsAsync(Error.Failure("Not found", "Not Found"));

        // Act
        var result = await _sut.CreateCompleteAppointmentAsync(request);

        // Assert
        Assert.True(result.IsFailure);
        _externalServiceMock.Verify(x => x.GetByDoctorAsync(It.IsAny<int>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task CreateCompleteAppointmentAsync_AppointmentFound_CreatesSuccessfully()
    {
        // Arrange
        var request = CreateTestRequest();
        var timePreferences = CreateTimePreferences(request.PatientProfile.UserId);
        var appointments = new List<Appointment> { CreateTestAppointment() };

        _timePreferencesServiceMock
            .Setup(x => x.GetByPresetAsync(request.PatientProfile.UserId, request.TimePreferencesPresetName, default))
            .ReturnsAsync(Result.Success(timePreferences));

        _externalServiceMock
            .Setup(x => x.GetByDoctorAsync(It.IsAny<int>(), It.IsAny<string>()))
            .ReturnsAsync(appointments);

        _externalServiceMock
            .Setup(x => x.CreateAppointmentAsync(It.IsAny<AppointmentCreateRequest>()))
            .ReturnsAsync((true, 0));

        _appointmentServiceMock
            .Setup(x => x.CreateAsync(It.IsAny<CreateAppointmentDto>(), default))
            .ReturnsAsync(Result.Success(Guid.NewGuid()));

        // Act
        var result = await _sut.CreateCompleteAppointmentAsync(request);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.True(result.Value);

        _externalServiceMock.Verify(x => x.CreateAppointmentAsync(It.IsAny<AppointmentCreateRequest>()), Times.Once);
        _appointmentServiceMock.Verify(x => x.CreateAsync(It.IsAny<CreateAppointmentDto>(), default), Times.Once);
    }

    [Fact]
    public async Task CreateCompleteAppointmentAsync_Error639_RetriesThreeTimes()
    {
        // Arrange
        var request = CreateTestRequest();
        var timePreferences = CreateTimePreferences(request.PatientProfile.UserId);
        var appointments = new List<(string, Appointment)> { ("Dr. Smith", CreateTestAppointment()) };

        _timePreferencesServiceMock
            .Setup(x => x.GetByPresetAsync(It.IsAny<Guid>(), It.IsAny<string>(), default))
            .ReturnsAsync(Result.Success(timePreferences));

        _externalServiceMock
            .Setup(x => x.GetByDoctorAsync(It.IsAny<int>(), It.IsAny<string>()))
            .ReturnsAsync(appointments.Select(a => a.Item2).ToList());

        _externalServiceMock
            .Setup(x => x.CreateAppointmentAsync(It.IsAny<AppointmentCreateRequest>()))
            .ReturnsAsync((false, 639)); // ← Race condition каждый раз

        // Act
        var result = await _sut.CreateCompleteAppointmentAsync(request);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("Нас обогнала бабуля!", result.Error.Description);

        // ✅ Проверяем 3 попытки
        _externalServiceMock.Verify(
            x => x.CreateAppointmentAsync(It.IsAny<AppointmentCreateRequest>()),
            Times.Exactly(3));
    }

    [Fact]
    public async Task CreateCompleteAppointmentAsync_ViewOnlyMode_DoesNotBook()
    {
        // Arrange
        var request = CreateTestRequest();
        request.ViewOnly = true;
        var timePreferences = CreateTimePreferences(request.PatientProfile.UserId);
        var appointments = new List<(string, Appointment)> { ("Dr. Smith", CreateTestAppointment()) };

        _timePreferencesServiceMock
            .Setup(x => x.GetByPresetAsync(It.IsAny<Guid>(), It.IsAny<string>(), default))
            .ReturnsAsync(Result.Success(timePreferences));

        _externalServiceMock
            .Setup(x => x.GetByDoctorAsync(It.IsAny<int>(), It.IsAny<string>()))
            .ReturnsAsync(appointments.Select(a => a.Item2).ToList());

        // Act
        var result = await _sut.CreateCompleteAppointmentAsync(request);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.False(result.Value); // ← Не забронировано

        _externalServiceMock.Verify(
            x => x.CreateAppointmentAsync(It.IsAny<AppointmentCreateRequest>()),
            Times.Never); // ✅ Не вызывали бронирование
    }

    [Fact]
    public async Task CreateCompleteAppointmentAsync_ExceptionThrown_ReturnsFailure()
    {
        // Arrange
        var request = CreateTestRequest();
        _timePreferencesServiceMock
            .Setup(x => x.GetByPresetAsync(It.IsAny<Guid>(), It.IsAny<string>(), default))
            .ThrowsAsync(new InvalidOperationException("DB connection failed"));

        // Act
        var result = await _sut.CreateCompleteAppointmentAsync(request);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Contains("Booking appointment error", result.Error.Description);
    }

    [Fact]
    public async Task CreateCompleteAppointmentAsync_ExcludedDate_SkipsAppointment()
    {
        // Arrange
        var request = CreateTestRequest();
        var excludedDate = DateOnly.FromDateTime(new DateTime(2024, 1, 1));
        var timePreferences = new TimePreferencesPresetDto(
            "WorkDays",
            request.PatientProfile.UserId,
            TimeSelectionMode.WeekdayPattern,
            new List<TimePreferenceDto> { new(null, DayOfWeek.Monday, new TimeOnly(9, 0), new TimeOnly(12, 0)) },
            new List<DateOnly> { excludedDate }, // ← Исключена дата
            14,
            2
        );

        var appointmentOnExcludedDate = new Appointment
        {
            Id = "123",
            VisitStart = new DateTime(2024, 1, 1, 10, 0, 0), // ← Попадает в ExcludedDates
            VisitEnd = new DateTime(2024, 1, 1, 11, 0, 0),
            Address = "Test",
            Room = "101",
            Number = "A101"
        };

        _timePreferencesServiceMock
            .Setup(x => x.GetByPresetAsync(It.IsAny<Guid>(), It.IsAny<string>(), default))
            .ReturnsAsync(Result.Success(timePreferences));

        _externalServiceMock
            .Setup(x => x.GetByDoctorAsync(It.IsAny<int>(), It.IsAny<string>()))
            .ReturnsAsync([appointmentOnExcludedDate]);

        // Act
        var result = await _sut.CreateCompleteAppointmentAsync(request);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.False(result.Value); // ← Не нашли подходящий слот

        _externalServiceMock.Verify(
            x => x.CreateAppointmentAsync(It.IsAny<AppointmentCreateRequest>()),
            Times.Never);
    }


    #region Test Data Helpers

    private static ManualSearchRequest CreateTestRequest()
    {
        return new ManualSearchRequest
        {
            PatientProfileId = Guid.NewGuid(),
            PatientProfile = new PatientProfile
            {
                UserId = Guid.NewGuid(),
                LpuId = "123",
                PatientId = "patient123",
                RecipientEmail = "test@test.com",
                PatientLastName = "Ivanov",
                PatientFirstName = "Ivan",
                PatientMiddleName = "Ivanovich",
                PatientBirthdate = new DateTime(1990, 1, 1)
            },
            LpuName = "TestLpu",
            Speciality = "Test spec",
            DoctorMode = DoctorSelectionMode.SpecificDoctorOrRange,
            DoctorIds = ["doc123"],
            DoctorNames = ["Dr. Smith"],
            TimePreferencesPresetName = "WorkDays"
        };
    }

    private static TimePreferencesPresetDto CreateTimePreferences(Guid userId)
    {
        return new TimePreferencesPresetDto(
            "WorkDays",
            userId,
            TimeSelectionMode.WeekdayPattern,
            new List<TimePreferenceDto>
            {
                new(null, DayOfWeek.Monday, new TimeOnly(9, 0), new TimeOnly(12, 0))
            },
            new List<DateOnly>(),
            14,
            2
        );
    }

    private static Appointment CreateTestAppointment()
    {
        return new Appointment
        {
            Id = "123",
            VisitStart = new DateTime(2024, 1, 1, 10, 0, 0),
            VisitEnd = new DateTime(2024, 1, 1, 11, 0, 0),
            Address = "Test Address",
            Room = "101",
            Number = "A101"
        };
    }

    #endregion
}
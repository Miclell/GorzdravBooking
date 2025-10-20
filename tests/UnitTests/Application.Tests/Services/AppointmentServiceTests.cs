using Application.DTOs.Appointment;
using Application.Services.Implementation;
using Core.Entities;
using Core.Interfaces.Repositories;
using Core.Models;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Application.Tests.Services;

public class AppointmentServiceTests
{
    private readonly Mock<IAppointmentRepository> _mockAppointmentRepository;
    private readonly Mock<Core.Interfaces.Services.IAppointmentService> _mockExternalAppointmentService;
    private readonly Mock<ILogger<AppointmentService>> _mockLogger;
    private readonly AppointmentService _appointmentService;

    public AppointmentServiceTests()
    {
        _mockAppointmentRepository = new Mock<IAppointmentRepository>();
        _mockExternalAppointmentService = new Mock<Core.Interfaces.Services.IAppointmentService>();
        _mockLogger = new Mock<ILogger<AppointmentService>>();
        _appointmentService = new AppointmentService(
            _mockAppointmentRepository.Object, 
            _mockExternalAppointmentService.Object, 
            _mockLogger.Object);
    }

    [Fact]
    public async Task CreateAsync_WithValidData_CreatesAppointment()
    {
        // Arrange
        var createDto = new CreateAppointmentDto
        {
            PatientProfileId = Guid.NewGuid(),
            AppointmentId = "ext123",
            VisitStart = new DateTime(2024, 1, 1, 10, 0, 0),
            VisitEnd = new DateTime(2024, 1, 1, 11, 0, 0),
            Address = "Test Address",
            Number = "A101",
            Room = "101"
        };

        var expectedAppointmentId = Guid.NewGuid();

        _mockAppointmentRepository
            .Setup(x => x.AddAsync(It.IsAny<Core.Entities.Appointment>(), It.IsAny<CancellationToken>()))
            .Callback<Core.Entities.Appointment, CancellationToken>((appointment, _) => appointment.Id = expectedAppointmentId);

        // Act
        var result = await _appointmentService.CreateAsync(createDto, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(expectedAppointmentId, result.Value);
        
        _mockAppointmentRepository.Verify(x => x.AddAsync(It.IsAny<Core.Entities.Appointment>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_WhenRepositoryThrowsException_ReturnsFailure()
    {
        // Arrange
        var createDto = new CreateAppointmentDto
        {
            PatientProfileId = Guid.NewGuid(),
            AppointmentId = "ext123",
            VisitStart = DateTime.Now,
            VisitEnd = DateTime.Now.AddHours(1),
            Address = "Test Address",
            Number = "A101",
            Room = "101"
        };

        var exception = new Exception("Database error");

        _mockAppointmentRepository
            .Setup(x => x.AddAsync(It.IsAny<Core.Entities.Appointment>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(exception);

        // Act
        var result = await _appointmentService.CreateAsync(createDto, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains($"Ошибка при создании записи на прием для пациента {createDto.PatientProfileId}")),
                exception,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_WhenAppointmentExistsAndExternalCancelSuccess_DeletesAppointment()
    {
        // Arrange
        var appointmentId = Guid.NewGuid();
        var appointment = new Core.Entities.Appointment
        {
            Id = appointmentId,
            AppointmentId = "ext123",
            PatientProfile = new PatientProfile
            {
                LpuId = "lpu123",
                PatientId = "patient123",
                PatientLastName = "Ivanov",
                PatientFirstName = "Ivan",
                PatientMiddleName = "Ivanovich"
            }
        };

        _mockAppointmentRepository
            .Setup(x => x.GetByIdAsync(appointmentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(appointment);

        _mockExternalAppointmentService
            .Setup(x => x.CancelAppointmentAsync(It.IsAny<AppointmentСancelRequest>()))
            .ReturnsAsync(true);

        _mockAppointmentRepository
            .Setup(x => x.DeleteAsync(appointmentId, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _appointmentService.DeleteAsync(appointmentId, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        
        _mockAppointmentRepository.Verify(x => x.GetByIdAsync(appointmentId, It.IsAny<CancellationToken>()), Times.Once);
        _mockExternalAppointmentService.Verify(x => x.CancelAppointmentAsync(It.IsAny<AppointmentСancelRequest>()), Times.Once);
        _mockAppointmentRepository.Verify(x => x.DeleteAsync(appointmentId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_WhenAppointmentNotFound_ReturnsFailure()
    {
        // Arrange
        var appointmentId = Guid.NewGuid();

        _mockAppointmentRepository
            .Setup(x => x.GetByIdAsync(appointmentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Core.Entities.Appointment)null);

        // Act
        var result = await _appointmentService.DeleteAsync(appointmentId, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("Appointment.NotFound", result.Error.Code);
        
        _mockExternalAppointmentService.Verify(x => x.CancelAppointmentAsync(It.IsAny<AppointmentСancelRequest>()), Times.Never);
        _mockAppointmentRepository.Verify(x => x.DeleteAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task DeleteAsync_WhenExternalCancelFails_ReturnsFailure()
    {
        // Arrange
        var appointmentId = Guid.NewGuid();
        var appointment = new Core.Entities.Appointment
        {
            Id = appointmentId,
            AppointmentId = "ext123",
            PatientProfile = new PatientProfile
            {
                LpuId = "lpu123",
                PatientId = "patient123"
            }
        };

        _mockAppointmentRepository
            .Setup(x => x.GetByIdAsync(appointmentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(appointment);

        _mockExternalAppointmentService
            .Setup(x => x.CancelAppointmentAsync(It.IsAny<AppointmentСancelRequest>()))
            .ReturnsAsync(false);

        // Act
        var result = await _appointmentService.DeleteAsync(appointmentId, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("External.Cancel.Failed", result.Error.Code);
        
        _mockExternalAppointmentService.Verify(x => x.CancelAppointmentAsync(It.IsAny<AppointmentСancelRequest>()), Times.Once);
        _mockAppointmentRepository.Verify(x => x.DeleteAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
        
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Не удалось отменить запись во внешней системе")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_WhenRepositoryThrowsException_ReturnsFailure()
    {
        // Arrange
        var appointmentId = Guid.NewGuid();
        var appointment = new Core.Entities.Appointment
        {
            Id = appointmentId,
            AppointmentId = "ext123",
            PatientProfile = new PatientProfile { LpuId = "lpu123", PatientId = "patient123" }
        };
        var exception = new Exception("Database error");

        _mockAppointmentRepository
            .Setup(x => x.GetByIdAsync(appointmentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(appointment);

        _mockExternalAppointmentService
            .Setup(x => x.CancelAppointmentAsync(It.IsAny<AppointmentСancelRequest>()))
            .ReturnsAsync(true);

        _mockAppointmentRepository
            .Setup(x => x.DeleteAsync(appointmentId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(exception);

        // Act
        var result = await _appointmentService.DeleteAsync(appointmentId, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains($"Ошибка при удалении записи на прием {appointmentId}")),
                exception,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task GetByUserAsync_WhenAppointmentsExist_ReturnsAppointments()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var appointments = new List<Core.Entities.Appointment>
        {
            new()
            {
                Id = Guid.NewGuid(),
                VisitStart = new DateTime(2024, 1, 1, 10, 0, 0),
                VisitEnd = new DateTime(2024, 1, 1, 11, 0, 0),
                Address = "Address 1",
                Number = "A101",
                PatientProfile = new PatientProfile
                {
                    PatientLastName = "Ivanov",
                    PatientFirstName = "Ivan",
                    PatientMiddleName = "Ivanovich",
                    LpuShortName = "Hospital 1"
                }
            },
            new()
            {
                Id = Guid.NewGuid(),
                VisitStart = new DateTime(2024, 1, 2, 14, 0, 0),
                VisitEnd = new DateTime(2024, 1, 2, 15, 0, 0),
                Address = "Address 2",
                Number = "B202",
                PatientProfile = new PatientProfile
                {
                    PatientLastName = "Petrov",
                    PatientFirstName = "Petr",
                    PatientMiddleName = "Petrovich",
                    LpuShortName = "Hospital 2"
                }
            }
        };

        _mockAppointmentRepository
            .Setup(x => x.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(appointments);

        // Act
        var result = await _appointmentService.GetByUserAsync(userId, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        var resultList = result.Value.ToList();
        
        Assert.Equal(2, resultList.Count);
        Assert.Equal("Ivanov Ivan Ivanovich", resultList[0].PatientFullName);
        Assert.Equal("Hospital 1", resultList[0].LpuShortName);
        Assert.Equal("Petrov Petr Petrovich", resultList[1].PatientFullName);
        Assert.Equal("Hospital 2", resultList[1].LpuShortName);
        
        _mockAppointmentRepository.Verify(x => x.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetByUserAsync_WhenNoAppointments_ReturnsEmptyList()
    {
        // Arrange
        var userId = Guid.NewGuid();

        _mockAppointmentRepository
            .Setup(x => x.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Core.Entities.Appointment>());

        // Act
        var result = await _appointmentService.GetByUserAsync(userId, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Empty(result.Value);
        
        _mockAppointmentRepository.Verify(x => x.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetByPatientAsync_WhenAppointmentsExist_ReturnsAppointments()
    {
        // Arrange
        var patientProfileId = Guid.NewGuid();
        var appointments = new List<Core.Entities.Appointment>
        {
            new()
            {
                Id = Guid.NewGuid(),
                PatientProfileId = patientProfileId,
                AppointmentId = "ext123",
                VisitStart = new DateTime(2024, 1, 1, 10, 0, 0),
                VisitEnd = new DateTime(2024, 1, 1, 11, 0, 0),
                Address = "Test Address",
                Number = "A101",
                Room = "101",
                PatientProfile = new PatientProfile
                {
                    PatientLastName = "Ivanov",
                    PatientFirstName = "Ivan",
                    PatientMiddleName = "Ivanovich",
                    LpuShortName = "Test Hospital"
                }
            }
        };

        _mockAppointmentRepository
            .Setup(x => x.GetByPatientProfileIdAsync(patientProfileId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(appointments);

        // Act
        var result = await _appointmentService.GetByPatientAsync(patientProfileId, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        var resultList = result.Value.ToList();
        
        Assert.Single(resultList);
        Assert.Equal(patientProfileId, resultList[0].PatientProfileId);
        Assert.Equal("ext123", resultList[0].AppointmentId);
        Assert.Equal("Ivanov Ivan Ivanovich", resultList[0].PatientFullName);
        
        _mockAppointmentRepository.Verify(x => x.GetByPatientProfileIdAsync(patientProfileId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetByIdAsync_WhenAppointmentExists_ReturnsAppointment()
    {
        // Arrange
        var appointmentId = Guid.NewGuid();
        var appointment = new Core.Entities.Appointment
        {
            Id = appointmentId,
            PatientProfileId = Guid.NewGuid(),
            AppointmentId = "ext123",
            VisitStart = new DateTime(2024, 1, 1, 10, 0, 0),
            VisitEnd = new DateTime(2024, 1, 1, 11, 0, 0),
            Address = "Test Address",
            Number = "A101",
            Room = "101",
            PatientProfile = new PatientProfile
            {
                PatientLastName = "Ivanov",
                PatientFirstName = "Ivan",
                PatientMiddleName = "Ivanovich",
                LpuShortName = "Test Hospital"
            }
        };

        _mockAppointmentRepository
            .Setup(x => x.GetByIdAsync(appointmentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(appointment);

        // Act
        var result = await _appointmentService.GetByIdAsync(appointmentId, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        var dto = result.Value;
        
        Assert.Equal(appointmentId, dto.Id);
        Assert.Equal("ext123", dto.AppointmentId);
        Assert.Equal("Ivanov Ivan Ivanovich", dto.PatientFullName);
        Assert.Equal("Test Hospital", dto.LpuShortName);
        
        _mockAppointmentRepository.Verify(x => x.GetByIdAsync(appointmentId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetByIdAsync_WhenAppointmentNotFound_ReturnsFailure()
    {
        // Arrange
        var appointmentId = Guid.NewGuid();

        _mockAppointmentRepository
            .Setup(x => x.GetByIdAsync(appointmentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Core.Entities.Appointment)null);

        // Act
        var result = await _appointmentService.GetByIdAsync(appointmentId, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("Appointment.NotFound", result.Error.Code);
        
        _mockAppointmentRepository.Verify(x => x.GetByIdAsync(appointmentId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetByIdAsync_WhenRepositoryThrowsException_ReturnsFailure()
    {
        // Arrange
        var appointmentId = Guid.NewGuid();
        var exception = new Exception("Database error");

        _mockAppointmentRepository
            .Setup(x => x.GetByIdAsync(appointmentId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(exception);

        // Act
        var result = await _appointmentService.GetByIdAsync(appointmentId, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains($"Ошибка при получении записи {appointmentId}")),
                exception,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }
}
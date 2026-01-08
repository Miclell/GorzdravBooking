using Application.DTOs.Patient;
using Application.Services.Implementation;
using Core.Entities;
using Core.Interfaces.Repositories;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Application.Tests.Services;

public class PatientServiceTests
{
    private readonly Mock<ILogger<PatientService>> _mockLogger;
    private readonly Mock<IPatientRepository> _mockPatientRepository;
    private readonly PatientService _patientService;

    public PatientServiceTests()
    {
        _mockPatientRepository = new Mock<IPatientRepository>();
        _mockLogger = new Mock<ILogger<PatientService>>();
        _patientService = new PatientService(_mockPatientRepository.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task Create_WithValidData_CreatesPatient()
    {
        // Arrange
        var createPatientDto = new CreatePatientDto(
            Guid.NewGuid(),
            "123",
            "Test Hospital",
            "Test Address",
            "patient123",
            "Ivanov",
            "Ivan",
            "Ivanovich",
            new DateTime(1990, 1, 1),
            "test@test.com",
            "+79991234567"
        );

        var expectedPatientId = Guid.NewGuid();

        _mockPatientRepository
            .Setup(x => x.AddAsync(It.IsAny<PatientProfile>(), It.IsAny<CancellationToken>()))
            .Callback<PatientProfile, CancellationToken>((patient, _) => patient.Id = expectedPatientId);

        // Act
        var result = await _patientService.Create(createPatientDto, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(expectedPatientId, result.Value);

        _mockPatientRepository.Verify(x => x.AddAsync(It.IsAny<PatientProfile>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Create_WhenRepositoryThrowsException_ReturnsFailure()
    {
        // Arrange
        var createPatientDto = new CreatePatientDto(
            Guid.NewGuid(),
            "123",
            "Test Hospital",
            "Test Address",
            "patient123",
            "Ivanov",
            "Ivan",
            "Ivanovich",
            new DateTime(1990, 1, 1),
            "test@test.com",
            "+79991234567"
        );

        var exception = new Exception("Database error");

        _mockPatientRepository
            .Setup(x => x.AddAsync(It.IsAny<PatientProfile>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(exception);

        // Act
        var result = await _patientService.Create(createPatientDto, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);

        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Ошибка при добавлении PatientProfile")),
                exception,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task Delete_WhenPatientExists_DeletesPatient()
    {
        // Arrange
        var patientId = Guid.NewGuid();

        _mockPatientRepository
            .Setup(x => x.DeleteAsync(patientId, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _patientService.Delete(patientId, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);

        _mockPatientRepository.Verify(x => x.DeleteAsync(patientId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Delete_WhenRepositoryThrowsException_ReturnsFailure()
    {
        // Arrange
        var patientId = Guid.NewGuid();
        var exception = new Exception("Database error");

        _mockPatientRepository
            .Setup(x => x.DeleteAsync(patientId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(exception);

        // Act
        var result = await _patientService.Delete(patientId, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);

        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains($"Ошибка при удалении пациента с id {patientId}")),
                exception,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task GetByUser_WhenPatientsExist_ReturnsPatients()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var patientProfiles = new List<PatientProfile>
        {
            new()
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                LpuShortName = "Hospital 1",
                PatientLastName = "Ivanov",
                PatientFirstName = "Ivan",
                PatientMiddleName = "Ivanovich",
                PatientBirthdate = new DateTime(1990, 1, 1),
                RecipientEmail = "ivanov@test.com",
                MobilePhoneNumber = "+79991111111"
            },
            new()
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                LpuShortName = "Hospital 2",
                PatientLastName = "Petrov",
                PatientFirstName = "Petr",
                PatientMiddleName = "Petrovich",
                PatientBirthdate = new DateTime(1985, 5, 15),
                RecipientEmail = "petrov@test.com",
                MobilePhoneNumber = "+79992222222"
            }
        };

        _mockPatientRepository
            .Setup(x => x.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(patientProfiles);

        // Act
        var result = await _patientService.GetByUser(userId, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        var patients = result.Value.ToList();
        Assert.Equal(2, patients.Count);

        Assert.Equal(patientProfiles[0].LpuShortName, patients[0].LpuShortName);
        Assert.Equal(patientProfiles[0].PatientLastName, patients[0].PatientLastName);
        Assert.Equal(patientProfiles[1].LpuShortName, patients[1].LpuShortName);
        Assert.Equal(patientProfiles[1].PatientLastName, patients[1].PatientLastName);

        _mockPatientRepository.Verify(x => x.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetByUser_WhenNoPatients_ReturnsEmptyList()
    {
        // Arrange
        var userId = Guid.NewGuid();

        _mockPatientRepository
            .Setup(x => x.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<PatientProfile>());

        // Act
        var result = await _patientService.GetByUser(userId, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Empty(result.Value);

        _mockPatientRepository.Verify(x => x.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetByUser_WhenRepositoryThrowsException_ReturnsFailure()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var exception = new Exception("Database error");

        _mockPatientRepository
            .Setup(x => x.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(exception);

        // Act
        var result = await _patientService.GetByUser(userId, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);

        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) =>
                    v.ToString().Contains($"Ошибка при получении пациентов для пользователя с id {userId}")),
                exception,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task Update_WhenPatientExists_UpdatesPatient()
    {
        // Arrange
        var patientId = Guid.NewGuid();
        var basePatientProfileDto = new BasePatientProfileDto(
            patientId,
            LpuId: "1",
            LpuShortName: "Updated Hospital",
            PatientLastName: "UpdatedLastName",
            PatientFirstName: "UpdatedFirstName",
            PatientMiddleName: "UpdatedMiddleName",
            PatientBirthdate: new DateTime(1995, 5, 5),
            RecipientEmail: "updated@test.com",
            MobilePhoneNumber: "+79998887766"
        );

        var existingPatient = new PatientProfile
        {
            Id = patientId,
            LpuShortName = "Old Hospital",
            PatientLastName = "OldLastName",
            PatientFirstName = "OldFirstName",
            PatientMiddleName = "OldMiddleName",
            PatientBirthdate = new DateTime(1990, 1, 1),
            RecipientEmail = "old@test.com",
            MobilePhoneNumber = "+79991111111"
        };

        _mockPatientRepository
            .Setup(x => x.GetByIdAsync(patientId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingPatient);

        _mockPatientRepository
            .Setup(x => x.UpdateAsync(existingPatient, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _patientService.Update(basePatientProfileDto, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);

        // Проверяем что данные обновились
        Assert.Equal(basePatientProfileDto.LpuShortName, existingPatient.LpuShortName);
        Assert.Equal(basePatientProfileDto.PatientLastName, existingPatient.PatientLastName);
        Assert.Equal(basePatientProfileDto.PatientFirstName, existingPatient.PatientFirstName);
        Assert.Equal(basePatientProfileDto.PatientMiddleName, existingPatient.PatientMiddleName);
        Assert.Equal(basePatientProfileDto.PatientBirthdate, existingPatient.PatientBirthdate);
        Assert.Equal(basePatientProfileDto.RecipientEmail, existingPatient.RecipientEmail);
        Assert.Equal(basePatientProfileDto.MobilePhoneNumber, existingPatient.MobilePhoneNumber);

        _mockPatientRepository.Verify(x => x.GetByIdAsync(patientId, It.IsAny<CancellationToken>()), Times.Once);
        _mockPatientRepository.Verify(x => x.UpdateAsync(existingPatient, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Update_WhenPatientNotFound_ReturnsNotFound()
    {
        // Arrange
        var patientId = Guid.NewGuid();
        var basePatientProfileDto = new BasePatientProfileDto(
            patientId,
            LpuId: "1",
            LpuShortName: "Updated Hospital",
            PatientLastName: "UpdatedLastName",
            PatientFirstName: "UpdatedFirstName",
            PatientMiddleName: "UpdatedMiddleName",
            PatientBirthdate: new DateTime(1995, 5, 5),
            RecipientEmail: "updated@test.com",
            MobilePhoneNumber: "+79998887766"
        );

        _mockPatientRepository
            .Setup(x => x.GetByIdAsync(patientId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((PatientProfile)null);

        // Act
        var result = await _patientService.Update(basePatientProfileDto, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("Patient.Not.Found", result.Error.Code);
        Assert.Equal("Patient not found", result.Error.Description);

        _mockPatientRepository.Verify(x => x.UpdateAsync(It.IsAny<PatientProfile>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Update_WhenRepositoryThrowsException_ReturnsFailure()
    {
        // Arrange
        var patientId = Guid.NewGuid();
        var basePatientProfileDto = new BasePatientProfileDto(
            patientId,
            LpuId: "1",
            LpuShortName: "Updated Hospital",
            PatientLastName: "UpdatedLastName",
            PatientFirstName: "UpdatedFirstName",
            PatientMiddleName: "UpdatedMiddleName",
            PatientBirthdate: new DateTime(1995, 5, 5),
            RecipientEmail: "updated@test.com",
            MobilePhoneNumber: "+79998887766"
        );

        var existingPatient = new PatientProfile { Id = patientId };
        var exception = new Exception("Database error");

        _mockPatientRepository
            .Setup(x => x.GetByIdAsync(patientId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingPatient);

        _mockPatientRepository
            .Setup(x => x.UpdateAsync(existingPatient, It.IsAny<CancellationToken>()))
            .ThrowsAsync(exception);

        // Act
        var result = await _patientService.Update(basePatientProfileDto, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);

        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) =>
                    v.ToString().Contains($"Ошибка при обновлении данных пациента с id {patientId}")),
                exception,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }
}
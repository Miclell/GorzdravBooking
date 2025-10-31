using Application.DTOs.TimePreferences;
using Application.Services.Implementation;
using Core.Entities;
using Core.Interfaces.Repositories;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Application.Tests.Services;

public class TimePreferencesServiceTests
{
    private readonly Mock<ITimePreferencesRepository> _mockRepository;
    private readonly Mock<ILogger<TimePreferencesService>> _mockLogger;
    private readonly TimePreferencesService _service;

    public TimePreferencesServiceTests()
    {
        _mockRepository = new Mock<ITimePreferencesRepository>();
        _mockLogger = new Mock<ILogger<TimePreferencesService>>();
        _service = new TimePreferencesService(_mockRepository.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task CreateRangeAsync_WithValidData_CreatesTimePreferences()
    {
        // Arrange
        var dtos = new List<CreateTimePreferenceDto>
        {
            new("Preset1", Guid.NewGuid(), DayOfWeek.Monday, TimeOnly.FromTimeSpan(TimeSpan.FromHours(9)), TimeOnly.FromTimeSpan(TimeSpan.FromHours(12)), false),
            new("Preset1", Guid.NewGuid(), DayOfWeek.Tuesday, TimeOnly.FromTimeSpan(TimeSpan.FromHours(14)), TimeOnly.FromTimeSpan(TimeSpan.FromHours(16)), false)
        };

        var createdIds = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() };
        var capturedEntities = new List<TimePreferences>();

        _mockRepository
            .Setup(x => x.AddRangeAsync(It.IsAny<IEnumerable<TimePreferences>>(), It.IsAny<CancellationToken>())) // ← IEnumerable вместо List
            .Callback<IEnumerable<TimePreferences>, CancellationToken>((entities, _) => // ← IEnumerable вместо List
            {
                var entityList = entities.ToList();
                capturedEntities.AddRange(entityList);
                for (int i = 0; i < entityList.Count; i++)
                    entityList[i].Id = createdIds[i];
            })
            .Returns(Task.CompletedTask);

        // Act
        var result = await _service.CreateRangeAsync(dtos, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(createdIds, result.Value);
        Assert.Equal(2, capturedEntities.Count);
        Assert.Equal(dtos[0].Name, capturedEntities[0].Name);
        Assert.Equal(dtos[0].Day, capturedEntities[0].Day);
    
        _mockRepository.Verify(x => x.AddRangeAsync(It.IsAny<IEnumerable<TimePreferences>>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateRangeAsync_WhenRepositoryThrowsException_ReturnsFailure()
    {
        // Arrange
        var dtos = new List<CreateTimePreferenceDto>
        {
            new("Preset1", Guid.NewGuid(), DayOfWeek.Monday, TimeOnly.FromTimeSpan(TimeSpan.FromHours(9)), TimeOnly.FromTimeSpan(TimeSpan.FromHours(12)), false)
        };

        var exception = new Exception("Database error");

        _mockRepository
            .Setup(x => x.AddRangeAsync(It.IsAny<List<TimePreferences>>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(exception);

        // Act
        var result = await _service.CreateRangeAsync(dtos, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Ошибка при создании временных предпочтений")),
                exception,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task DeleteByPresetAsync_WithValidData_DeletesPreferences()
    {
        // Arrange
        var dto = new DeleteTimePreferencesDto(Guid.NewGuid(), "TestPreset");

        _mockRepository
            .Setup(x => x.DeleteByPresetAsync(dto.UserId, dto.Name, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _service.DeleteByPresetAsync(dto, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        
        _mockRepository.Verify(x => x.DeleteByPresetAsync(dto.UserId, dto.Name, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteByPresetAsync_WhenRepositoryThrowsException_ReturnsFailure()
    {
        // Arrange
        var dto = new DeleteTimePreferencesDto(Guid.NewGuid(), "TestPreset");
        var exception = new Exception("Database error");

        _mockRepository
            .Setup(x => x.DeleteByPresetAsync(dto.UserId, dto.Name, It.IsAny<CancellationToken>()))
            .ThrowsAsync(exception);

        // Act
        var result = await _service.DeleteByPresetAsync(dto, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains($"Ошибка при удалении пресета {dto.Name} для пользователя {dto.UserId}")),
                exception,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task GetByUserAsync_WhenPreferencesExist_ReturnsGroupedPresets()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var UserId = Guid.NewGuid();
    
        var entities = new List<TimePreferences>
        {
            new() { Name = "WorkDays", UserId = UserId, AnyTime = false, Day = DayOfWeek.Monday, PreferredTimeFrom = TimeOnly.FromTimeSpan(TimeSpan.FromHours(9)), PreferredTimeTo = TimeOnly.FromTimeSpan(TimeSpan.FromHours(12)) },
            new() { Name = "WorkDays", UserId = UserId, AnyTime = false, Day = DayOfWeek.Tuesday, PreferredTimeFrom = TimeOnly.FromTimeSpan(TimeSpan.FromHours(9)), PreferredTimeTo = TimeOnly.FromTimeSpan(TimeSpan.FromHours(12)) },
            new() { Name = "Weekend", UserId = UserId, AnyTime = true }
        };

        _mockRepository
            .Setup(x => x.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(entities);

        // Act
        var result = await _service.GetByUserAsync(userId, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        var presets = result.Value.ToList();
    
        Assert.Equal(2, presets.Count); // WorkDays и Weekend
    
        var workDaysPreset = presets.First(p => p.Name == "WorkDays");
        Assert.Equal(2, workDaysPreset.Preferences.Count);
        Assert.False(workDaysPreset.AnyTime);
    
        var weekendPreset = presets.First(p => p.Name == "Weekend");
        
        Assert.True(weekendPreset.AnyTime);
        Assert.NotEmpty(weekendPreset.Preferences);
    
        _mockRepository.Verify(x => x.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetByUserAsync_WhenNoPreferences_ReturnsEmptyList()
    {
        // Arrange
        var userId = Guid.NewGuid();

        _mockRepository
            .Setup(x => x.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<TimePreferences>());

        // Act
        var result = await _service.GetByUserAsync(userId, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Empty(result.Value);
        
        _mockRepository.Verify(x => x.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetByUserAsync_WhenRepositoryThrowsException_ReturnsFailure()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var exception = new Exception("Database error");

        _mockRepository
            .Setup(x => x.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(exception);

        // Act
        var result = await _service.GetByUserAsync(userId, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains($"Ошибка при получении временных предпочтений для пользователя {userId}")),
                exception,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task GetByPresetAsync_WhenPreferencesExist_ReturnsPresetDto()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var presetName = "WorkDays";
        
        var preferences = new List<TimePreferences>
        {
            new() { Name = presetName, UserId = userId, AnyTime = false, Day = DayOfWeek.Monday, PreferredTimeFrom = TimeOnly.FromTimeSpan(TimeSpan.FromHours(9)), PreferredTimeTo = TimeOnly.FromTimeSpan(TimeSpan.FromHours(12)) },
            new() { Name = presetName, UserId = userId, AnyTime = false, Day = DayOfWeek.Tuesday, PreferredTimeFrom = TimeOnly.FromTimeSpan(TimeSpan.FromHours(14)), PreferredTimeTo = TimeOnly.FromTimeSpan(TimeSpan.FromHours(16)) }
        };

        _mockRepository
            .Setup(x => x.GetByPresetAsync(userId, presetName, It.IsAny<CancellationToken>()))
            .ReturnsAsync(preferences);

        // Act
        var result = await _service.GetByPresetAsync(userId, presetName, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        var preset = result.Value;
        
        Assert.Equal(presetName, preset.Name);
        Assert.Equal(userId, preset.UserId);
        Assert.False(preset.AnyTime);
        Assert.Equal(2, preset.Preferences.Count);
        
        _mockRepository.Verify(x => x.GetByPresetAsync(userId, presetName, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetByPresetAsync_WhenAnyTimePreferenceExists_ReturnsEmptyPreferences()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var presetName = "AnyTimePreset";
        
        var preferences = new List<TimePreferences>
        {
            new() { Name = presetName, UserId = userId, AnyTime = true }
        };

        _mockRepository
            .Setup(x => x.GetByPresetAsync(userId, presetName, It.IsAny<CancellationToken>()))
            .ReturnsAsync(preferences);

        // Act
        var result = await _service.GetByPresetAsync(userId, presetName, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        var preset = result.Value;
        
        Assert.Equal(presetName, preset.Name);
        Assert.True(preset.AnyTime);
        Assert.Empty(preset.Preferences); // Preferences должны быть пустыми для AnyTime
        
        _mockRepository.Verify(x => x.GetByPresetAsync(userId, presetName, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetByPresetAsync_WhenPreferencesNotFound_ReturnsNotFound()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var presetName = "NonExistentPreset";

        _mockRepository
            .Setup(x => x.GetByPresetAsync(userId, presetName, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<TimePreferences>());

        // Act
        var result = await _service.GetByPresetAsync(userId, presetName, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("Preferences.Not.Found", result.Error.Code);
        Assert.Equal("Preferences not found", result.Error.Description);
        
        _mockRepository.Verify(x => x.GetByPresetAsync(userId, presetName, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetByPresetAsync_WhenRepositoryThrowsException_ReturnsFailure()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var presetName = "TestPreset";
        var exception = new Exception("Database error");

        _mockRepository
            .Setup(x => x.GetByPresetAsync(userId, presetName, It.IsAny<CancellationToken>()))
            .ThrowsAsync(exception);

        // Act
        var result = await _service.GetByPresetAsync(userId, presetName, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains($"Ошибка при получении пресета - {presetName} для пациента {userId}")),
                exception,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }
}
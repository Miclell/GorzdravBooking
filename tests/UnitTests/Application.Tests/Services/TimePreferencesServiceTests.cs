using Application.DTOs.TimePreferences;
using Application.Services.Implementation;
using Core.Entities;
using Core.Enums;
using Core.Interfaces.Repositories;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Application.Tests.Services;

public class TimePreferencesServiceTests
{
    private readonly Mock<ILogger<TimePreferencesService>> _loggerMock;
    private readonly Mock<ITimePreferencesRepository> _repositoryMock;
    private readonly TimePreferencesService _sut;

    public TimePreferencesServiceTests()
    {
        _repositoryMock = new Mock<ITimePreferencesRepository>();
        _loggerMock = new Mock<ILogger<TimePreferencesService>>();
        _sut = new TimePreferencesService(_repositoryMock.Object, _loggerMock.Object);
    }

    #region CreateRangeAsync Tests

    [Fact]
    public async Task CreateRangeAsync_ValidDtos_ReturnsSuccessWithIds()
    {
        // Arrange
        var dtos = CreateValidDtos();
        var expectedIds = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() };

        _repositoryMock
            .Setup(x => x.AddRangeAsync(It.IsAny<IEnumerable<TimePreference>>(), default))
            .Callback<IEnumerable<TimePreference>, CancellationToken>((entities, _) =>
            {
                var list = entities.ToList();
                for (var i = 0; i < list.Count; i++)
                    list[i].Id = expectedIds[i];
            });

        // Act
        var result = await _sut.CreateRangeAsync(dtos);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(expectedIds, result.Value);
        _repositoryMock.Verify(x => x.AddRangeAsync(It.IsAny<IEnumerable<TimePreference>>(), default), Times.Once);
    }

    [Fact]
    public async Task CreateRangeAsync_RepositoryThrows_ReturnsFailure()
    {
        // Arrange
        var dtos = CreateValidDtos();
        _repositoryMock
            .Setup(x => x.AddRangeAsync(It.IsAny<IEnumerable<TimePreference>>(), default))
            .ThrowsAsync(new Exception("DB Error"));

        // Act
        var result = await _sut.CreateRangeAsync(dtos);

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

    #endregion

    #region DeleteByPresetAsync Tests

    [Fact]
    public async Task DeleteByPresetAsync_ValidDto_ReturnsSuccess()
    {
        // Arrange
        var dto = new DeleteTimePreferencesDto(Guid.NewGuid(), "TestPreset");

        // Act
        var result = await _sut.DeleteByPresetAsync(dto);

        // Assert
        Assert.True(result.IsSuccess);
        _repositoryMock.Verify(x => x.DeleteByPresetAsync(dto.UserId, dto.Name, default), Times.Once);
    }

    [Fact]
    public async Task DeleteByPresetAsync_RepositoryThrows_ReturnsFailure()
    {
        // Arrange
        var dto = new DeleteTimePreferencesDto(Guid.NewGuid(), "TestPreset");
        _repositoryMock
            .Setup(x => x.DeleteByPresetAsync(dto.UserId, dto.Name, default))
            .ThrowsAsync(new Exception("DB Error"));

        // Act
        var result = await _sut.DeleteByPresetAsync(dto);

        // Assert
        Assert.True(result.IsFailure);
    }

    #endregion

    #region GetByUserAsync Tests

    [Fact]
    public async Task GetByUserAsync_PreferencesExist_ReturnsGroupedPresets()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var entities = CreateTestPreferences(userId);

        _repositoryMock
            .Setup(x => x.GetByUserIdAsync(userId, default))
            .ReturnsAsync(entities);

        // Act
        var result = await _sut.GetByUserAsync(userId);

        // Assert
        Assert.True(result.IsSuccess);
        var presets = result.Value.ToList();
        Assert.Equal(2, presets.Count);

        var workDays = presets.First(p => p.Name == "WorkDays");
        Assert.Equal(2, workDays.Preferences.Count);
    }

    [Fact]
    public async Task GetByUserAsync_NoPreferences_ReturnsEmptyList()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _repositoryMock
            .Setup(x => x.GetByUserIdAsync(userId, default))
            .ReturnsAsync(new List<TimePreference>());

        // Act
        var result = await _sut.GetByUserAsync(userId);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Empty(result.Value);
    }

    [Fact]
    public async Task GetByUserAsync_RepositoryThrows_ReturnsFailure()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _repositoryMock
            .Setup(x => x.GetByUserIdAsync(userId, default))
            .ThrowsAsync(new Exception("DB Error"));

        // Act
        var result = await _sut.GetByUserAsync(userId);

        // Assert
        Assert.True(result.IsFailure);
    }

    #endregion

    #region GetByPresetAsync Tests

    [Fact]
    public async Task GetByPresetAsync_PreferencesExist_ReturnsPreset()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var presetName = "WorkDays";
        var preferences = new List<TimePreference>
        {
            CreateWeekDayPreference(userId, presetName, DayOfWeek.Monday),
            CreateWeekDayPreference(userId, presetName, DayOfWeek.Tuesday)
        };

        _repositoryMock
            .Setup(x => x.GetByPresetAsync(userId, presetName, default))
            .ReturnsAsync(preferences);

        // Act
        var result = await _sut.GetByPresetAsync(userId, presetName);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(presetName, result.Value.Name);
        Assert.Equal(2, result.Value.Preferences.Count);
    }

    [Fact]
    public async Task GetByPresetAsync_NotFound_ReturnsNotFoundError()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var presetName = "NonExistent";

        _repositoryMock
            .Setup(x => x.GetByPresetAsync(userId, presetName, default))
            .ReturnsAsync(new List<TimePreference>());

        // Act
        var result = await _sut.GetByPresetAsync(userId, presetName);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("Preferences.Not.Found", result.Error.Code);
    }

    [Fact]
    public async Task GetByPresetAsync_RepositoryThrows_ReturnsFailure()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var presetName = "TestPreset";
        _repositoryMock
            .Setup(x => x.GetByPresetAsync(userId, presetName, default))
            .ThrowsAsync(new Exception("DB Error"));

        // Act
        var result = await _sut.GetByPresetAsync(userId, presetName);

        // Assert
        Assert.True(result.IsFailure);
    }

    #endregion

    #region Test Data Helpers

    private static List<CreateTimePreferenceDto> CreateValidDtos()
    {
        var userId = Guid.NewGuid();
        return new List<CreateTimePreferenceDto>
        {
            new("WorkDays", userId, TimeSelectionMode.WeekdayPattern, null, DayOfWeek.Monday,
                new TimeOnly(9, 0), new TimeOnly(12, 0), new List<DateOnly>(), 14, 2),
            new("WorkDays", userId, TimeSelectionMode.WeekdayPattern, null, DayOfWeek.Tuesday,
                new TimeOnly(14, 0), new TimeOnly(16, 0), new List<DateOnly>(), 14, 2)
        };
    }

    private static List<TimePreference> CreateTestPreferences(Guid userId)
    {
        return new List<TimePreference>
        {
            CreateWeekDayPreference(userId, "WorkDays", DayOfWeek.Monday),
            CreateWeekDayPreference(userId, "WorkDays", DayOfWeek.Tuesday),
            CreateAnyTimePreference(userId, "Weekend")
        };
    }

    private static WeekDayPreference CreateWeekDayPreference(Guid userId, string name, DayOfWeek day)
    {
        return new WeekDayPreference
        {
            Name = name,
            UserId = userId,
            TimeMode = TimeSelectionMode.WeekdayPattern,
            Day = day,
            PreferredTimeFrom = new TimeOnly(9, 0),
            PreferredTimeTo = new TimeOnly(12, 0),
            ExcludedDates = new List<DateOnly>(),
            MaxDaysAhead = 14,
            MinHoursFromNow = 2
        };
    }

    private static AnyTimePreference CreateAnyTimePreference(Guid userId, string name)
    {
        return new AnyTimePreference
        {
            Name = name,
            UserId = userId,
            TimeMode = TimeSelectionMode.AnyTime,
            ExcludedDates = new List<DateOnly>(),
            MaxDaysAhead = 30,
            MinHoursFromNow = 1
        };
    }

    #endregion
}
using Application.DTOs.User;
using Application.Services.Implementation;
using Core.Entities;
using Core.Interfaces.Repositories;
using Core.Interfaces.Security;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Application.Tests.Services;

public class UserServiceTests
{
    private readonly Mock<ILogger<UserService>> _mockLogger;
    private readonly Mock<IPasswordHasher> _mockPasswordHasher;
    private readonly Mock<IUserRepository> _mockUserRepository;
    private readonly UserService _userService;

    public UserServiceTests()
    {
        _mockUserRepository = new Mock<IUserRepository>();
        _mockPasswordHasher = new Mock<IPasswordHasher>();
        _mockLogger = new Mock<ILogger<UserService>>();
        _userService = new UserService(_mockUserRepository.Object, _mockPasswordHasher.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task Create_WhenUserDoesNotExist_CreatesUser()
    {
        // Arrange
        var baseUserDto = new BaseUserDto("testuser", "password123");
        var expectedUserId = Guid.NewGuid();
        const string passwordHash = "hashed_password";

        _mockUserRepository
            .Setup(x => x.GetByUsernameAsync(baseUserDto.Username, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User)null!);

        _mockPasswordHasher
            .Setup(x => x.HashPassword(baseUserDto.Password))
            .Returns(passwordHash);

        _mockUserRepository
            .Setup(x => x.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .Callback<User, CancellationToken>((user, _) => user.Id = expectedUserId);

        // Act
        var result = await _userService.Create(baseUserDto, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(expectedUserId, result.Value);

        _mockUserRepository.Verify(x => x.GetByUsernameAsync(baseUserDto.Username, It.IsAny<CancellationToken>()),
            Times.Once);
        _mockPasswordHasher.Verify(x => x.HashPassword(baseUserDto.Password), Times.Once);
        _mockUserRepository.Verify(x => x.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Create_WhenUserExists_ReturnsConflict()
    {
        // Arrange
        var baseUserDto = new BaseUserDto("existinguser", "password123");
        var existingUser = new User { Id = Guid.NewGuid(), Username = "existinguser" };

        _mockUserRepository
            .Setup(x => x.GetByUsernameAsync(baseUserDto.Username, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingUser);

        // Act
        var result = await _userService.Create(baseUserDto, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("User.Username.Exists", result.Error.Code);
        Assert.Equal("Пользователь с таким именем уже существует", result.Error.Description);

        _mockUserRepository.Verify(x => x.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Create_WhenRepositoryThrowsException_ReturnsFailure()
    {
        // Arrange
        var baseUserDto = new BaseUserDto("testuser", "password123");
        var exception = new Exception("Database error");

        _mockUserRepository
            .Setup(x => x.GetByUsernameAsync(baseUserDto.Username, It.IsAny<CancellationToken>()))
            .ThrowsAsync(exception);

        // Act
        var result = await _userService.Create(baseUserDto, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("Failure", result.Error.Type.ToString());

        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Ошибка при добавление User")),
                It.IsAny<Exception?>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()!),
            Times.Once);
    }

    [Fact]
    public async Task Delete_WhenUserExists_DeletesUser()
    {
        // Arrange
        var userId = Guid.NewGuid();

        _mockUserRepository
            .Setup(x => x.DeleteAsync(userId, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _userService.Delete(userId, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);

        _mockUserRepository.Verify(x => x.DeleteAsync(userId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Delete_WhenRepositoryThrowsException_ReturnsFailure()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var exception = new Exception("Database error");

        _mockUserRepository
            .Setup(x => x.DeleteAsync(userId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(exception);

        // Act
        var result = await _userService.Delete(userId, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);

        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains($"Database error deleting user {userId}")),
                exception,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()!),
            Times.Once);
    }

    [Fact]
    public async Task UpdatePassword_WhenUserExists_UpdatesPassword()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var newPassword = "newPassword123";
        var passwordHash = "new_hashed_password";
        var existingUser = new User
        {
            Id = userId,
            Username = "testuser",
            PasswordHash = "old_hash",
            UpdatedAt = DateTime.UtcNow.AddDays(-1) // старое значение
        };

        var oldUpdatedAt = existingUser.UpdatedAt;

        _mockUserRepository
            .Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingUser);

        _mockPasswordHasher
            .Setup(x => x.HashPassword(newPassword))
            .Returns(passwordHash);

        // Act
        var result = await _userService.UpdatePassword(userId, newPassword, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(passwordHash, existingUser.PasswordHash);

        Assert.True(existingUser.UpdatedAt > oldUpdatedAt);
        Assert.True(existingUser.UpdatedAt <= DateTime.UtcNow);

        _mockUserRepository.Verify(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()), Times.Once);
        _mockPasswordHasher.Verify(x => x.HashPassword(newPassword), Times.Once);
        _mockUserRepository.Verify(x => x.UpdateAsync(existingUser, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdatePassword_WhenUserNotFound_ReturnsConflict()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var newPassword = "newPassword123";

        _mockUserRepository
            .Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User)null!);

        // Act
        var result = await _userService.UpdatePassword(userId, newPassword, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("User.Not.Found", result.Error.Code);
        Assert.Equal($"Пользователь с {userId} не найден", result.Error.Description);

        _mockPasswordHasher.Verify(x => x.HashPassword(It.IsAny<string>()), Times.Never);
        _mockUserRepository.Verify(x => x.UpdateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task UpdatePassword_WhenRepositoryThrowsException_ReturnsFailure()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var newPassword = "newPassword123";
        var existingUser = new User { Id = userId, Username = "testuser" };
        var exception = new Exception("Database error");

        _mockUserRepository
            .Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingUser);

        _mockUserRepository
            .Setup(x => x.UpdateAsync(existingUser, It.IsAny<CancellationToken>()))
            .ThrowsAsync(exception);

        // Act
        var result = await _userService.UpdatePassword(userId, newPassword, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);

        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains($"Database error update user {userId} password")),
                exception,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()!),
            Times.Once);
    }
}
using Application.Abstract;
using Application.Common.Results;
using Application.DTOs.User;
using Application.Services.Interfaces;
using Core.Entities;
using Core.Interfaces.Repositories;
using Core.Interfaces.Security;
using Microsoft.Extensions.Logging;

namespace Application.Services.Implementation;

public class UserService(
    IUserRepository userRepository,
    IPasswordHasher passwordHasher,
    ILogger<UserService> logger) : IAppService, IUserService
{
    public async Task<Result<Guid>> Create(BaseUserDto baseUserDto, CancellationToken cancellationToken = default)
    {
        try
        {
            var existing = await userRepository.GetByUsernameAsync(baseUserDto.Username, cancellationToken);

            if (existing != null)
                return Error.Conflict("User.Username.Exists", "Пользователь с таким именем уже существует");

            var user = new User
            {
                Username = baseUserDto.Username.Trim(),
                PasswordHash = passwordHasher.HashPassword(baseUserDto.Password)
            };

            await userRepository.AddAsync(user, cancellationToken);
            return user.Id;
        }
        catch (Exception e)
        {
            logger.LogError(e, "Ошибка при добавление User с {Username} - {e}", baseUserDto.Username, e);
            return Error.Failure($"{e}", "Ошибка");
        }
    }

    public async Task<Result> Delete(Guid userId, CancellationToken cancellationToken = default)
    {
        try
        {
            await userRepository.DeleteAsync(userId, cancellationToken);
            return Result.Success();
        }
        catch (Exception e)
        {
            logger.LogError(e, "Database error deleting user {UserId}", userId);
            return Error.Failure(e.ToString(), "Ошибка");
        }
    }

    public async Task<Result> UpdatePassword(Guid userId, string newPassword,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var user = await userRepository.GetByIdAsync(userId, cancellationToken);
            if (user == null)
                return Error.Conflict("User.Not.Found", $"Пользователь с {userId} не найден");

            user.PasswordHash = passwordHasher.HashPassword(newPassword);
            user.UpdatedAt = DateTime.UtcNow;

            await userRepository.UpdateAsync(user, cancellationToken);
            return Result.Success();
        }
        catch (Exception e)
        {
            logger.LogError(e, "Database error update user {UserId} password", userId);
            return Error.Failure(e.ToString(), "Ошибка");
        }
    }
}
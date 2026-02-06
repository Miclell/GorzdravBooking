using Application.Common.Results;
using Application.DTOs.User;

namespace Application.Services.Interfaces;

public interface IUserService
{
    Task<Result<Guid>> Create(BaseUserDto baseUserDto, CancellationToken cancellationToken = default);
    Task<Result<Guid>> Validate(BaseUserDto baseUserDto, CancellationToken cancellationToken = default);
    Task<Result> Delete(Guid userId, CancellationToken cancellationToken = default);
    Task<Result> UpdatePassword(Guid userId, string newPassword, CancellationToken cancellationToken = default);
}
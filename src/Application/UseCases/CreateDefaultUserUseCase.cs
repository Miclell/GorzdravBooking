using Application.Abstract;
using Application.Common.Results;
using Application.DTOs.User;
using Application.Services.Interfaces;

namespace Application.UseCases;

public class CreateDefaultUserUseCase(IUserService userService) : IAppUseCase
{
    public async Task<Result<Guid>> ExecuteAsync()
    {
        var dto = new BaseUserDto("admin", "admin");

        return await userService.Create(dto);
    }
}
using Application.Common.Results;
using Application.DTOs.User;
using Application.Services;

namespace Application.UseCases;

public class CreateDefaultUserUseCase(UserService userService)
{
    public async Task<Result<Guid>> ExecuteAsync()
    {
        var dto = new BaseUserDto("admin", "admin");
        
        return await userService.Create(dto);
    }
}
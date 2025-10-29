using Application.Abstract;
using Application.Services.Interfaces;
using Application.UseCases;
using Core.Entities;
using Core.Interfaces.Repositories;

namespace Application.Services.Implementation;

public class AppSettingsService(IAppSettingRepository appSettingRepository, CreateDefaultUserUseCase createDefaultUserUseCase)
    : IAppSettingsService, IAppService
{
    public async Task AppInitializeAsync()
    {
        var isFirstInit = await appSettingRepository.GetAsync("IsInit") == null;
        if (isFirstInit)
        {
            await CreateDefaultUser();
            await appSettingRepository.AddAsync(new AppSetting()
            {
                Key = "IsInit",
                Value = "true"
            });
        }
    }

    public async Task<Guid> GetDefaultUserIdAsync()
    {
        var appSetting = await appSettingRepository.GetAsync("DefaultUserId");

        if (appSetting != null) return Guid.Parse(appSetting.Value);
        
        await CreateDefaultUser();

        return await GetDefaultUserIdAsync();
    }

    private async Task CreateDefaultUser()
    {
        var userId = await createDefaultUserUseCase.ExecuteAsync();
        var appSetting = new AppSetting()
        {
            Key = "DefaultUserId",
            Value = userId.ToString()
        };
        await appSettingRepository.AddAsync(appSetting);
    }
}
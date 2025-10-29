namespace Application.Services.Interfaces;

public interface IAppSettingsService
{
    Task AppInitializeAsync();
    Task<Guid> GetDefaultUserIdAsync();
}
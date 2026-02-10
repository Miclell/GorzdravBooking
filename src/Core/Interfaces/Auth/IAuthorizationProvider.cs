namespace Core.Interfaces.Auth;

public interface IAuthorizationProvider
{
    Task<bool> CanAccessAsync<T>(Guid userId, Guid resourceId)
        where T : class;
}
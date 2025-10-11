using Core.Models;

namespace Core.Interfaces.ApiClient;

public interface IApiService
{
    Task<ApiResponse<TResponse>> GetAsync<TResponse>(string additionalUri);
    Task<ApiResponse<TResponse>> PostAsync<TRequest, TResponse>(string additionalUri, TRequest data);
}
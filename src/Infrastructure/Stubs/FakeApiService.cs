using Core.Interfaces.ApiClient;
using Core.Models;

namespace Infrastructure.Stubs;

public class FakeApiService : IApiService
{
    private readonly Dictionary<string, object> _getResponses = new();
    private readonly Dictionary<string, object> _postResponses = new();
    
    private readonly FakeApiDataService _dataService = new();

    public FakeApiService() { }

    public FakeApiService(FakeApiDataService dataService)
    {
        _dataService = dataService;
        SetupDemoResponses();
    }

    public Task<ApiResponse<TResponse>> GetAsync<TResponse>(string additionalUri)
    {
        if (_getResponses.TryGetValue(additionalUri, out var response))
            return Task.FromResult((ApiResponse<TResponse>)response);

        return Task.FromResult(new ApiResponse<TResponse>
        {
            Success = false,
            Message = $"URL '{additionalUri}' not configured in fake service"
        });
    }

    public Task<ApiResponse<TResponse>> PostAsync<TRequest, TResponse>(string additionalUri, TRequest data)
    {
        if (_postResponses.TryGetValue(additionalUri, out var response))
            return Task.FromResult((ApiResponse<TResponse>)response);

        return Task.FromResult(new ApiResponse<TResponse>
        {
            Success = false,
            Message = $"URL '{additionalUri}' not configured in fake service"
        });
    }

    public void SetupGetResponse<TResponse>(string uri, ApiResponse<TResponse> response)
    {
        _getResponses[uri] = response;
    }

    public void SetupPostResponse<TRequest, TResponse>(string uri, ApiResponse<TResponse> response)
    {
        _postResponses[uri] = response;
    }
    
    private void SetupDemoResponses()
    {
        // var моделька = _dataService.GetDemoМоделька
    }
}
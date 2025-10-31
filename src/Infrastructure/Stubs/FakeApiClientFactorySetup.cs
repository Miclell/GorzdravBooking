using Core.Interfaces.ApiClient;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.Stubs;

public static class FakeApiClientFactorySetup
{
    public static void AddFakeGorzdravClient(this IServiceCollection services)
    {
        services.AddSingleton<FakeApiDataService>();
        services.AddScoped<IApiService, FakeApiService>();
    }
}
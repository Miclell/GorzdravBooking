using Core.Models;
using Infrastructure.ApiClient.Models;
using Infrastructure.Services;
using Infrastructure.Tests.Fakes;
using Xunit;

namespace Infrastructure.Tests.ServicesTests;

public class DistrictServiceTests
{
    [Fact]
    public async Task GetDistricts_Success()
    {
        var fakeApiService = new FakeApiService();
        var districtService = new DistrictService(fakeApiService);

        var expectedAppointments = new List<District>
        {
            new() { Id = "1", Name = "Адмиралтейский", Okato = 40262 },
            new() { Id = "2", Name = "Василеостровский", Okato = 40263 },
            new() { Id = "3", Name = "Выборгский", Okato = 40265 },
            new() { Id = "4", Name = "Калининский", Okato = 40273 },
            new() { Id = "5", Name = "Кировский", Okato = 40276 },
            new() { Id = "6", Name = "Колпинский", Okato = 40277 },
            new() { Id = "7", Name = "Красногвардейский", Okato = 40278 },
            new() { Id = "8", Name = "Красносельский", Okato = 40279 },
            new() { Id = "9", Name = "Кронштадтский", Okato = 40280 },
            new() { Id = "10", Name = "Курортный", Okato = 40281 },
            new() { Id = "11", Name = "Московский", Okato = 40284 },
            new() { Id = "12", Name = "Невский", Okato = 40285 },
            new() { Id = "13", Name = "Петроградский", Okato = 40288 },
            new() { Id = "14", Name = "Петродворцовый", Okato = 40290 },
            new() { Id = "15", Name = "Приморский", Okato = 40270 },
            new() { Id = "16", Name = "Пушкинский", Okato = 40294 },
            new() { Id = "17", Name = "Фрунзенский", Okato = 40296 },
            new() { Id = "18", Name = "Центральный", Okato = 40298 }
        };

        const string uri = GorzdravApiEndpoints.Districts;
        fakeApiService.SetupGetResponse(uri, new ApiResponse<List<District>>
        {
            Success = true,
            Result = expectedAppointments
        });

        var result = await districtService.GetDistrictsAsync();

        Assert.Equal(expectedAppointments, result);
    }
}
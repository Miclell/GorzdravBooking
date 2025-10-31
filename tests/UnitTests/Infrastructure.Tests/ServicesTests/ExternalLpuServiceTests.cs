using Core.Models;
using Infrastructure.ApiClient.Models;
using Infrastructure.Services;
using Infrastructure.Stubs;
using Xunit;

namespace Infrastructure.Tests.ServicesTests;

public class ExternalLpuServiceTests
{
    [Fact]
    public async Task GetLpuByDistrict_Success()
    {
        var fakeApiService = new FakeApiService();
        var lpuService = new ExternalLpuService(fakeApiService);

        var expectedLpus = new List<Lpu>
        {
            new()
            {
                Id = 1,
                Description = "Система А",
                District = 1,
                DistrictId = 1,
                DistrictName = "Адмиралтейский",
                IsActive = true,
                LpuFullName = "Городская поликлиника №27",
                LpuShortName = "ГП №27",
                LpuType = "Поликлиника",
                HeadOrganization = "org-001",
                Organization = "org-001-sub",
                Address = "ул. Примерная, д. 27",
                Phone = "(812) 000-00-01",
                Email = "clinic27@example.com",
                Longitude = "30.307837",
                Latitude = "59.925867",
                CovidVaccination = true,
                InDepthExamination = true
            },
            new()
            {
                Id = 2,
                Description = "Система А",
                District = 1,
                DistrictId = 1,
                DistrictName = "Адмиралтейский",
                IsActive = true,
                LpuFullName = "Городская поликлиника №28",
                LpuShortName = "ГП №28",
                LpuType = "Поликлиника",
                HeadOrganization = "org-002",
                Organization = "org-002-sub",
                Address = "ул. Тестовая, д. 2",
                Phone = "(812) 000-00-02",
                Email = "clinic28@example.com",
                Longitude = "30.3302",
                Latitude = "59.9208",
                CovidVaccination = true,
                InDepthExamination = true
            }
        };

        const string districtId = "1";
        var uri = GorzdravApiEndpoints.LpusByDistrict(districtId);

        fakeApiService.SetupGetResponse(uri, new ApiResponse<List<Lpu>>
        {
            Success = true,
            Result = expectedLpus
        });

        var result = await lpuService.GetByDistrictAsync(districtId);

        Assert.Equal(expectedLpus.Count, result.Count);
        Assert.Equal(expectedLpus[0].LpuFullName, result[0].LpuFullName);
        Assert.Equal(expectedLpus[1].LpuShortName, result[1].LpuShortName);
    }
}
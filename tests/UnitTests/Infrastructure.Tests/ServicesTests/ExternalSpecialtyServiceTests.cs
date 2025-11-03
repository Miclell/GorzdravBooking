using Core.Models;
using Infrastructure.ApiClient.Models;
using Infrastructure.Services;
using Infrastructure.Stubs;
using Xunit;
using FakeApiService = Infrastructure.Tests.Fakes.FakeApiService;

namespace Infrastructure.Tests.ServicesTests;

public class ExternalSpecialtyServiceTests
{
    [Fact]
    public async Task GetSpecialtiesByLpu_Success()
    {
        var fakeApiService = new FakeApiService();
        var specialtyService = new ExternalSpecialtyService(fakeApiService);

        var expectedSpecialties = new List<MedicalSpeciality>
        {
            new()
            {
                Id = "92134140",
                FerId = "8",
                Name = "Акушерство и гинекология",
                CountFreeParticipant = 184,
                CountFreeTicket = 184,
                LastDate = "2025-10-27T15:30:00",
                NearestDate = "2025-10-14T15:45:00"
            },
            new()
            {
                Id = "92134158",
                FerId = "79",
                Name = "Гастроэнтерология",
                CountFreeParticipant = 16,
                CountFreeTicket = 16,
                LastDate = "2025-10-27T14:45:00",
                NearestDate = "2025-10-21T10:00:00"
            }
        };

        const int lpuId = 1;
        // ВОЗМОЖНО НУЖНО ИСПРАВИТЬ URI - проверь реальный запрос
        var uri = GorzdravApiEndpoints.SpecialtiesByLpu(lpuId);

        fakeApiService.SetupGetResponse(uri, new ApiResponse<List<MedicalSpeciality>>
        {
            Success = true,
            Result = expectedSpecialties
        });

        var result = await specialtyService.GetByLpuAsync(lpuId);

        Assert.Equal(expectedSpecialties.Count, result.Count);
        Assert.Equal(expectedSpecialties[0].Name, result[0].Name);
    }
}
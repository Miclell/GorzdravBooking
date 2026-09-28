using Infrastructure.Services;
using Infrastructure.Stubs;
using Xunit;

namespace Infrastructure.Tests.ServicesTests;

public class SpecialtyIdFlowTests
{
    [Fact]
    public async Task FakeApiService_DoctorsEndpointAcceptsSelectedSpecialtyId()
    {
        var api = new FakeApiService(new FakeApiDataService());
        var specialtyService = new ExternalSpecialtyService(api);
        var doctorService = new ExternalDoctorService(api);

        var specialty = (await specialtyService.GetByLpuAsync(187)).First();

        Assert.NotEmpty(await doctorService.GetBySpecialtyAsync(187, specialty.Id));
        await Assert.ThrowsAsync<HttpRequestException>(() =>
            doctorService.GetBySpecialtyAsync(187, specialty.Name));
    }
}

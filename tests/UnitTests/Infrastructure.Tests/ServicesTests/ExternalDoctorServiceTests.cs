using Core.Models;
using Infrastructure.ApiClient;
using Infrastructure.Services;
using Infrastructure.Stubs;
using Xunit;
using FakeApiService = Infrastructure.Tests.Fakes.FakeApiService;

namespace Infrastructure.Tests.ServicesTests;

public class ExternalDoctorServiceTests
{
    [Fact]
    public async Task GetDoctorsBySpecialty_Success()
    {
        var fakeApiService = new FakeApiService();
        var doctorService = new ExternalDoctorService(fakeApiService);

        var expectedDoctors = new List<Doctor>
        {
            new()
            {
                AriaNumber = "A123",
                AriaType = "Основной",
                Comment = "Врач высшей категории",
                FreeParticipantCount = 15,
                FreeTicketCount = 15,
                Id = "doctor_1",
                LastDate = new DateTime(2025, 10, 27, 15, 30, 0),
                Name = "Врач Первый",
                LastName = "Врач",
                FirstName = "Первый",
                MiddleName = "Тестович",
                NearestDate = new DateTime(2025, 10, 14, 9, 0, 0)
            },
            new()
            {
                AriaNumber = "A124",
                AriaType = "Дополнительный",
                Comment = "Кандидат медицинских наук",
                FreeParticipantCount = 8,
                FreeTicketCount = 8,
                Id = "doctor_2",
                LastDate = new DateTime(2025, 10, 25, 14, 0, 0),
                Name = "Врач Второй",
                LastName = "Врач",
                FirstName = "Второй",
                MiddleName = "Тестович",
                NearestDate = new DateTime(2025, 10, 15, 10, 30, 0)
            }
        };

        const int lpuId = 1;
        const string specialtyId = "92134140";
        var uri = GorzdravApiEndpoints.DoctorsBySpecialty(lpuId, specialtyId);

        fakeApiService.SetupGetResponse(uri, new ApiResponse<List<Doctor>>
        {
            Success = true,
            Result = expectedDoctors
        });

        var result = await doctorService.GetBySpecialtyAsync(lpuId, specialtyId);

        Assert.Equal(expectedDoctors.Count, result.Count);
        Assert.Equal(expectedDoctors[0].Name, result[0].Name);
        Assert.Equal(expectedDoctors[1].AriaType, result[1].AriaType);
    }
}
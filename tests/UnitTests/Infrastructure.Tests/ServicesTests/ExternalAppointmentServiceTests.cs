using Core.Models;
using Infrastructure.Services;
using Infrastructure.Stubs;
using Xunit;
using FakeApiService = Infrastructure.Tests.Fakes.FakeApiService;

namespace Infrastructure.Tests.ServicesTests;

public class ExternalAppointmentServiceTests
{
    private readonly FakeApiService _fakeApiService;
    private readonly ExternalAppointmentService _externalAppointmentService;

    public ExternalAppointmentServiceTests()
    {
        _fakeApiService = new FakeApiService();
        _externalAppointmentService = new ExternalAppointmentService(_fakeApiService);
    }

    [Fact]
    public async Task GetByDoctorAsync_Success()
    {
        // Arrange
        var expectedAppointments = new List<Appointment>
        {
            new Appointment 
            { 
                Id = "appointment_1",
                VisitStart = new DateTime(2025, 10, 14, 9, 0, 0),
                VisitEnd = new DateTime(2025, 10, 14, 9, 30, 0),
                Address = "ул. Примерная, д. 27, каб. 101",
                Number = "A001",
                Room = "101"
            },
            new Appointment 
            { 
                Id = "appointment_2",
                VisitStart = new DateTime(2025, 10, 14, 10, 0, 0),
                VisitEnd = new DateTime(2025, 10, 14, 10, 30, 0),
                Address = "ул. Примерная, д. 27, каб. 101",
                Number = "A002",
                Room = "101"
            },
            new Appointment 
            { 
                Id = "appointment_3",
                VisitStart = new DateTime(2025, 10, 14, 11, 0, 0),
                VisitEnd = new DateTime(2025, 10, 14, 11, 30, 0),
                Address = "ул. Примерная, д. 27, каб. 101",
                Number = "A003",
                Room = "101"
            }
        };

        const int lpuId = 1;
        const string doctorId = "doctor_123";
        var uri = $"schedule/lpu/{lpuId}/doctor/{doctorId}/appointments";
        
        _fakeApiService.SetupGetResponse(uri, new ApiResponse<List<Appointment>>
        {
            Success = true,
            Result = expectedAppointments
        });

        // Act
        var result = await _externalAppointmentService.GetByDoctorAsync(lpuId, doctorId);

        // Assert
        Assert.Equal(expectedAppointments.Count, result.Count);
        Assert.Equal(expectedAppointments[0].Number, result[0].Number);
        Assert.Equal(expectedAppointments[1].VisitStart, result[1].VisitStart);
        Assert.Equal(expectedAppointments[2].Room, result[2].Room);
    }

    [Fact]
    public async Task GetByDoctorAsync_Failure()
    {
        // Arrange
        const int lpuId = 1;
        const string doctorId = "doctor_123";
        var uri = $"schedule/lpu/{lpuId}/doctor/{doctorId}/appointments";
        
        _fakeApiService.SetupGetResponse(uri, new ApiResponse<List<Appointment>>
        {
            Success = false,
            Message = "Doctor not found"
        });

        // Act & Assert
        var exception = await Assert.ThrowsAsync<HttpRequestException>(
            () => _externalAppointmentService.GetByDoctorAsync(lpuId, doctorId));
        
        Assert.Contains("Ошибка при получении номерков: Doctor not found", exception.Message);
    }

    [Fact]
    public async Task CreateAppointmentAsync_Success()
    {
        // Arrange
        var createRequest = new AppointmentCreateRequest
        {
            EsiaId = "esia_123",
            LpuId = "1",
            PatientId = "patient_456",
            AppointmentId = "appointment_789",
            ReferralId = "referral_001",
            IpmpiCardId = "card_002",
            RecipientEmail = "patient@example.com",
            PatientLastName = "Иванов",
            PatientFirstName = "Иван",
            PatientMiddleName = "Иванович",
            PatientBirthdate = new DateTime(1980, 5, 15),
            Room = "101",
            Address = "ул. Примерная, д. 27",
            VisitDate = new DateTime(2025, 10, 14, 9, 0, 0)
        };

        const string uri = "appointment/create";
        
        _fakeApiService.SetupPostResponse<AppointmentCreateRequest, bool>(uri, new ApiResponse<bool>
        {
            Success = true,
            Result = true
        });

        // Act
        var result = await _externalAppointmentService.CreateAppointmentAsync(createRequest);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task CreateAppointmentAsync_Failure()
    {
        // Arrange
        var createRequest = new AppointmentCreateRequest
        {
            LpuId = "1",
            PatientId = "patient_456",
            AppointmentId = "appointment_789",
            PatientLastName = "Иванов",
            PatientFirstName = "Иван",
            PatientBirthdate = new DateTime(1980, 5, 15),
            Address = "ул. Примерная, д. 27",
            VisitDate = new DateTime(2025, 10, 14, 9, 0, 0)
        };

        const string uri = "appointment/create";
        
        _fakeApiService.SetupPostResponse<AppointmentCreateRequest, bool>(uri, new ApiResponse<bool>
        {
            Success = false,
            Message = "Appointment time is already taken"
        });

        // Act & Assert
        var exception = await Assert.ThrowsAsync<HttpRequestException>(
            () => _externalAppointmentService.CreateAppointmentAsync(createRequest));
        
        Assert.Contains("Ошибка при выполнении записи: Appointment time is already taken", exception.Message);
    }

    [Fact]
    public async Task CancelAppointmentAsync_Success()
    {
        // Arrange
        var cancelRequest = new AppointmentСancelRequest
        {
            AppointmentId = "appointment_789",
            LpuId = "1",
            PatientId = "patient_456",
            EsiaId = "esia_123",
            AppointmentType = "regular"
        };

        const string uri = "appointment/cancel";
        
        _fakeApiService.SetupPostResponse<AppointmentСancelRequest, bool>(uri, new ApiResponse<bool>
        {
            Success = true,
            Result = true
        });

        // Act
        var result = await _externalAppointmentService.CancelAppointmentAsync(cancelRequest);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task CancelAppointmentAsync_Failure()
    {
        // Arrange
        var cancelRequest = new AppointmentСancelRequest
        {
            AppointmentId = "appointment_789",
            LpuId = "1",
            PatientId = "patient_456",
            EsiaId = "esia_123",
            AppointmentType = "regular"
        };

        const string uri = "appointment/cancel";
        
        _fakeApiService.SetupPostResponse<AppointmentСancelRequest, bool>(uri, new ApiResponse<bool>
        {
            Success = false,
            Message = "Appointment not found"
        });

        // Act & Assert
        var exception = await Assert.ThrowsAsync<HttpRequestException>(
            () => _externalAppointmentService.CancelAppointmentAsync(cancelRequest));
        
        Assert.Contains("Ошибка при отмене записи: Appointment not found", exception.Message);
    }

    [Fact]
    public async Task GetByDoctorAsync_UrlEncoding_Success()
    {
        // Arrange
        var expectedAppointments = new List<Appointment>
        {
            new Appointment 
            { 
                Id = "appointment_1",
                VisitStart = DateTime.Now.AddDays(1),
                VisitEnd = DateTime.Now.AddDays(1).AddMinutes(30),
                Address = "Test Address",
                Number = "A001",
                Room = "101"
            }
        };

        const int lpuId = 1;
        const string doctorId = "doctor/with/special&chars?id=123";
        var uri = $"schedule/lpu/{lpuId}/doctor/{Uri.EscapeDataString(doctorId)}/appointments";
        
        _fakeApiService.SetupGetResponse(uri, new ApiResponse<List<Appointment>>
        {
            Success = true,
            Result = expectedAppointments
        });

        // Act
        var result = await _externalAppointmentService.GetByDoctorAsync(lpuId, doctorId);

        // Assert
        Assert.Single(result);
        Assert.Equal(expectedAppointments[0].Id, result[0].Id);
    }
}
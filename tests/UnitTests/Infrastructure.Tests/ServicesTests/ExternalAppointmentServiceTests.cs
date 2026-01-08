using Core.Interfaces.Services;
using Core.Models;
using Core.Models.Referral;
using Infrastructure.ApiClient;
using Infrastructure.Services;
using Moq;
using Xunit;
using FakeApiService = Infrastructure.Tests.Fakes.FakeApiService;

namespace Infrastructure.Tests.ServicesTests;

public class ExternalAppointmentServiceTests
{
    private readonly ExternalAppointmentService _externalAppointmentService;
    private readonly FakeApiService _fakeApiService;
    private readonly Mock<IExternalDoctorService> _mockDoctorService;

    public ExternalAppointmentServiceTests()
    {
        _fakeApiService = new FakeApiService();
        _mockDoctorService = new Mock<IExternalDoctorService>();
        _externalAppointmentService = new ExternalAppointmentService(_fakeApiService, _mockDoctorService.Object);
    }

    [Fact]
    public async Task GetBySpecialityAsync_Success()
    {
        // Arrange
        const int lpuId = 1;
        const string specialtyId = "specialty_123";

        var doctors = new List<Doctor>
        {
            new() { Id = "doctor_1", Name = "Иванов Иван Иванович" },
            new() { Id = "doctor_2", Name = "Петров Петр Петрович" }
        };

        var appointmentsDoctor1 = new List<Appointment>
        {
            new()
            {
                Id = "appointment_1",
                VisitStart = new DateTime(2025, 10, 14, 9, 0, 0),
                VisitEnd = new DateTime(2025, 10, 14, 9, 30, 0),
                Address = "ул. Примерная, д. 27, каб. 101",
                Number = "A001",
                Room = "101"
            }
        };

        var appointmentsDoctor2 = new List<Appointment>
        {
            new()
            {
                Id = "appointment_2",
                VisitStart = new DateTime(2025, 10, 14, 10, 0, 0),
                VisitEnd = new DateTime(2025, 10, 14, 10, 30, 0),
                Address = "ул. Примерная, д. 27, каб. 102",
                Number = "A002",
                Room = "102"
            }
        };

        _mockDoctorService
            .Setup(x => x.GetBySpecialtyAsync(lpuId, specialtyId))
            .ReturnsAsync(doctors);

        _fakeApiService.SetupGetResponse(
            $"schedule/lpu/{lpuId}/doctor/{doctors[0].Id}/appointments",
            new ApiResponse<List<Appointment>>
            {
                Success = true,
                Result = appointmentsDoctor1
            });

        _fakeApiService.SetupGetResponse(
            $"schedule/lpu/{lpuId}/doctor/{doctors[1].Id}/appointments",
            new ApiResponse<List<Appointment>>
            {
                Success = true,
                Result = appointmentsDoctor2
            });

        // Act
        var result = await _externalAppointmentService.GetBySpecialityAsync(lpuId, specialtyId);

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Equal(doctors[0].Name, result[0].Doctor);
        Assert.Equal(appointmentsDoctor1[0].Id, result[0].Appointment.Id);
        Assert.Equal(doctors[1].Name, result[1].Doctor);
        Assert.Equal(appointmentsDoctor2[0].Id, result[1].Appointment.Id);
    }

    [Fact]
    public async Task GetBySpecialityAsync_DoctorAppointmentsFailed_ThrowsException()
    {
        // Arrange
        const int lpuId = 1;
        const string specialtyId = "specialty_123";

        var doctors = new List<Doctor>
        {
            new() { Id = "doctor_1", Name = "Иванов Иван Иванович" }
        };

        _mockDoctorService
            .Setup(x => x.GetBySpecialtyAsync(lpuId, specialtyId))
            .ReturnsAsync(doctors);

        _fakeApiService.SetupGetResponse(
            $"schedule/lpu/{lpuId}/doctor/{doctors[0].Id}/appointments",
            new ApiResponse<List<Appointment>>
            {
                Success = false,
                Message = "API error occurred"
            });

        // Act & Assert
        var exception = await Assert.ThrowsAsync<HttpRequestException>(() =>
            _externalAppointmentService.GetBySpecialityAsync(lpuId, specialtyId));

        Assert.Contains("Ошибка при получении номерков: API error occurred", exception.Message);
    }

    [Fact]
    public async Task GetBySpecialityAsync_NoDoctors_ReturnsEmptyList()
    {
        // Arrange
        const int lpuId = 1;
        const string specialtyId = "specialty_123";

        _mockDoctorService
            .Setup(x => x.GetBySpecialtyAsync(lpuId, specialtyId))
            .ReturnsAsync(new List<Doctor>());

        // Act
        var result = await _externalAppointmentService.GetBySpecialityAsync(lpuId, specialtyId);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetByReferralAsync_Success()
    {
        // Arrange
        const int referralNumber = 12345;
        const string lastName = "Иванов";

        var expectedReferrals = new List<ReferralResult>
        {
            new()
            {
                LpuId = "1",
                LpuShortName = "Поликлиника №1",
                LpuFullName = "ГБУЗ Поликлиника №1",
                LpuAddress = "ул. Ленина, д. 10",
                LpuPhone = "+7(812)123-45-67",
                PatId = "patient_456",
                LastName = "Иванов",
                FirstName = "Иван",
                MiddleName = "Иванович",
                BirthDate = new DateTime(1980, 5, 15),
                Specialities = new List<ReferralSpeciality>
                {
                    new()
                    {
                        Id = "spec_1",
                        Name = "Терапевт",
                        Description = "Врач-терапевт",
                        Doctors = new List<ReferralDoctor>()
                    }
                }
            },
            new()
            {
                LpuId = "2",
                LpuShortName = "Поликлиника №2",
                LpuFullName = "ГБУЗ Поликлиника №2",
                LpuAddress = "пр. Победы, д. 25",
                LpuPhone = "+7(812)765-43-21",
                PatId = "patient_456",
                LastName = "Иванов",
                FirstName = "Иван",
                MiddleName = "Иванович",
                BirthDate = new DateTime(1980, 5, 15),
                Specialities = new List<ReferralSpeciality>
                {
                    new()
                    {
                        Id = "spec_2",
                        Name = "Кардиолог",
                        Description = "Врач-кардиолог",
                        Doctors = new List<ReferralDoctor>()
                    }
                }
            }
        };

        var uri = GorzdravApiEndpoints.AppointmentsByReferral(referralNumber, lastName);

        _fakeApiService.SetupGetResponse(uri, new ApiResponse<List<ReferralResult>>
        {
            Success = true,
            Result = expectedReferrals
        });

        // Act
        var result = await _externalAppointmentService.GetByReferralAsync(referralNumber, lastName);

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Equal(expectedReferrals[0].LpuId, result[0].LpuId);
        Assert.Equal(expectedReferrals[0].LpuShortName, result[0].LpuShortName);
        Assert.Equal(expectedReferrals[1].PatId, result[1].PatId);
        Assert.Single(result[0].Specialities);
        Assert.Equal("Терапевт", result[0].Specialities[0].Name);
        Assert.Equal("spec_1", result[0].Specialities[0].Id);
    }

    [Fact]
    public async Task GetByReferralAsync_Failure()
    {
        // Arrange
        const int referralNumber = 12345;
        const string lastName = "Иванов";
        var uri = GorzdravApiEndpoints.AppointmentsByReferral(referralNumber, lastName);

        _fakeApiService.SetupGetResponse(uri, new ApiResponse<List<ReferralResult>>
        {
            Success = false,
            Message = "Referral not found"
        });

        // Act & Assert
        var exception = await Assert.ThrowsAsync<HttpRequestException>(() =>
            _externalAppointmentService.GetByReferralAsync(referralNumber, lastName));

        Assert.Contains("Ошибка при получении номерков: Referral not found", exception.Message);
    }

    [Fact]
    public async Task GetByDoctorAsync_Success()
    {
        // Arrange
        var expectedAppointments = new List<Appointment>
        {
            new()
            {
                Id = "appointment_1",
                VisitStart = new DateTime(2025, 10, 14, 9, 0, 0),
                VisitEnd = new DateTime(2025, 10, 14, 9, 30, 0),
                Address = "ул. Примерная, д. 27, каб. 101",
                Number = "A001",
                Room = "101"
            },
            new()
            {
                Id = "appointment_2",
                VisitStart = new DateTime(2025, 10, 14, 10, 0, 0),
                VisitEnd = new DateTime(2025, 10, 14, 10, 30, 0),
                Address = "ул. Примерная, д. 27, каб. 101",
                Number = "A002",
                Room = "101"
            },
            new()
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
        var exception =
            await Assert.ThrowsAsync<HttpRequestException>(() =>
                _externalAppointmentService.GetByDoctorAsync(lpuId, doctorId));

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
            Result = true,
            ErrorCode = 0
        });

        // Act
        var result = await _externalAppointmentService.CreateAppointmentAsync(createRequest);

        // Assert
        Assert.True(result.IsSucces);
        Assert.Equal(0, result.ErrorCode);
    }

    [Fact]
    public async Task CreateAppointmentAsync_SuccessWithErrorCode()
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
            Success = true,
            Result = true,
            ErrorCode = 42
        });

        // Act
        var result = await _externalAppointmentService.CreateAppointmentAsync(createRequest);

        // Assert
        Assert.True(result.IsSucces);
        Assert.Equal(42, result.ErrorCode);
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
        var exception = await Assert.ThrowsAsync<HttpRequestException>(() =>
            _externalAppointmentService.CreateAppointmentAsync(createRequest));

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
            EsiaId = "esia_123"
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
            EsiaId = "esia_123"
        };

        const string uri = "appointment/cancel";

        _fakeApiService.SetupPostResponse<AppointmentСancelRequest, bool>(uri, new ApiResponse<bool>
        {
            Success = false,
            Message = "Appointment not found"
        });

        // Act & Assert
        var exception = await Assert.ThrowsAsync<HttpRequestException>(() =>
            _externalAppointmentService.CancelAppointmentAsync(cancelRequest));

        Assert.Contains("Ошибка при отмене записи: Appointment not found", exception.Message);
    }

    [Fact]
    public async Task GetByDoctorAsync_UrlEncoding_Success()
    {
        // Arrange
        var expectedAppointments = new List<Appointment>
        {
            new()
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
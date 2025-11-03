using Core.Interfaces.ApiClient;
using Core.Models;
using Infrastructure.ApiClient.Models;

namespace Infrastructure.Stubs;

public class FakeApiService : IApiService
{
    private readonly Dictionary<string, object> _getResponses = new();
    private readonly Dictionary<string, object> _postResponses = new();
    
    private readonly FakeApiDataService _dataService;

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
        // Districts
        var districts = _dataService.GetDistricts();
        SetupGetResponse(GorzdravApiEndpoints.Districts, 
            new ApiResponse<List<District>> { Success = true, Result = districts });

        // LPUs by district
        foreach (var district in districts)
        {
            var lpus = _dataService.GetLpusByDistrict(district.Id);
            SetupGetResponse(GorzdravApiEndpoints.LpusByDistrict(district.Id), 
                new ApiResponse<List<Lpu>> { Success = true, Result = lpus });
        }

        // Specialties by LPU
        var lpuIds = _dataService.GetLpus().Select(l => l.Id).Distinct().ToList();
        foreach (var lpuId in lpuIds)
        {
            var specialties = _dataService.GetSpecialtiesByLpu(lpuId);
            SetupGetResponse(GorzdravApiEndpoints.SpecialtiesByLpu(lpuId), 
                new ApiResponse<List<MedicalSpeciality>> { Success = true, Result = specialties });
        }

        // Doctors by specialty and LPU
        List<Doctor>? doctors;
        foreach (var lpuId in lpuIds)
        {
            var specialties = _dataService.GetSpecialtiesByLpu(lpuId);
            foreach (var specialty in specialties)
            {
                doctors = _dataService.GetDoctorsBySpecialty(lpuId, specialty.Id);
                SetupGetResponse(GorzdravApiEndpoints.DoctorsBySpecialty(lpuId, specialty.Id), 
                    new ApiResponse<List<Doctor>> { Success = true, Result = doctors });
            }
        }
        
        // Appointments by doctor and LPU
        doctors = _dataService.GetDoctors();
        foreach (var doctor in doctors)
        {
            foreach (var lpuId in lpuIds)
            {
                var appointments = _dataService.GetAppointmentsByDoctor(lpuId, doctor.Id);
                SetupGetResponse(GorzdravApiEndpoints.AppointmentsByDoctor(lpuId, doctor.Id), 
                    new ApiResponse<List<Appointment>> { Success = true, Result = appointments });
            }
        }

        // Patient search - демо ответ
        SetupGetResponse($"patient/search?lpuId=1&lastName=Иванов&firstName=Иван&birthdate=01.01.1980&birthdateValue=1980-01-01", 
            new ApiResponse<string> { Success = true, Result = "464211" });

        // POST endpoints
        SetupPostResponse<AppointmentCreateRequest, bool>(GorzdravApiEndpoints.AppointmentCreate, 
            new ApiResponse<bool> { Success = true, Result = true });

        SetupPostResponse<AppointmentСancelRequest, bool>(GorzdravApiEndpoints.AppointmentCancel, 
            new ApiResponse<bool> { Success = true, Result = true });

        SetupPostResponse<PatientPhoneUpdateRequest, bool>(GorzdravApiEndpoints.PatientPhoneUpdate, 
            new ApiResponse<bool> { Success = true, Result = true });

        // Можно добавить специальные сценарии для тестирования ошибок
        SetupSpecialTestScenarios();
    }

    private void SetupSpecialTestScenarios()
    {
        // Специальный сценарий для тестирования ошибки при записи
        var errorResponse = new ApiResponse<bool> 
        { 
            Success = false, 
            ErrorCode = 751,
            Message = "Запись к врачу возможна при отсутствии активной записи к аналогичному специалисту"
        };
        
        // Можно добавить специфичные URL для тестирования ошибок
        // Например, для определенного doctorId или appointmentId
    }
}
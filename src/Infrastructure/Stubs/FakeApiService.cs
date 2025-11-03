using Core.Interfaces.ApiClient;
using Core.Models;
using System.Text.RegularExpressions;

namespace Infrastructure.Stubs;

public class FakeApiService : IApiService
{
    private readonly FakeApiDataService _dataService;

    public FakeApiService(FakeApiDataService dataService)
    {
        _dataService = dataService;
    }

    public Task<ApiResponse<TResponse>> GetAsync<TResponse>(string additionalUri)
    {
        try
        {
            var response = HandleGetRequest<TResponse>(additionalUri);
            return Task.FromResult(response);
        }
        catch (Exception ex)
        {
            return Task.FromResult(new ApiResponse<TResponse>
            {
                Success = false,
                Message = ex.Message
            });
        }
    }

    public Task<ApiResponse<TResponse>> PostAsync<TRequest, TResponse>(string additionalUri, TRequest data)
    {
        try
        {
            var response = HandlePostRequest<TRequest, TResponse>(additionalUri, data);
            return Task.FromResult(response);
        }
        catch (Exception ex)
        {
            return Task.FromResult(new ApiResponse<TResponse>
            {
                Success = false,
                Message = ex.Message
            });
        }
    }

    // Для тестов - пустые реализации
    public void SetupGetResponse<TResponse>(string uri, ApiResponse<TResponse> response) { }
    public void SetupPostResponse<TRequest, TResponse>(string uri, ApiResponse<TResponse> response) { }

    private ApiResponse<TResponse> HandleGetRequest<TResponse>(string additionalUri)
    {
        // Убираем начальный слеш если есть
        var uri = additionalUri.TrimStart('/');

        return uri switch
        {
            "shared/districts" => 
                SuccessResponse<TResponse>(_dataService.GetDistricts()),

            string s when Regex.IsMatch(s, @"^shared/district/(\d+)/lpus$") => 
                HandleLpusByDistrict<TResponse>(s),

            string s when Regex.IsMatch(s, @"^schedule/lpu/(\d+)/specialties$") => 
                HandleSpecialtiesByLpu<TResponse>(s),

            string s when Regex.IsMatch(s, @"^schedule/lpu/(\d+)/speciality/([^/]+)/doctors$") => 
                HandleDoctorsBySpecialty<TResponse>(s),

            string s when Regex.IsMatch(s, @"^schedule/lpu/(\d+)/doctor/([^/]+)/appointments$") => 
                HandleAppointmentsByDoctor<TResponse>(s),

            string s when s.StartsWith("patient/search") => 
                HandlePatientSearch<TResponse>(s),

            _ => ErrorResponse<TResponse>($"URL '{additionalUri}' not configured in fake service")
        };
    }

    private ApiResponse<TResponse> HandlePostRequest<TRequest, TResponse>(string additionalUri, TRequest data)
    {
        var uri = additionalUri.TrimStart('/');
    
        return uri switch
        {
            "appointment/create" when data is AppointmentCreateRequest request => 
                HandleAppointmentCreate<TResponse>(request),
    
            "appointment/cancel" when data is AppointmentСancelRequest request => 
                HandleAppointmentCancel<TResponse>(request),
    
            "patient/update" when data is PatientPhoneUpdateRequest request => 
                HandlePatientUpdate<TResponse>(request),
    
            _ => new ApiResponse<TResponse> { Success = false, Message = $"URL '{additionalUri}' not found" }
        };
    }
    
    // Обработчики POST запросов
    
    private ApiResponse<TResponse> HandleAppointmentCreate<TResponse>(AppointmentCreateRequest request)
    {
        var result = _dataService.CreateAppointment(request);
        
        return new ApiResponse<TResponse>
        {
            Success = result.Success,
            Result = (TResponse)(object)result.Result,
            Message = result.Message,
            ErrorCode = result.ErrorCode
        };
    }
    
    private ApiResponse<TResponse> HandleAppointmentCancel<TResponse>(AppointmentСancelRequest request)
    {
        var result = _dataService.CancelAppointment(request);
        return new ApiResponse<TResponse>
        {
            Success = result.Success,
            Result = (TResponse)(object)result.Result,
            Message = result.Message,
            ErrorCode = result.ErrorCode
        };
    }
    
    private ApiResponse<TResponse> HandlePatientUpdate<TResponse>(PatientPhoneUpdateRequest request)
    {
        var result = _dataService.UpdatePatientPhone(request);
        return new ApiResponse<TResponse>
        {
            Success = result.Success,
            Result = (TResponse)(object)result.Result,
            Message = result.Message,
            ErrorCode = result.ErrorCode
        };
    }

    // Обработчики GET запросов
    private ApiResponse<TResponse> HandleLpusByDistrict<TResponse>(string uri)
    {
        var match = Regex.Match(uri, @"^shared/district/(\d+)/lpus$");
        if (match.Success)
        {
            var districtId = match.Groups[1].Value;
            var lpus = _dataService.GetLpusByDistrict(districtId);
            return SuccessResponse<TResponse>(lpus);
        }
        return ErrorResponse<TResponse>("Invalid district URL");
    }

    private ApiResponse<TResponse> HandleSpecialtiesByLpu<TResponse>(string uri)
    {
        var match = Regex.Match(uri, @"^schedule/lpu/(\d+)/specialties$");
        if (match.Success)
        {
            var lpuId = int.Parse(match.Groups[1].Value);
            var specialties = _dataService.GetSpecialtiesByLpu(lpuId);
            return SuccessResponse<TResponse>(specialties);
        }
        return ErrorResponse<TResponse>("Invalid specialties URL");
    }

    private ApiResponse<TResponse> HandleDoctorsBySpecialty<TResponse>(string uri)
    {
        var match = Regex.Match(uri, @"^schedule/lpu/(\d+)/speciality/([^/]+)/doctors$");
        if (match.Success)
        {
            var lpuId = int.Parse(match.Groups[1].Value);
            var specialtyId = Uri.UnescapeDataString(match.Groups[2].Value);
            var doctors = _dataService.GetDoctorsBySpecialty(lpuId, specialtyId);
            return SuccessResponse<TResponse>(doctors);
        }
        return ErrorResponse<TResponse>("Invalid doctors URL");
    }

    private ApiResponse<TResponse> HandleAppointmentsByDoctor<TResponse>(string uri)
    {
        var match = Regex.Match(uri, @"^schedule/lpu/(\d+)/doctor/([^/]+)/appointments$");
        if (match.Success)
        {
            var lpuId = int.Parse(match.Groups[1].Value);
            var doctorId = Uri.UnescapeDataString(match.Groups[2].Value);
            var appointments = _dataService.GetAppointmentsByDoctor(lpuId, doctorId);
            return SuccessResponse<TResponse>(appointments);
        }
        return ErrorResponse<TResponse>("Invalid appointments URL");
    }

    private ApiResponse<TResponse> HandlePatientSearch<TResponse>(string uri)
    {
        var queryString = uri.Contains('?') ? uri.Split('?')[1] : "";
        var queryParams = ParseQueryString(queryString);
        
        var request = new PatientIdSearchRequest
        {
            LpuId = queryParams["lpuId"] ?? "1",
            LastName = queryParams["lastName"] ?? "",
            FirstName = queryParams["firstName"] ?? "",
            MiddleName = queryParams["middleName"] ?? "",
            BirthDate = DateTime.Parse(queryParams["birthdateValue"] ?? DateTime.Now.AddYears(-30).ToString("yyyy-MM-dd"))
        };

        var patientId = _dataService.GetPatientId(request);
        return SuccessResponse<TResponse>(patientId);
    }

    // Вспомогательные методы
    private ApiResponse<TResponse> SuccessResponse<TResponse>(object result)
    {
        return new ApiResponse<TResponse> 
        { 
            Success = true, 
            Result = (TResponse)result 
        };
    }

    private ApiResponse<TResponse> ErrorResponse<TResponse>(string message)
    {
        return new ApiResponse<TResponse> 
        { 
            Success = false, 
            Message = message 
        };
    }

    private System.Collections.Specialized.NameValueCollection ParseQueryString(string query)
    {
        var collection = new System.Collections.Specialized.NameValueCollection();
        foreach (var pair in query.Split('&'))
        {
            var parts = pair.Split('=');
            if (parts.Length == 2)
            {
                collection[parts[0]] = Uri.UnescapeDataString(parts[1]);
            }
        }
        return collection;
    }
}
using Core.Models;

namespace Infrastructure.Stubs;

public class FakeApiDataService
{
    private readonly List<District> _districts;
    private readonly List<Lpu> _lpus;
    private readonly List<MedicalSpeciality> _specialties;
    private readonly List<Doctor> _doctors;
    private readonly List<Appointment> _appointments;

    public FakeApiDataService()
    {
        _districts = GenerateDistricts();
        _lpus = GenerateLpus();
        _specialties = GenerateSpecialties();
        _doctors = GenerateDoctors();
        _appointments = GenerateAppointments();
    }

    // Districts
    public List<District> GetDistricts() => _districts;
    public List<Lpu> GetLpusByDistrict(string districtId) => 
        _lpus.Where(l => l.DistrictId.ToString() == districtId).ToList();

    // Lpus
    public List<Lpu> GetLpus() => _lpus;
    public Lpu? GetLpu(int lpuId) => _lpus.FirstOrDefault(l => l.Id == lpuId);

    // Specialties
    public List<MedicalSpeciality> GetSpecialtiesByLpu(int lpuId) => _specialties;
    public MedicalSpeciality? GetSpecialty(string specialtyId) => 
        _specialties.FirstOrDefault(s => s.Id == specialtyId);

    // Doctors
    public List<Doctor> GetDoctorsBySpecialty(int lpuId, string specialtyId) => _doctors;
    public Doctor? GetDoctor(string doctorId) => _doctors.FirstOrDefault(d => d.Id == doctorId);

    // Appointments
    public List<Appointment> GetAppointmentsByDoctor(int lpuId, string doctorId) => _appointments;
    
    // Patient search
    public string SearchPatient(PatientIdSearchRequest request) => "464211"; // demo patient ID
    
    // Appointment operations
    public ApiResponse<bool> CreateAppointment(AppointmentCreateRequest request)
    {
        // Можешь добавить логику для разных сценариев
        if (request.AppointmentId == "11806743") 
        {
            return new ApiResponse<bool> 
            { 
                Success = false, 
                ErrorCode = 751,
                Message = "Запись к врачу возможна при отсутствии активной записи к аналогичному специалисту"
            };
        }
        
        return new ApiResponse<bool> { Success = true, Result = true };
    }
    
    public ApiResponse<bool> CancelAppointment(AppointmentСancelRequest request) => 
        new ApiResponse<bool> { Success = true, Result = true };
    
    public ApiResponse<bool> UpdatePatientPhone(PatientPhoneUpdateRequest request) => 
        new ApiResponse<bool> { Success = true, Result = true };

    // Private methods for demo data generation
    private List<District> GenerateDistricts()
    {
        return new List<District>
        {
            new District { Id = "1", Name = "Адмиралтейский", Okato = 40262 },
            new District { Id = "2", Name = "Василеостровский", Okato = 40263 },
            new District { Id = "3", Name = "Выборгский", Okato = 40265 },
            new District { Id = "4", Name = "Калининский", Okato = 40267 },
            new District { Id = "5", Name = "Кировский", Okato = 40269 },
            new District { Id = "6", Name = "Колпинский", Okato = 40275 },
            new District { Id = "7", Name = "Красногвардейский", Okato = 40279 },
            new District { Id = "8", Name = "Красносельский", Okato = 40295 },
            new District { Id = "9", Name = "Кронштадтский", Okato = 40305 },
            new District { Id = "10", Name = "Курортный", Okato = 40385 },
            new District { Id = "11", Name = "Московский", Okato = 40390 },
            new District { Id = "12", Name = "Невский", Okato = 40395 },
            new District { Id = "13", Name = "Петроградский", Okato = 40296 },
            new District { Id = "14", Name = "Петродворцовый", Okato = 40390 },
            new District { Id = "15", Name = "Приморский", Okato = 40397 },
            new District { Id = "16", Name = "Пушкинский", Okato = 40395 },
            new District { Id = "17", Name = "Фрунзенский", Okato = 40398 },
            new District { Id = "18", Name = "Центральный", Okato = 40301 }
        };
    }

    private List<Lpu> GenerateLpus()
    {
        return new List<Lpu>
        {
            new Lpu 
            { 
                Id = 187,
                Description = "Ариадна",
                District = 17,
                DistrictId = 17,
                DistrictName = "Фрунзенский",
                IsActive = true,
                LpuFullName = "СПб ГБУЗ \"Городская поликлиника №44\" Женская консультация №19",
                LpuShortName = "ГП №44 ЖК №19", 
                LpuType = "Женская консультация",
                HeadOrganization = "d536347b-4105-49a1-bae6-0b3114e89aa0",
                Organization = "03c5c829-3e51-4098-9966-5e7a443001c7",
                Address = "192241, Санкт-Петербург, ул. Пражская, д. 12, к. 1",
                Phone = "(812) 246-25-66",
                Email = "p44@zdrav.spb.ru",
                Longitude = "30.3854",
                Latitude = "59.8782",
                CovidVaccination = false,
                InDepthExamination = false
            },
            new Lpu 
            { 
                Id = 268,
                Description = "Будапештская",
                District = 17, 
                DistrictId = 17,
                DistrictName = "Фрунзенский",
                IsActive = true,
                LpuFullName = "СПб ГБУЗ \"Городская поликлиника №44\"",
                LpuShortName = "ГП №44",
                LpuType = "Поликлиника",
                HeadOrganization = "d536347b-4105-49a1-bae6-0b3114e89aa0", 
                Organization = "03c5c829-3e51-4098-9966-5e7a443001c7",
                Address = "192239, Санкт-Петербург, ул. Будапештская, д. 63, к. 2",
                Phone = "(812) 246-25-66",
                Email = "p44@zdrav.spb.ru",
                Longitude = "30.3854",
                Latitude = "59.8782",
                CovidVaccination = false,
                InDepthExamination = false
            }
        };
    }

    private List<MedicalSpeciality> GenerateSpecialties()
    {
        return new List<MedicalSpeciality>
        {
            new MedicalSpeciality 
            { 
                Id = "92134141", 
                FerId = "77", 
                Name = "Аллергология и иммунология",
                CountFreeParticipant = 25,
                CountFreeTicket = 25,
                LastDate = "2025-08-26T19:00:00",
                NearestDate = "2025-08-22T10:00:00"
            },
            new MedicalSpeciality 
            { 
                Id = "92137183", 
                FerId = "55", 
                Name = "Вакцинация от COVID-19",
                CountFreeParticipant = 126,
                CountFreeTicket = 126, 
                LastDate = "2025-08-26T13:55:00",
                NearestDate = "2025-08-12T15:15:00"
            },
            new MedicalSpeciality 
            { 
                Id = "92134142", 
                FerId = "78", 
                Name = "Терапия",
                CountFreeParticipant = 150,
                CountFreeTicket = 150,
                LastDate = "2025-08-30T18:00:00", 
                NearestDate = "2025-08-15T09:00:00"
            }
        };
    }

    private List<Doctor> GenerateDoctors()
    {
        return new List<Doctor>
        {
            new Doctor
            {
                AriaNumber = "23, 26, 27, 28, 35, 36, 37, 38, 41, 42, 43, 44, 45",
                Comment = "28.07.2025-17.08.2025: Приём временно не ведётся.",
                FreeParticipantCount = 0,
                FreeTicketCount = 0,
                Id = "518", 
                Name = "Арутюнян Арпине Араиковна",
                LastName = "Арутюнян",
                FirstName = "Арпине", 
                MiddleName = "Араиковна"
            },
            new Doctor
            {
                AriaNumber = "101, 102, 103",
                Comment = "",
                FreeParticipantCount = 15,
                FreeTicketCount = 15, 
                Id = "519",
                Name = "Иванов Иван Иванович",
                LastName = "Иванов",
                FirstName = "Иван",
                MiddleName = "Иванович",
                NearestDate = DateTime.Today.AddDays(1)
            }
        };
    }

    private List<Appointment> GenerateAppointments()
    {
        var baseDate = DateTime.Today.AddDays(1);
        return new List<Appointment>
        {
            new Appointment 
            { 
                Id = "11806734",
                VisitStart = baseDate.AddHours(10),
                VisitEnd = baseDate.AddHours(10).AddMinutes(20),
                Room = "221"
            },
            new Appointment 
            { 
                Id = "11806727", 
                VisitStart = baseDate.AddHours(12).AddMinutes(20),
                VisitEnd = baseDate.AddHours(12).AddMinutes(40),
                Room = "221"
            },
            new Appointment 
            { 
                Id = "11806743",
                VisitStart = baseDate.AddDays(1).AddHours(11),
                VisitEnd = baseDate.AddDays(1).AddHours(11).AddMinutes(20),
                Room = "221",
                Address = "192239, Санкт-Петербург, ул. Будапештская, д. 63, к. 2"
            }
        };
    }
}
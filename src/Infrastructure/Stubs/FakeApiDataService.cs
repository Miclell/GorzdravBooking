using Core.Models;

namespace Infrastructure.Stubs;

public class FakeApiDataService
{
    private readonly Dictionary<string, List<Appointment>> _appointmentsByDoctor;
    private readonly List<District> _districts;
    private readonly List<Doctor> _doctors;
    private readonly List<Lpu> _lpus;
    private readonly Dictionary<string, string> _patientIds;
    private readonly Random _random;
    private readonly List<MedicalSpeciality> _specialties;

    public FakeApiDataService()
    {
        _random = new Random();
        _patientIds = new Dictionary<string, string>();
        _districts = GenerateDistricts();
        _lpus = GenerateLpus();
        _specialties = GenerateSpecialties();
        _doctors = GenerateDoctors();
        _appointmentsByDoctor = new Dictionary<string, List<Appointment>>();
        InitializePatientIds();
    }

    // Districts
    public List<District> GetDistricts()
    {
        return _districts;
    }

    public List<Lpu> GetLpusByDistrict(string districtId)
    {
        return _lpus.Where(l => l.DistrictId.ToString() == districtId).ToList();
    }

    // Lpus
    public List<Lpu> GetLpus()
    {
        return _lpus;
    }

    public Lpu? GetLpu(int lpuId)
    {
        return _lpus.FirstOrDefault(l => l.Id == lpuId);
    }

    // Specialties
    public List<MedicalSpeciality> GetSpecialtiesByLpu(int lpuId)
    {
        return _specialties;
    }

    public MedicalSpeciality? GetSpecialty(string specialtyId)
    {
        return _specialties.FirstOrDefault(s => s.Id == specialtyId);
    }

    // Doctors
    public List<Doctor> GetDoctorsBySpecialty(int lpuId, string specialtyId)
    {
        return _doctors;
    }

    public List<Doctor> GetDoctors()
    {
        return _doctors;
    }

    public Doctor? GetDoctor(string doctorId)
    {
        return _doctors.FirstOrDefault(d => d.Id == doctorId);
    }

    // Appointments - основная логика
    public List<Appointment> GetAppointmentsByDoctor(int lpuId, string doctorId)
    {
        var key = $"{lpuId}_{doctorId}";

        // 70% вероятность обновить номерки
        if (_random.Next(100) < 70 || !_appointmentsByDoctor.ContainsKey(key))
            GenerateAppointmentsForDoctor(lpuId, doctorId);

        return _appointmentsByDoctor.TryGetValue(key, out var appointments)
            ? appointments
            : new List<Appointment>();
    }

    public void AddAppointment(string doctorKey, Appointment appointment)
    {
        if (!_appointmentsByDoctor.ContainsKey(doctorKey))
            _appointmentsByDoctor[doctorKey] = new List<Appointment>();

        _appointmentsByDoctor[doctorKey].Add(appointment);
    }

    public bool RemoveAppointment(string appointmentId)
    {
        foreach (var (doctorKey, appointments) in _appointmentsByDoctor)
        {
            var appointment = appointments.FirstOrDefault(a => a.Id == appointmentId);
            if (appointment != null)
            {
                appointments.Remove(appointment);
                return true;
            }
        }

        return false;
    }

    // Patient search
    public string GetPatientId(PatientIdSearchRequest request)
    {
        var key = $"{request.LpuId}_{request.LastName}_{request.FirstName}_{request.BirthDate:yyyyMMdd}";

        if (!_patientIds.TryGetValue(key, out var patientId))
        {
            patientId = $"{request.LpuId}{_random.Next(10000, 99999)}";
            _patientIds[key] = patientId;
        }

        return patientId;
    }

    // Appointment operations
    public ApiResponse<bool> CreateAppointment(AppointmentCreateRequest request)
    {
        // 10% вероятность ошибки "уже есть запись"
        if (_random.Next(100) < 10)
            return new ApiResponse<bool>
            {
                Success = false,
                ErrorCode = 751,
                Message = "Запись к врачу возможна при отсутствии активной записи к аналогичному специалисту"
            };

        // Удаляем номерок из доступных
        if (!RemoveAppointment(request.AppointmentId))
            return new ApiResponse<bool>
            {
                Success = false,
                ErrorCode = 404,
                Message = "Талон не найден"
            };

        return new ApiResponse<bool> { Success = true, Result = true };
    }

    public ApiResponse<bool> CancelAppointment(AppointmentСancelRequest request)
    {
        // В реальном API при отмене нам не нужно возвращать номерок в пул,
        // т.к. это уже обработанное время, которое может быть занято другими пациентами

        // Просто удаляем запись из системы (если бы у нас была история записей)
        // и возвращаем успех

        // В нашем фейковом сервисе просто логируем отмену
        Console.WriteLine(
            $"Отменена запись: AppointmentId={request.AppointmentId}, LpuId={request.LpuId}, PatientId={request.PatientId}");

        return new ApiResponse<bool> { Success = true, Result = true };
    }

    public ApiResponse<bool> UpdatePatientPhone(PatientPhoneUpdateRequest request)
    {
        return new ApiResponse<bool> { Success = true, Result = true };
    }

    // Private methods
    private void InitializePatientIds()
    {
        _patientIds["1_Иванов_Иван_19800101"] = "1464211";
        _patientIds["1_Петров_Петр_19850215"] = "1464212";
        _patientIds["2_Сидоров_Алексей_19900320"] = "2464213";
        _patientIds["2_Козлова_Мария_19871110"] = "2464214";
    }

    private void GenerateAppointmentsForDoctor(int lpuId, string doctorId)
    {
        var key = $"{lpuId}_{doctorId}";
        var appointments = new List<Appointment>();
        var now = DateTime.Now;

        // Генерируем на 7 дней вперед, только рабочие дни
        for (var i = 0; i < 7; i++)
        {
            var date = now.AddDays(i);
            if (date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday)
                continue;

            // Пропускаем прошедшие дни и время
            if (date.Date < now.Date || (date.Date == now.Date && now.Hour >= 20))
                continue;

            var dayAppointments = GenerateAppointmentsForDay(date, now);
            appointments.AddRange(dayAppointments);
        }

        _appointmentsByDoctor[key] = appointments;
    }

    private List<Appointment> GenerateAppointmentsForDay(DateTime date, DateTime now)
    {
        var appointments = new List<Appointment>();
        var startHour = date.Date == now.Date ? Math.Max(8, now.Hour + 1) : 8;

        // λ = 0.3 - в среднем 30% слотов заполнены
        var expectedAppointments = _random.Next(5, 12); // 5-12 номерков в день

        for (var i = 0; i < expectedAppointments; i++)
        {
            var hour = _random.Next(startHour, 20);
            var minute = _random.Next(0, 3) * 20; // 0, 20, 40 минут

            var visitStart = new DateTime(date.Year, date.Month, date.Day, hour, minute, 0);

            // Пропускаем если время уже прошло
            if (visitStart <= now)
                continue;

            var visitEnd = visitStart.AddMinutes(20);

            appointments.Add(new Appointment
            {
                Id = GenerateAppointmentId(),
                VisitStart = visitStart,
                VisitEnd = visitEnd,
                Room = $"2{_random.Next(10, 30)}",
                Address = GetRandomAddress()
            });
        }

        // Сортируем по времени
        return appointments.OrderBy(a => a.VisitStart).ToList();
    }

    private string GenerateAppointmentId()
    {
        return _random.Next(10000000, 99999999).ToString();
    }

    private string GetRandomAddress()
    {
        var addresses = new[]
        {
            "111111, Санкт-Петербург, ул. Центральная, д. 15, лит. А",
            "111112, Санкт-Петербург, пр. Медицинский, д. 42, к. 3",
            "222111, Санкт-Петербург, ул. Музыкальная, д. 28, лит. Б",
            "222112, Санкт-Петербург, пр. Оркестровый, д. 57, к. 2"
        };
        return addresses[_random.Next(addresses.Length)];
    }

    // Остальные методы GenerateDistricts, GenerateLpus, GenerateSpecialties, GenerateDoctors
    // остаются как были ранее (в твоем коде)
    private List<District> GenerateDistricts()
    {
        return new List<District>
        {
            new() { Id = "1", Name = "Балалайковский", Okato = 1 },
            new() { Id = "2", Name = "Оркестровый", Okato = 2 }
        };
    }

    private List<Lpu> GenerateLpus()
    {
        return new List<Lpu>
        {
            new()
            {
                Id = 1,
                Description = "Центральная поликлиника района",
                District = 1,
                DistrictId = 1,
                DistrictName = "Балалайковский",
                IsActive = true,
                LpuFullName = "СПб ГБУЗ \"Городская поликлиника №101\"",
                LpuShortName = "ГП №101",
                LpuType = "Поликлиника",
                HeadOrganization = "a536347b-4105-49a1-bae6-0b3114e89aa1",
                Organization = "13c5c829-3e51-4098-9966-5e7a443001c8",
                Address = "111111, Санкт-Петербург, ул. Центральная, д. 15, лит. А",
                Phone = "(812) 111-22-33",
                Email = "gp101@med.spb.ru",
                Longitude = "30.315644",
                Latitude = "59.938784",
                CovidVaccination = true,
                InDepthExamination = true
            },
            new()
            {
                Id = 2,
                Description = "Современный медицинский комплекс",
                District = 1,
                DistrictId = 1,
                DistrictName = "Балалайковский",
                IsActive = true,
                LpuFullName = "СПб ГБУЗ \"Районная поликлиника №205\"",
                LpuShortName = "РП №205",
                LpuType = "Поликлиника",
                HeadOrganization = "b536347b-4105-49a1-bae6-0b3114e89aa2",
                Organization = "23c5c829-3e51-4098-9966-5e7a443001c9",
                Address = "111112, Санкт-Петербург, пр. Медицинский, д. 42, к. 3",
                Phone = "(812) 111-44-55",
                Email = "rp205@med.spb.ru",
                Longitude = "30.325644",
                Latitude = "59.948784",
                CovidVaccination = true,
                InDepthExamination = false
            },
            new()
            {
                Id = 3,
                Description = "Поликлиника с детским отделением",
                District = 2,
                DistrictId = 2,
                DistrictName = "Оркестровый",
                IsActive = true,
                LpuFullName = "СПб ГБУЗ \"Городская поликлиника №76\"",
                LpuShortName = "ГП №76",
                LpuType = "Поликлиника",
                HeadOrganization = "c536347b-4105-49a1-bae6-0b3114e89aa3",
                Organization = "33c5c829-3e51-4098-9966-5e7a443001d0",
                Address = "222111, Санкт-Петербург, ул. Музыкальная, д. 28, лит. Б",
                Phone = "(812) 222-33-44",
                Email = "gp76@med.spb.ru",
                Longitude = "30.335644",
                Latitude = "59.958784",
                CovidVaccination = false,
                InDepthExamination = true
            },
            new()
            {
                Id = 4,
                Description = "Многопрофильный медицинский центр",
                District = 2,
                DistrictId = 2,
                DistrictName = "Оркестровый",
                IsActive = true,
                LpuFullName = "СПб ГБУЗ \"Клинико-диагностический центр №5\"",
                LpuShortName = "КДЦ №5",
                LpuType = "Поликлиника",
                HeadOrganization = "d536347b-4105-49a1-bae6-0b3114e89aa4",
                Organization = "43c5c829-3e51-4098-9966-5e7a443001d1",
                Address = "222112, Санкт-Петербург, пр. Оркестровый, д. 57, к. 2",
                Phone = "(812) 222-55-66",
                Email = "kdc5@med.spb.ru",
                Longitude = "30.345644",
                Latitude = "59.968784",
                CovidVaccination = true,
                InDepthExamination = true
            }
        };
    }

    private List<MedicalSpeciality> GenerateSpecialties()
    {
        return new List<MedicalSpeciality>
        {
            new()
            {
                Id = "92134141",
                FerId = "77",
                Name = "Аллергология и иммунология",
                CountFreeParticipant = 25,
                CountFreeTicket = 25,
                LastDate = "2025-08-26T19:00:00",
                NearestDate = "2025-08-22T10:00:00"
            },
            new()
            {
                Id = "92137183",
                FerId = "55",
                Name = "Вакцинация от COVID-19",
                CountFreeParticipant = 126,
                CountFreeTicket = 126,
                LastDate = "2025-08-26T13:55:00",
                NearestDate = "2025-08-12T15:15:00"
            },
            new()
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
            new()
            {
                AriaNumber = "15, 16, 17, 18, 25, 26, 27, 28, 31, 32, 33, 34, 35",
                Comment = "01.09.2025-15.09.2025: Врач в учебном отпуске.",
                FreeParticipantCount = 0,
                FreeTicketCount = 0,
                Id = "701",
                Name = "Соколова Анна Михайловна",
                LastName = "Соколова",
                FirstName = "Анна",
                MiddleName = "Михайловna",
                NearestDate = DateTime.Today.AddDays(1)
            },
            new()
            {
                AriaNumber = "201, 202, 203",
                Comment = "",
                FreeParticipantCount = 12,
                FreeTicketCount = 12,
                Id = "702",
                Name = "Лебедев Дмитрий Сергеевич",
                LastName = "Лебедев",
                FirstName = "Дмитрий",
                MiddleName = "Сергеевич",
                NearestDate = DateTime.Today.AddDays(1)
            },
            new()
            {
                AriaNumber = "45, 46, 47, 48, 49, 50",
                Comment = "Работает только с хроническими пациентами",
                FreeParticipantCount = 8,
                FreeTicketCount = 8,
                Id = "703",
                Name = "Воронцов Алексей Петрович",
                LastName = "Воронцов",
                FirstName = "Алексей",
                MiddleName = "Петрович",
                NearestDate = DateTime.Today.AddDays(3)
            }
        };
    }
}
using System.Web;
using Core.Models;

namespace Infrastructure.ApiClient;

/// <summary>
///     Относительные пути API Горздрава.
///     Методы возвращают готовые сегменты URL с подставленными параметрами.
/// </summary>
public static class GorzdravApiEndpoints
{
    /// <summary>
    ///     Получение списка районов.
    /// </summary>
    public const string Districts = "shared/districts";

    /// <summary>
    ///     Создание записи на приём.
    /// </summary>
    public const string AppointmentCreate = "appointment/create";

    /// <summary>
    ///     Отмена записи на приём.
    /// </summary>
    public const string AppointmentCancel = "appointment/cancel";

    /// <summary>
    ///     Обновление номера телефона в медицинской организации
    /// </summary>
    public static string PatientPhoneUpdate = "patient/update";

    /// <summary>
    ///     Получение списка организаций района.
    /// </summary>
    /// <remarks>
    ///     <paramref name="districtId" /> — идентификатор района.
    /// </remarks>
    public static string LpusByDistrict(string districtId)
    {
        return $"shared/district/{E(districtId)}/lpus";
    }

    /// <summary>
    ///     Получение списка специальностей организации (LPU).
    /// </summary>
    /// <remarks>
    ///     <paramref name="lpuId" /> — идентификатор организации (LPU).
    /// </remarks>
    public static string SpecialtiesByLpu(int lpuId)
    {
        return $"schedule/lpu/{lpuId}/specialties";
    }

    /// <summary>
    ///     Получение списка докторов по специальности.
    /// </summary>
    /// <remarks>
    ///     <paramref name="lpuId" /> — идентификатор LPU. <br />
    ///     <paramref name="specialityId" /> — идентификатор специальности (строка).
    /// </remarks>
    public static string DoctorsBySpecialty(int lpuId, string specialityId)
    {
        return $"schedule/lpu/{lpuId}/speciality/{E(specialityId)}/doctors";
    }

    /// <summary>
    ///     Получение доступных талонов врача.
    /// </summary>
    /// <remarks>
    ///     <paramref name="lpuId" /> — идентификатор LPU. <br />
    ///     <paramref name="doctorId" /> — идентификатор врача (строка).
    /// </remarks>
    public static string AppointmentsByDoctor(int lpuId, string doctorId)
    {
        return $"schedule/lpu/{lpuId}/doctor/{E(doctorId)}/appointments";
    }

    /// <summary>
    ///     Получение доступных врачей и талонов по направлению.
    /// </summary>
    /// <remarks>
    ///     <paramref name="referralNumber" /> — номер направления. <br />
    ///     <paramref name="lastName" /> — фамилия (строка).
    /// </remarks>
    public static string AppointmentsByReferral(int referralNumber, string lastName)
    {
        return $"referral/{referralNumber}?lastName={E(lastName)}";
    }

    /// <summary>
    ///     Получение Id пациента.
    /// </summary>
    /// <remarks>
    ///     <paramref name="request" /> — DTO запроса.
    /// </remarks>
    public static string PatientIdSearch(PatientIdSearchRequest request)
    {
        var query = HttpUtility.ParseQueryString(string.Empty);
        query["lpuId"] = request.LpuId;
        query["lastName"] = request.LastName;
        query["firstName"] = request.FirstName;
        if (!string.IsNullOrEmpty(request.MiddleName))
            query["middleName"] = request.MiddleName;
        query["birthdate"] = request.BirthDate.ToString("dd.MM.yyyy");
        query["birthdateValue"] = request.BirthDate.ToString("yyyy-MM-dd");

        return $"patient/search?{query}";
    }

    private static string E(string value)
    {
        return Uri.EscapeDataString(value ?? string.Empty);
    }
}
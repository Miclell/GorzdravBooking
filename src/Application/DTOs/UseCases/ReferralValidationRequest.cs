using StatefulMenu.Core.Attributes;

namespace Application.DTOs.UseCases;

[InputModel("номера направления")]
public record ReferralValidationRequest(
    Guid UserId,
    [property: InputField("Введите номер направления")]
    string ReferralNumber,
    [property: InputField("Введите фамилию")]
    string LastName
);
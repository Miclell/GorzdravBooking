namespace Application.DTOs.User;

public record UserResponse(
    string Username,
    Guid Id);
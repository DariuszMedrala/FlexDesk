namespace FlexDesk.Application.Authentication.Dtos;

public record AuthResponse(
    string Token,
    DateTime ExpiresAtUtc,
    string Email,
    string Role,
    string FirstName,
    string LastName);

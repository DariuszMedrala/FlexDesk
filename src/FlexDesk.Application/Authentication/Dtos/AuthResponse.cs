namespace FlexDesk.Application.Authentication.Dtos;

public record AuthResponse(string Token, DateTime ExpiresAtUtc, string Email, string Role);

namespace FlexDesk.Application.Authentication.Dtos;

public record RegisterRequest(string Email, string Password, string FirstName, string LastName);

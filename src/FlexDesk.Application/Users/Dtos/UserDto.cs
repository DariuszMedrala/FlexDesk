using FlexDesk.Domain.Enums;

namespace FlexDesk.Application.Users.Dtos;

public record UserDto(
    Guid Id,
    string Email,
    string FirstName,
    string LastName,
    UserRole Role,
    DateTime CreatedAt);

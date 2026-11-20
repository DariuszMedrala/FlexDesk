using FlexDesk.Domain.Enums;

namespace FlexDesk.Application.Account.Dtos;

public record ProfileDto(
    Guid Id,
    string Email,
    string FirstName,
    string LastName,
    UserRole Role);

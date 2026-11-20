namespace FlexDesk.Application.Account.Dtos;

public record ChangePasswordRequest(string CurrentPassword, string NewPassword);

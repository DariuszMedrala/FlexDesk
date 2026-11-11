namespace FlexDesk.Application.Common.Security;

public record AccessToken(string Value, DateTime ExpiresAtUtc);

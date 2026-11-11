namespace FlexDesk.Application.Common.Security;

public interface ICurrentUser
{
    Guid? UserId { get; }

    string? Email { get; }

    bool IsAuthenticated { get; }

    bool IsAdmin { get; }
}

using FlexDesk.Domain.Entities;

namespace FlexDesk.Application.Common.Security;

public interface IJwtTokenGenerator
{
    AccessToken Generate(User user);
}

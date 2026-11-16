using FlexDesk.Application.Common.Models;
using FlexDesk.Application.Users.Dtos;

namespace FlexDesk.Application.Users;

public interface IUserService
{
    Task<PagedResult<UserDto>> GetAsync(UserQuery query, CancellationToken cancellationToken = default);
}

using FlexDesk.Application.Common.Models;
using FlexDesk.Application.Common.Persistence;
using FlexDesk.Application.Users.Dtos;
using Microsoft.EntityFrameworkCore;

namespace FlexDesk.Application.Users;

public class UserService : IUserService
{
    private readonly IApplicationDbContext _context;

    public UserService(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PagedResult<UserDto>> GetAsync(UserQuery query, CancellationToken cancellationToken = default)
    {
        var page = Math.Max(1, query.Page);
        var pageSize = Math.Clamp(query.PageSize, 1, 100);

        var users = _context.Users.AsNoTracking();

        var totalCount = await users.CountAsync(cancellationToken);

        var items = await users
            .OrderBy(u => u.LastName)
            .ThenBy(u => u.FirstName)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(u => new UserDto(u.Id, u.Email, u.FirstName, u.LastName, u.Role, u.CreatedAt))
            .ToListAsync(cancellationToken);

        return new PagedResult<UserDto>(items, page, pageSize, totalCount);
    }
}

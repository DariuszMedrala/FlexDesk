using FlexDesk.Application.Common.Models;
using FlexDesk.Application.Users;
using FlexDesk.Application.Users.Dtos;
using FlexDesk.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FlexDesk.Api.Controllers;

[ApiController]
[Route("api/users")]
[Authorize(Roles = nameof(UserRole.Admin))]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;

    public UsersController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpGet]
    public async Task<ActionResult<PagedResult<UserDto>>> Get([FromQuery] UserQuery query, CancellationToken cancellationToken)
    {
        var users = await _userService.GetAsync(query, cancellationToken);
        return Ok(users);
    }
}

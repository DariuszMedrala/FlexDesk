using FlexDesk.Application.Account;
using FlexDesk.Application.Account.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FlexDesk.Api.Controllers;

[ApiController]
[Route("api/account")]
[Authorize]
public class AccountController : ControllerBase
{
    private readonly IAccountService _accountService;

    public AccountController(IAccountService accountService)
    {
        _accountService = accountService;
    }

    [HttpGet("me")]
    public async Task<ActionResult<ProfileDto>> GetProfile(CancellationToken cancellationToken)
    {
        var profile = await _accountService.GetProfileAsync(cancellationToken);
        return Ok(profile);
    }

    [HttpPut("me")]
    public async Task<ActionResult<ProfileDto>> UpdateProfile(UpdateProfileRequest request, CancellationToken cancellationToken)
    {
        var profile = await _accountService.UpdateProfileAsync(request, cancellationToken);
        return Ok(profile);
    }

    [HttpPut("password")]
    public async Task<IActionResult> ChangePassword(ChangePasswordRequest request, CancellationToken cancellationToken)
    {
        await _accountService.ChangePasswordAsync(request, cancellationToken);
        return NoContent();
    }
}

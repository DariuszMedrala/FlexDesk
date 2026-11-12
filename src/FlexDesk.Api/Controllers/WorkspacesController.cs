using FlexDesk.Application.Common.Models;
using FlexDesk.Application.Workspaces;
using FlexDesk.Application.Workspaces.Dtos;
using FlexDesk.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FlexDesk.Api.Controllers;

[ApiController]
[Route("api/workspaces")]
public class WorkspacesController : ControllerBase
{
    private readonly IWorkspaceService _workspaceService;

    public WorkspacesController(IWorkspaceService workspaceService)
    {
        _workspaceService = workspaceService;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<PagedResult<WorkspaceDto>>> Get([FromQuery] WorkspaceQuery query, CancellationToken cancellationToken)
    {
        var workspaces = await _workspaceService.GetAsync(query, cancellationToken);
        return Ok(workspaces);
    }

    [HttpGet("{id:guid}")]
    [AllowAnonymous]
    public async Task<ActionResult<WorkspaceDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var workspace = await _workspaceService.GetByIdAsync(id, cancellationToken);
        return Ok(workspace);
    }

    [HttpPost]
    [Authorize(Roles = nameof(UserRole.Admin))]
    public async Task<ActionResult<WorkspaceDto>> Create(CreateWorkspaceRequest request, CancellationToken cancellationToken)
    {
        var created = await _workspaceService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = nameof(UserRole.Admin))]
    public async Task<ActionResult<WorkspaceDto>> Update(Guid id, UpdateWorkspaceRequest request, CancellationToken cancellationToken)
    {
        var updated = await _workspaceService.UpdateAsync(id, request, cancellationToken);
        return Ok(updated);
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = nameof(UserRole.Admin))]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _workspaceService.DeleteAsync(id, cancellationToken);
        return NoContent();
    }
}

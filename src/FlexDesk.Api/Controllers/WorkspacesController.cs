using FlexDesk.Application.Workspaces;
using FlexDesk.Application.Workspaces.Dtos;
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
    public async Task<ActionResult<IReadOnlyList<WorkspaceDto>>> GetAll(CancellationToken cancellationToken)
    {
        var workspaces = await _workspaceService.GetAllAsync(cancellationToken);
        return Ok(workspaces);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<WorkspaceDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var workspace = await _workspaceService.GetByIdAsync(id, cancellationToken);
        return Ok(workspace);
    }

    [HttpPost]
    public async Task<ActionResult<WorkspaceDto>> Create(CreateWorkspaceRequest request, CancellationToken cancellationToken)
    {
        var created = await _workspaceService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<WorkspaceDto>> Update(Guid id, UpdateWorkspaceRequest request, CancellationToken cancellationToken)
    {
        var updated = await _workspaceService.UpdateAsync(id, request, cancellationToken);
        return Ok(updated);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _workspaceService.DeleteAsync(id, cancellationToken);
        return NoContent();
    }
}

using FlexDesk.Application.Incidents;
using FlexDesk.Application.Incidents.Dtos;
using FlexDesk.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FlexDesk.Api.Controllers;

[ApiController]
[Route("api/incidents")]
[Authorize]
public class IncidentsController : ControllerBase
{
    private readonly IIncidentService _incidentService;

    public IncidentsController(IIncidentService incidentService)
    {
        _incidentService = incidentService;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<IncidentDto>>> GetAll(CancellationToken cancellationToken)
    {
        var incidents = await _incidentService.GetAllAsync(cancellationToken);
        return Ok(incidents);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<IncidentDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var incident = await _incidentService.GetByIdAsync(id, cancellationToken);
        return Ok(incident);
    }

    [HttpPost]
    public async Task<ActionResult<IncidentDto>> Create(CreateIncidentRequest request, CancellationToken cancellationToken)
    {
        var created = await _incidentService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:guid}/status")]
    [Authorize(Roles = nameof(UserRole.Admin))]
    public async Task<ActionResult<IncidentDto>> UpdateStatus(Guid id, UpdateIncidentStatusRequest request, CancellationToken cancellationToken)
    {
        var updated = await _incidentService.UpdateStatusAsync(id, request, cancellationToken);
        return Ok(updated);
    }
}

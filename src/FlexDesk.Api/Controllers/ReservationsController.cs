using FlexDesk.Application.Reservations;
using FlexDesk.Application.Reservations.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FlexDesk.Api.Controllers;

[ApiController]
[Route("api/reservations")]
[Authorize]
public class ReservationsController : ControllerBase
{
    private readonly IReservationService _reservationService;

    public ReservationsController(IReservationService reservationService)
    {
        _reservationService = reservationService;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<ReservationDto>>> GetAll(CancellationToken cancellationToken)
    {
        var reservations = await _reservationService.GetAllAsync(cancellationToken);
        return Ok(reservations);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ReservationDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var reservation = await _reservationService.GetByIdAsync(id, cancellationToken);
        return Ok(reservation);
    }

    [HttpPost]
    public async Task<ActionResult<ReservationDto>> Create(CreateReservationRequest request, CancellationToken cancellationToken)
    {
        var created = await _reservationService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPost("{id:guid}/pay")]
    public async Task<ActionResult<PaymentDto>> Pay(Guid id, PayReservationRequest request, CancellationToken cancellationToken)
    {
        var payment = await _reservationService.PayAsync(id, request, cancellationToken);
        return Ok(payment);
    }

    [HttpPost("{id:guid}/cancel")]
    public async Task<ActionResult<ReservationDto>> Cancel(Guid id, CancellationToken cancellationToken)
    {
        var cancelled = await _reservationService.CancelAsync(id, cancellationToken);
        return Ok(cancelled);
    }
}

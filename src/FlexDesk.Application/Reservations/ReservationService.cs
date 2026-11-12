using FlexDesk.Application.Common.Exceptions;
using FlexDesk.Application.Common.Persistence;
using FlexDesk.Application.Common.Security;
using FlexDesk.Application.Reservations.Dtos;
using FlexDesk.Domain.Entities;
using FlexDesk.Domain.Enums;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace FlexDesk.Application.Reservations;

public class ReservationService : IReservationService
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUser _currentUser;
    private readonly TimeProvider _timeProvider;
    private readonly IValidator<CreateReservationRequest> _createValidator;
    private readonly IValidator<PayReservationRequest> _payValidator;

    public ReservationService(
        IApplicationDbContext context,
        ICurrentUser currentUser,
        TimeProvider timeProvider,
        IValidator<CreateReservationRequest> createValidator,
        IValidator<PayReservationRequest> payValidator)
    {
        _context = context;
        _currentUser = currentUser;
        _timeProvider = timeProvider;
        _createValidator = createValidator;
        _payValidator = payValidator;
    }

    public async Task<IReadOnlyList<ReservationDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var userId = RequireUserId();

        var query = _context.Reservations.AsNoTracking();

        if (!_currentUser.IsAdmin)
        {
            query = query.Where(r => r.UserId == userId);
        }

        return await query
            .OrderByDescending(r => r.StartTime)
            .Select(r => new ReservationDto(
                r.Id,
                r.WorkspaceId,
                r.Workspace.Name,
                r.UserId,
                r.StartTime,
                r.EndTime,
                r.TotalPrice,
                r.Status,
                r.CreatedAt))
            .ToListAsync(cancellationToken);
    }

    public async Task<ReservationDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var reservation = await _context.Reservations
            .AsNoTracking()
            .Include(r => r.Workspace)
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken)
            ?? throw NotFoundException.For(nameof(Reservation), id);

        EnsureOwnerOrAdmin(reservation);

        return reservation.ToDto();
    }

    public async Task<ReservationDto> CreateAsync(CreateReservationRequest request, CancellationToken cancellationToken = default)
    {
        await _createValidator.ValidateAndThrowAsync(request, cancellationToken);

        var userId = RequireUserId();
        var startTime = AsUtc(request.StartTime);
        var endTime = AsUtc(request.EndTime);
        var now = _timeProvider.GetUtcNow().UtcDateTime;

        if (startTime <= now)
        {
            throw new BusinessRuleException("Reservation start time must be in the future.");
        }

        var workspace = await _context.Workspaces
            .FirstOrDefaultAsync(w => w.Id == request.WorkspaceId, cancellationToken)
            ?? throw NotFoundException.For(nameof(Workspace), request.WorkspaceId);

        if (!workspace.IsActive)
        {
            throw new BusinessRuleException("Workspace is not available for reservations.");
        }

        var minimumDuration = ReservationPolicy.MinimumDuration(workspace.Type);
        if (endTime - startTime < minimumDuration)
        {
            throw new BusinessRuleException(
                $"A {workspace.Type} must be reserved for at least {minimumDuration.TotalHours:0} hour(s).");
        }

        var hasConflict = await _context.Reservations.AnyAsync(
            r => r.WorkspaceId == workspace.Id
                && r.Status != ReservationStatus.Cancelled
                && r.StartTime < endTime
                && startTime < r.EndTime,
            cancellationToken);

        if (hasConflict)
        {
            throw new ConflictException("The workspace is already reserved for the selected time range.");
        }

        var reservation = new Reservation
        {
            Id = Guid.CreateVersion7(),
            UserId = userId,
            WorkspaceId = workspace.Id,
            StartTime = startTime,
            EndTime = endTime,
            TotalPrice = ReservationPricing.CalculateTotal(workspace.PricePerHour, startTime, endTime),
            Status = ReservationStatus.Pending,
            CreatedAt = now
        };

        _context.Reservations.Add(reservation);
        _context.Entry(workspace).State = EntityState.Modified;

        await _context.SaveChangesAsync(cancellationToken);

        reservation.Workspace = workspace;
        return reservation.ToDto();
    }

    public async Task<PaymentDto> PayAsync(Guid id, PayReservationRequest request, CancellationToken cancellationToken = default)
    {
        await _payValidator.ValidateAndThrowAsync(request, cancellationToken);

        var reservation = await _context.Reservations
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken)
            ?? throw NotFoundException.For(nameof(Reservation), id);

        EnsureOwnerOrAdmin(reservation);

        if (reservation.Status == ReservationStatus.Cancelled)
        {
            throw new BusinessRuleException("A cancelled reservation cannot be paid.");
        }

        if (reservation.Status == ReservationStatus.Confirmed)
        {
            throw new ConflictException("The reservation has already been paid.");
        }

        var payment = new Payment
        {
            Id = Guid.CreateVersion7(),
            ReservationId = reservation.Id,
            Amount = reservation.TotalPrice,
            PaymentDate = _timeProvider.GetUtcNow().UtcDateTime,
            Method = request.Method,
            IsSuccessful = true
        };

        reservation.Status = ReservationStatus.Confirmed;
        _context.Payments.Add(payment);

        await _context.SaveChangesAsync(cancellationToken);

        return payment.ToDto();
    }

    public async Task<ReservationDto> CancelAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var reservation = await _context.Reservations
            .Include(r => r.Workspace)
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken)
            ?? throw NotFoundException.For(nameof(Reservation), id);

        EnsureOwnerOrAdmin(reservation);

        if (reservation.Status == ReservationStatus.Cancelled)
        {
            throw new ConflictException("The reservation is already cancelled.");
        }

        if (!_currentUser.IsAdmin)
        {
            var now = _timeProvider.GetUtcNow().UtcDateTime;
            if (reservation.StartTime - now <= TimeSpan.FromHours(ReservationPolicy.CancellationCutoffHours))
            {
                throw new BusinessRuleException(
                    $"A reservation can only be cancelled more than {ReservationPolicy.CancellationCutoffHours} hours before it starts.");
            }
        }

        reservation.Status = ReservationStatus.Cancelled;
        await _context.SaveChangesAsync(cancellationToken);

        return reservation.ToDto();
    }

    private Guid RequireUserId() =>
        _currentUser.UserId ?? throw new UnauthorizedException("Authentication is required.");

    private void EnsureOwnerOrAdmin(Reservation reservation)
    {
        var userId = RequireUserId();
        if (!_currentUser.IsAdmin && reservation.UserId != userId)
        {
            throw new ForbiddenException("You do not have access to this reservation.");
        }
    }

    private static DateTime AsUtc(DateTime value) => value.Kind switch
    {
        DateTimeKind.Utc => value,
        DateTimeKind.Local => value.ToUniversalTime(),
        _ => DateTime.SpecifyKind(value, DateTimeKind.Utc)
    };
}

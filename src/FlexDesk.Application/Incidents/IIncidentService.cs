using FlexDesk.Application.Incidents.Dtos;

namespace FlexDesk.Application.Incidents;

public interface IIncidentService
{
    Task<IReadOnlyList<IncidentDto>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<IncidentDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<IncidentDto> CreateAsync(CreateIncidentRequest request, CancellationToken cancellationToken = default);

    Task<IncidentDto> UpdateStatusAsync(Guid id, UpdateIncidentStatusRequest request, CancellationToken cancellationToken = default);
}

using FlexDesk.Domain.Enums;

namespace FlexDesk.Application.Workspaces.Dtos;

public class WorkspaceQuery
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public WorkspaceType? Type { get; set; }
    public int? MinCapacity { get; set; }
    public bool? IsActive { get; set; }
    public string? Search { get; set; }
}

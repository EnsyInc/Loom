namespace EnsyInc.Loom.ServiceTests.Models;

public sealed record CreateStatusRequest(string Name, StatusCategory Category);

public sealed record UpdateStatusRequest(string Name, StatusCategory Category);

public sealed record GetStatusResponse(
    Guid Id,
    string Name,
    StatusCategory Category,
    DateTime CreatedAt,
    DateTime? UpdatedAt);

public sealed record GetStatusesResponse(IEnumerable<GetStatusResponse> Statuses);

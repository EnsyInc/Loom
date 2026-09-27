namespace EnsyInc.Loom.ServiceTests.Models;

public sealed record CreateStatusTransitionRequest(Guid FromStatusId, Guid ToStatusId);

public sealed record GetStatusTransitionResponse(
    Guid Id,
    Guid TypeId,
    Guid FromStatusId,
    Guid ToStatusId,
    DateTime CreatedAt,
    DateTime? UpdatedAt);

public sealed record GetStatusTransitionsResponse(IEnumerable<GetStatusTransitionResponse> Transitions);

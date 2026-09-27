namespace EnsyInc.Loom.ServiceTests.Models;

public sealed record CreateFieldOptionRequest(string Value, string Label, int Rank);

public sealed record UpdateFieldOptionRequest(string Label, int Rank);

public sealed record GetFieldOptionResponse(
    Guid Id,
    Guid FieldId,
    string Value,
    string Label,
    int Rank,
    DateTime CreatedAt,
    DateTime? UpdatedAt);

public sealed record GetFieldOptionsResponse(IEnumerable<GetFieldOptionResponse> Options);

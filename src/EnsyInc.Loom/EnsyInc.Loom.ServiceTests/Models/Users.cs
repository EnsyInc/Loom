namespace EnsyInc.Loom.ServiceTests.Models;

public sealed record GetUserResponse(
    Guid Id,
    string FirstName,
    string LastName,
    string Email,
    DateTime CreatedAt,
    DateTime? UpdatedAt);

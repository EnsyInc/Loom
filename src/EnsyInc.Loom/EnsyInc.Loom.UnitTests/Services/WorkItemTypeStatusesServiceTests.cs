using System.Linq.Expressions;

using EnsyInc.Loom.Core.Models;
using EnsyInc.Loom.DataAccess.Abstractions;
using EnsyInc.Loom.DataAccess.Models;
using EnsyInc.Loom.Services.Implementations;

using EnsyNet.Core.Results;
using EnsyNet.DataAccess.Abstractions.Errors;

using Moq;

namespace EnsyInc.Loom.UnitTests.Services;

// Only covers a scenario ServiceTests (black-box, against a real running instance) can't reach:
// AddStatusToType's insert racing with a concurrent add of the same link between the
// not-yet-linked pre-check and the insert itself, which must be treated as success (idempotent),
// not surfaced as an error.
public sealed class WorkItemTypeStatusesServiceTests
{
    private readonly Mock<IWorkItemTypeStatusRepo> _typeStatusRepoMock = new();
    private readonly Mock<ITplWorkItemRepo> _typeRepoMock = new();
    private readonly Mock<ITplWorkItemStatusRepo> _statusRepoMock = new();
    private readonly Mock<IStatusTransitionRepo> _transitionRepoMock = new();
    private readonly WorkItemTypeStatusesService _sut;

    public WorkItemTypeStatusesServiceTests()
    {
        _sut = new WorkItemTypeStatusesService(
            _typeStatusRepoMock.Object,
            _typeRepoMock.Object,
            _statusRepoMock.Object,
            _transitionRepoMock.Object);
    }

    [Fact]
    public async Task AddStatusToType_InsertRacesWithConcurrentAdd_ReturnsOk()
    {
        var typeId = Guid.NewGuid();
        var statusId = Guid.NewGuid();
        _typeRepoMock.Setup(r => r.GetById(typeId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok(new TplWorkItemEntity { Id = typeId, Name = "Bug" }));
        _statusRepoMock.Setup(r => r.GetById(statusId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok(new TplWorkItemStatusEntity { Id = statusId, Name = "To Do", Category = StatusCategory.ToDo }));
        _typeStatusRepoMock.Setup(r => r.GetManyByExpression(It.IsAny<Expression<Func<WorkItemTypeStatusEntity, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok(Enumerable.Empty<WorkItemTypeStatusEntity>()));
        _typeStatusRepoMock.Setup(r => r.Insert(It.IsAny<WorkItemTypeStatusEntity>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.FromError<WorkItemTypeStatusEntity>(new UniqueConstraintViolationError(new InvalidOperationException())));

        var result = await _sut.AddStatusToType(typeId, statusId, CancellationToken.None);

        Assert.False(result.HasError);
    }
}

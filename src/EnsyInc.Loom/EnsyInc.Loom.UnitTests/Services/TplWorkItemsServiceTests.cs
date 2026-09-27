using System.Linq.Expressions;

using EnsyInc.Loom.Core.Errors;
using EnsyInc.Loom.DataAccess.Abstractions;
using EnsyInc.Loom.DataAccess.Mappers;
using EnsyInc.Loom.DataAccess.Models;
using EnsyInc.Loom.Services.Implementations;

using EnsyNet.Core.Results;
using EnsyNet.DataAccess.Abstractions.Errors;
using EnsyNet.DataAccess.Abstractions.Models;

using Moq;

namespace EnsyInc.Loom.UnitTests.Services;

// Only covers scenarios ServiceTests (black-box, against a real running instance) can't reach:
// the update failing after a successful existence pre-check (the row vanishes in between), for
// both UpdateType and SetInitialStatus.
public sealed class TplWorkItemsServiceTests
{
    private readonly Mock<ITplWorkItemRepo> _typeRepoMock = new();
    private readonly Mock<IWorkItemTypeStatusRepo> _typeStatusRepoMock = new();
    private readonly Mock<IProjectWorkItemTypeRepo> _projectTypeRepoMock = new();
    private readonly Mock<IStatusTransitionRepo> _transitionRepoMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly TplWorkItemsService _sut;

    public TplWorkItemsServiceTests()
    {
        _unitOfWorkMock.Setup(u => u.RunInTransaction(It.IsAny<Func<Task<Result>>>(), It.IsAny<CancellationToken>()))
            .Returns((Func<Task<Result>> operation, CancellationToken _) => operation());

        _sut = new TplWorkItemsService(
            _typeRepoMock.Object,
            _typeStatusRepoMock.Object,
            _projectTypeRepoMock.Object,
            _transitionRepoMock.Object,
            _unitOfWorkMock.Object);
    }

    private static TplWorkItemEntity CreateEntity(Guid? id = null, string name = "Bug")
        => new()
        {
            Id = id ?? Guid.NewGuid(),
            Name = name,
        };

    [Fact]
    public async Task UpdateType_UpdateFailsAfterSuccessfulPrecheck_ReturnsTplWorkItemNotFoundError()
    {
        var entity = CreateEntity();
        var type = entity.ToCoreModel();
        _typeRepoMock.Setup(r => r.GetById(type.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok(entity));
        _typeRepoMock.Setup(r => r.Update(type.Id, It.IsAny<Action<EntityUpdates<TplWorkItemEntity>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.FromError(new UpdateOperationFailedError()));

        var result = await _sut.UpdateType(type, CancellationToken.None);

        Assert.True(result.HasError);
        Assert.IsType<TplWorkItemNotFoundError>(result.Error);
    }

    [Fact]
    public async Task SetInitialStatus_UpdateFailsAfterSuccessfulPrecheck_ReturnsTplWorkItemNotFoundError()
    {
        var entity = CreateEntity();
        var statusId = Guid.NewGuid();
        _typeRepoMock.Setup(r => r.GetById(entity.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok(entity));
        _typeStatusRepoMock.Setup(r => r.GetManyByExpression(It.IsAny<Expression<Func<WorkItemTypeStatusEntity, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok<IEnumerable<WorkItemTypeStatusEntity>>([new WorkItemTypeStatusEntity { Id = Guid.NewGuid(), TypeId = entity.Id, StatusId = statusId }]));
        _typeRepoMock.Setup(r => r.Update(entity.Id, It.IsAny<Action<EntityUpdates<TplWorkItemEntity>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.FromError(new UpdateOperationFailedError()));

        var result = await _sut.SetInitialStatus(entity.Id, statusId, CancellationToken.None);

        Assert.True(result.HasError);
        Assert.IsType<TplWorkItemNotFoundError>(result.Error);
    }
}

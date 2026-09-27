using EnsyInc.Loom.Core.Errors;
using EnsyInc.Loom.Core.Models;
using EnsyInc.Loom.DataAccess.Abstractions;
using EnsyInc.Loom.DataAccess.Mappers;
using EnsyInc.Loom.DataAccess.Models;
using EnsyInc.Loom.Services.Implementations;

using EnsyNet.Core.Results;
using EnsyNet.DataAccess.Abstractions.Errors;
using EnsyNet.DataAccess.Abstractions.Models;

using Moq;

namespace EnsyInc.Loom.UnitTests.Services;

// Only covers a scenario ServiceTests (black-box, against a real running instance) can't reach:
// the update failing after a successful existence pre-check (the row vanishes in between).
public sealed class TplWorkItemStatusesServiceTests
{
    private readonly Mock<ITplWorkItemStatusRepo> _statusRepoMock = new();
    private readonly Mock<IWorkItemTypeStatusRepo> _typeStatusRepoMock = new();
    private readonly TplWorkItemStatusesService _sut;

    public TplWorkItemStatusesServiceTests()
    {
        _sut = new TplWorkItemStatusesService(_statusRepoMock.Object, _typeStatusRepoMock.Object);
    }

    private static TplWorkItemStatusEntity CreateEntity(Guid? id = null, string name = "To Do")
        => new()
        {
            Id = id ?? Guid.NewGuid(),
            Name = name,
            Category = StatusCategory.ToDo,
        };

    [Fact]
    public async Task UpdateStatus_UpdateFailsAfterSuccessfulPrecheck_ReturnsTplWorkItemStatusNotFoundError()
    {
        var entity = CreateEntity();
        var status = entity.ToCoreModel();
        _statusRepoMock.Setup(r => r.GetById(status.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok(entity));
        _statusRepoMock.Setup(r => r.Update(status.Id, It.IsAny<Action<EntityUpdates<TplWorkItemStatusEntity>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.FromError(new UpdateOperationFailedError()));

        var result = await _sut.UpdateStatus(status, CancellationToken.None);

        Assert.True(result.HasError);
        Assert.IsType<TplWorkItemStatusNotFoundError>(result.Error);
    }
}

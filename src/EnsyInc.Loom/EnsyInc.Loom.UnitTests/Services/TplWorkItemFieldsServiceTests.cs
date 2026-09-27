using EnsyInc.Loom.Core.Errors;
using EnsyInc.Loom.Core.Models;
using EnsyInc.Loom.DataAccess.Abstractions;
using EnsyInc.Loom.DataAccess.Models;
using EnsyInc.Loom.Services.Implementations;

using EnsyNet.Core.Results;
using EnsyNet.DataAccess.Abstractions.Errors;
using EnsyNet.DataAccess.Abstractions.Models;

using Moq;

namespace EnsyInc.Loom.UnitTests.Services;

// Only covers a scenario ServiceTests (black-box, against a real running instance) can't reach:
// the update failing after a successful existence pre-check (the row vanishes in between).
public sealed class TplWorkItemFieldsServiceTests
{
    private readonly Mock<ITplWorkItemFieldRepo> _fieldRepoMock = new();
    private readonly Mock<ITplWorkItemRepo> _typeRepoMock = new();
    private readonly Mock<ITplWorkItemFieldOptionRepo> _fieldOptionRepoMock = new();
    private readonly TplWorkItemFieldsService _sut;

    public TplWorkItemFieldsServiceTests()
    {
        _sut = new TplWorkItemFieldsService(_fieldRepoMock.Object, _typeRepoMock.Object, _fieldOptionRepoMock.Object);
    }

    private static TplWorkItemFieldEntity CreateEntity(Guid? id = null)
        => new()
        {
            Id = id ?? Guid.NewGuid(),
            TypeId = Guid.NewGuid(),
            Key = "priority",
            Label = "Priority",
            DataType = WorkItemFieldDataType.Text,
            Required = false,
        };

    [Fact]
    public async Task UpdateField_UpdateFailsAfterSuccessfulPrecheck_ReturnsTplWorkItemFieldNotFoundError()
    {
        var entity = CreateEntity();
        _fieldRepoMock.Setup(r => r.GetById(entity.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok(entity));
        _fieldRepoMock.Setup(r => r.Update(entity.Id, It.IsAny<Action<EntityUpdates<TplWorkItemFieldEntity>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.FromError(new UpdateOperationFailedError()));

        var result = await _sut.UpdateField(entity.Id, "New Label", true, "42", CancellationToken.None);

        Assert.True(result.HasError);
        Assert.IsType<TplWorkItemFieldNotFoundError>(result.Error);
    }
}

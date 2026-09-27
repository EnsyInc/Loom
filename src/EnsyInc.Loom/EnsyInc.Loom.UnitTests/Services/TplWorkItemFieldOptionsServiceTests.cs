using EnsyInc.Loom.Core.Errors;
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
public sealed class TplWorkItemFieldOptionsServiceTests
{
    private readonly Mock<ITplWorkItemFieldOptionRepo> _optionRepoMock = new();
    private readonly Mock<ITplWorkItemFieldRepo> _fieldRepoMock = new();
    private readonly TplWorkItemFieldOptionsService _sut;

    public TplWorkItemFieldOptionsServiceTests()
    {
        _sut = new TplWorkItemFieldOptionsService(_optionRepoMock.Object, _fieldRepoMock.Object);
    }

    private static TplWorkItemFieldOptionEntity CreateEntity(Guid? id = null)
        => new()
        {
            Id = id ?? Guid.NewGuid(),
            FieldId = Guid.NewGuid(),
            Value = "high",
            Label = "High",
            Rank = 1,
        };

    [Fact]
    public async Task UpdateOption_UpdateFailsAfterSuccessfulPrecheck_ReturnsTplWorkItemFieldOptionNotFoundError()
    {
        var entity = CreateEntity();
        _optionRepoMock.Setup(r => r.GetById(entity.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok(entity));
        _optionRepoMock.Setup(r => r.Update(entity.Id, It.IsAny<Action<EntityUpdates<TplWorkItemFieldOptionEntity>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.FromError(new UpdateOperationFailedError()));

        var result = await _sut.UpdateOption(entity.Id, "Very High", 5, CancellationToken.None);

        Assert.True(result.HasError);
        Assert.IsType<TplWorkItemFieldOptionNotFoundError>(result.Error);
    }
}

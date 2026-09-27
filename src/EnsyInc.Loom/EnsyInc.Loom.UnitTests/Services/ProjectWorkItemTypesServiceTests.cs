using System.Linq.Expressions;

using EnsyInc.Loom.DataAccess.Abstractions;
using EnsyInc.Loom.DataAccess.Models;
using EnsyInc.Loom.Services.Implementations;

using EnsyNet.Core.Results;
using EnsyNet.DataAccess.Abstractions.Errors;

using Moq;

namespace EnsyInc.Loom.UnitTests.Services;

// Only covers a scenario ServiceTests (black-box, against a real running instance) can't reach:
// OptIn's insert racing with a concurrent opt-in of the same project/type pair between the
// not-yet-linked pre-check and the insert itself, which must be treated as success (idempotent),
// not surfaced as an error.
public sealed class ProjectWorkItemTypesServiceTests
{
    private readonly Mock<IProjectWorkItemTypeRepo> _projectTypeRepoMock = new();
    private readonly Mock<IProjectRepo> _projectRepoMock = new();
    private readonly Mock<ITplWorkItemRepo> _typeRepoMock = new();
    private readonly ProjectWorkItemTypesService _sut;

    public ProjectWorkItemTypesServiceTests()
    {
        _sut = new ProjectWorkItemTypesService(_projectTypeRepoMock.Object, _projectRepoMock.Object, _typeRepoMock.Object);
    }

    [Fact]
    public async Task OptIn_InsertRacesWithConcurrentOptIn_ReturnsOk()
    {
        var projectId = Guid.NewGuid();
        var typeId = Guid.NewGuid();
        _projectRepoMock.Setup(r => r.GetById(projectId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok(new ProjectEntity { Id = projectId, Name = "Roadmap" }));
        _typeRepoMock.Setup(r => r.GetById(typeId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok(new TplWorkItemEntity { Id = typeId, Name = "Bug" }));
        _projectTypeRepoMock.Setup(r => r.GetManyByExpression(It.IsAny<Expression<Func<ProjectWorkItemTypeEntity, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok(Enumerable.Empty<ProjectWorkItemTypeEntity>()));
        _projectTypeRepoMock.Setup(r => r.Insert(It.IsAny<ProjectWorkItemTypeEntity>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.FromError<ProjectWorkItemTypeEntity>(new UniqueConstraintViolationError(new InvalidOperationException())));

        var result = await _sut.OptIn(projectId, typeId, CancellationToken.None);

        Assert.False(result.HasError);
    }
}

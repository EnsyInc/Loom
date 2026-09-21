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

public sealed class ProjectsServiceTests
{
    private readonly Mock<IProjectRepo> _projectRepoMock = new();
    private readonly ProjectsService _sut;

    public ProjectsServiceTests()
    {
        _sut = new ProjectsService(_projectRepoMock.Object);
    }

    private static ProjectEntity CreateEntity(Guid? id = null, string name = "Roadmap")
        => new()
        {
            Id = id ?? Guid.NewGuid(),
            Name = name,
        };

    [Fact]
    public async Task UpdateProject_UpdateFailsAfterSuccessfulPrecheck_ReturnsProjectNotFoundError()
    {
        var entity = CreateEntity();
        var project = entity.ToCoreModel();
        _projectRepoMock.Setup(r => r.GetById(project.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok(entity));
        _projectRepoMock.Setup(r => r.Update(project.Id, It.IsAny<Action<EntityUpdates<ProjectEntity>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.FromError(new UpdateOperationFailedError()));

        var result = await _sut.UpdateProject(project, CancellationToken.None);

        Assert.True(result.HasError);
        Assert.IsType<ProjectNotFoundError>(result.Error);
    }
}

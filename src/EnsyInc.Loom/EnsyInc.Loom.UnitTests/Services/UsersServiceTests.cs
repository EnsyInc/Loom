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
// UpsertOnLogin's insert racing with a concurrent first login for the same entraObjectId, and the
// update failing after a successful existence pre-check (the row vanishes in between).
public sealed class UsersServiceTests
{
    private readonly Mock<IUserRepo> _userRepoMock = new();
    private readonly UsersService _sut;

    public UsersServiceTests()
    {
        _sut = new UsersService(_userRepoMock.Object);
    }

    private static UserEntity CreateEntity(Guid? id = null, string entraObjectId = "entra-object-id")
        => new()
        {
            Id = id ?? Guid.NewGuid(),
            EntraObjectId = entraObjectId,
            FirstName = "First",
            LastName = "Last",
            Email = "user@example.com",
        };

    [Fact]
    public async Task UpsertOnLogin_InsertRacesWithConcurrentFirstLogin_ReturnsWinnersUser()
    {
        var entraObjectId = "entra-object-id";
        var winner = CreateEntity(entraObjectId: entraObjectId);

        _userRepoMock.SetupSequence(r => r.GetByExpression(It.IsAny<Expression<Func<UserEntity, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.FromError<UserEntity>(new EntityNotFoundError<UserEntity>()))
            .ReturnsAsync(Result.Ok(winner));
        _userRepoMock.Setup(r => r.Insert(It.IsAny<UserEntity>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.FromError<UserEntity>(new UniqueConstraintViolationError(new InvalidOperationException())));

        var result = await _sut.UpsertOnLogin(entraObjectId, "First", "Last", "user@example.com", CancellationToken.None);

        Assert.False(result.HasError);
        Assert.Equal(winner.Id, result.Data.Id);
    }

    [Fact]
    public async Task UpsertOnLogin_UpdateFailsAfterSuccessfulGet_ReturnsUserNotFoundError()
    {
        var entity = CreateEntity();
        var user = entity.ToCoreModel();
        _userRepoMock.Setup(r => r.GetByExpression(It.IsAny<Expression<Func<UserEntity, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok(entity));
        _userRepoMock.Setup(r => r.Update(user.Id, It.IsAny<Action<EntityUpdates<UserEntity>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.FromError(new UpdateOperationFailedError()));

        var result = await _sut.UpsertOnLogin(user.EntraObjectId, "New First", "New Last", "new@example.com", CancellationToken.None);

        Assert.True(result.HasError);
        Assert.IsType<UserNotFoundError>(result.Error);
    }
}

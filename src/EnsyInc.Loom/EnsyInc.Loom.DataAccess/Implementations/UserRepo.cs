using EnsyInc.Loom.DataAccess.Abstractions;
using EnsyInc.Loom.DataAccess.Models;

using EnsyNet.DataAccess.EntityFramework;

using Microsoft.Extensions.Logging;

namespace EnsyInc.Loom.DataAccess.Implementations;

internal sealed class UserRepo : BaseRepository<UserEntity>, IUserRepo
{
    public UserRepo(LoomDbContext dbContext, ILogger<UserRepo> logger) : base(dbContext, dbContext.Users, logger) { }
}

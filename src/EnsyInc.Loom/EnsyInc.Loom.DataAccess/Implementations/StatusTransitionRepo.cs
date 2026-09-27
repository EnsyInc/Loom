using EnsyInc.Loom.DataAccess.Abstractions;
using EnsyInc.Loom.DataAccess.Models;

using EnsyNet.DataAccess.EntityFramework;

using Microsoft.Extensions.Logging;

namespace EnsyInc.Loom.DataAccess.Implementations;

internal sealed class StatusTransitionRepo : BaseRepository<StatusTransitionEntity>, IStatusTransitionRepo
{
    public StatusTransitionRepo(LoomDbContext dbContext, ILogger<StatusTransitionRepo> logger) : base(dbContext, dbContext.StatusTransitions, logger) { }
}

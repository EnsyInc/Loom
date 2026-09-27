using EnsyInc.Loom.DataAccess.Abstractions;
using EnsyInc.Loom.DataAccess.Models;

using EnsyNet.DataAccess.EntityFramework;

using Microsoft.Extensions.Logging;

namespace EnsyInc.Loom.DataAccess.Implementations;

internal sealed class TplWorkItemStatusRepo : BaseRepository<TplWorkItemStatusEntity>, ITplWorkItemStatusRepo
{
    public TplWorkItemStatusRepo(LoomDbContext dbContext, ILogger<TplWorkItemStatusRepo> logger) : base(dbContext, dbContext.TplWorkItemStatuses, logger) { }
}

using EnsyInc.Loom.DataAccess.Abstractions;
using EnsyInc.Loom.DataAccess.Models;

using EnsyNet.DataAccess.EntityFramework;

using Microsoft.Extensions.Logging;

namespace EnsyInc.Loom.DataAccess.Implementations;

internal sealed class TplWorkItemRepo : BaseRepository<TplWorkItemEntity>, ITplWorkItemRepo
{
    public TplWorkItemRepo(LoomDbContext dbContext, ILogger<TplWorkItemRepo> logger) : base(dbContext, dbContext.TplWorkItems, logger) { }
}

using EnsyInc.Loom.DataAccess.Abstractions;
using EnsyInc.Loom.DataAccess.Models;

using EnsyNet.DataAccess.EntityFramework;

using Microsoft.Extensions.Logging;

namespace EnsyInc.Loom.DataAccess.Implementations;

internal sealed class TplWorkItemFieldRepo : BaseRepository<TplWorkItemFieldEntity>, ITplWorkItemFieldRepo
{
    public TplWorkItemFieldRepo(LoomDbContext dbContext, ILogger<TplWorkItemFieldRepo> logger) : base(dbContext, dbContext.TplWorkItemFields, logger) { }
}

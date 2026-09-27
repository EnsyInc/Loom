using EnsyInc.Loom.DataAccess.Abstractions;
using EnsyInc.Loom.DataAccess.Models;

using EnsyNet.DataAccess.EntityFramework;

using Microsoft.Extensions.Logging;

namespace EnsyInc.Loom.DataAccess.Implementations;

internal sealed class TplWorkItemFieldOptionRepo : BaseRepository<TplWorkItemFieldOptionEntity>, ITplWorkItemFieldOptionRepo
{
    public TplWorkItemFieldOptionRepo(LoomDbContext dbContext, ILogger<TplWorkItemFieldOptionRepo> logger) : base(dbContext, dbContext.TplWorkItemFieldOptions, logger) { }
}

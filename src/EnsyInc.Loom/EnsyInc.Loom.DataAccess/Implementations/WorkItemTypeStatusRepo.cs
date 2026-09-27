using EnsyInc.Loom.DataAccess.Abstractions;
using EnsyInc.Loom.DataAccess.Models;

using EnsyNet.DataAccess.EntityFramework;

using Microsoft.Extensions.Logging;

namespace EnsyInc.Loom.DataAccess.Implementations;

internal sealed class WorkItemTypeStatusRepo : BaseRepository<WorkItemTypeStatusEntity>, IWorkItemTypeStatusRepo
{
    public WorkItemTypeStatusRepo(LoomDbContext dbContext, ILogger<WorkItemTypeStatusRepo> logger) : base(dbContext, dbContext.WorkItemTypeStatuses, logger) { }
}

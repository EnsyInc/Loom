using EnsyInc.Loom.DataAccess.Abstractions;
using EnsyInc.Loom.DataAccess.Models;

using EnsyNet.DataAccess.EntityFramework;

using Microsoft.Extensions.Logging;

namespace EnsyInc.Loom.DataAccess.Implementations;

internal sealed class ProjectWorkItemTypeRepo : BaseRepository<ProjectWorkItemTypeEntity>, IProjectWorkItemTypeRepo
{
    public ProjectWorkItemTypeRepo(LoomDbContext dbContext, ILogger<ProjectWorkItemTypeRepo> logger) : base(dbContext, dbContext.ProjectWorkItemTypes, logger) { }
}

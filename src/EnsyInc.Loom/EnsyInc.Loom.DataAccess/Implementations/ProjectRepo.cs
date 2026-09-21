using EnsyInc.Loom.DataAccess.Abstractions;
using EnsyInc.Loom.DataAccess.Models;

using EnsyNet.DataAccess.EntityFramework;

using Microsoft.Extensions.Logging;

namespace EnsyInc.Loom.DataAccess.Implementations;

internal sealed class ProjectRepo : BaseRepository<ProjectEntity>, IProjectRepo
{
    public ProjectRepo(LoomDbContext dbContext, ILogger<ProjectRepo> logger) : base(dbContext, dbContext.Projects, logger) { }
}

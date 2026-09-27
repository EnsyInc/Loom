using EnsyInc.Loom.Core.Models;
using EnsyInc.Loom.DataAccess.Models;

namespace EnsyInc.Loom.DataAccess.Mappers;

public static class ProjectWorkItemTypeMapper
{
    extension(ProjectWorkItemType coreModel)
    {
        public ProjectWorkItemTypeEntity ToEntityModel()
            => new()
            {
                Id = coreModel.Id,
                CreatedAt = coreModel.CreatedAt,
                UpdatedAt = coreModel.UpdatedAt,
                DeletedAt = coreModel.DeletedAt,
                ProjectId = coreModel.ProjectId,
                TypeId = coreModel.TypeId,
            };
    }
}

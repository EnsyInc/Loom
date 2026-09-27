using EnsyInc.Loom.Core.Models;
using EnsyInc.Loom.DataAccess.Models;

namespace EnsyInc.Loom.DataAccess.Mappers;

public static class WorkItemTypeStatusMapper
{
    extension(WorkItemTypeStatus coreModel)
    {
        public WorkItemTypeStatusEntity ToEntityModel()
            => new()
            {
                Id = coreModel.Id,
                CreatedAt = coreModel.CreatedAt,
                UpdatedAt = coreModel.UpdatedAt,
                DeletedAt = coreModel.DeletedAt,
                TypeId = coreModel.TypeId,
                StatusId = coreModel.StatusId,
            };
    }
}

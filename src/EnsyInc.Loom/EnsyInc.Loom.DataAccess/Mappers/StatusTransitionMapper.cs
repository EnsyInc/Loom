using System.Diagnostics.CodeAnalysis;

using EnsyInc.Loom.Core.Models;
using EnsyInc.Loom.DataAccess.Models;

namespace EnsyInc.Loom.DataAccess.Mappers;

[SuppressMessage("Naming", "CA1708:Identifiers should differ by more than case", Justification = "The compiler emits a member literally named 'extension' for each C# extension block; there's no way to rename a compiler-synthesized symbol.")]
public static class StatusTransitionMapper
{
    extension(StatusTransitionEntity entity)
    {
        public StatusTransition ToCoreModel()
            => new()
            {
                Id = entity.Id,
                CreatedAt = entity.CreatedAt,
                UpdatedAt = entity.UpdatedAt,
                DeletedAt = entity.DeletedAt,
                TypeId = entity.TypeId,
                FromStatusId = entity.FromStatusId,
                ToStatusId = entity.ToStatusId,
            };
    }

    extension(StatusTransition coreModel)
    {
        public StatusTransitionEntity ToEntityModel()
            => new()
            {
                Id = coreModel.Id,
                CreatedAt = coreModel.CreatedAt,
                UpdatedAt = coreModel.UpdatedAt,
                DeletedAt = coreModel.DeletedAt,
                TypeId = coreModel.TypeId,
                FromStatusId = coreModel.FromStatusId,
                ToStatusId = coreModel.ToStatusId,
            };
    }
}

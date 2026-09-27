using System.Diagnostics.CodeAnalysis;

using EnsyInc.Loom.Core.Models;
using EnsyInc.Loom.DataAccess.Models;

namespace EnsyInc.Loom.DataAccess.Mappers;

[SuppressMessage("Naming", "CA1708:Identifiers should differ by more than case", Justification = "The compiler emits a member literally named 'extension' for each C# extension block; there's no way to rename a compiler-synthesized symbol.")]
public static class TplWorkItemFieldOptionMapper
{
    extension(TplWorkItemFieldOptionEntity entity)
    {
        public TplWorkItemFieldOption ToCoreModel()
            => new()
            {
                Id = entity.Id,
                CreatedAt = entity.CreatedAt,
                UpdatedAt = entity.UpdatedAt,
                DeletedAt = entity.DeletedAt,
                FieldId = entity.FieldId,
                Value = entity.Value,
                Label = entity.Label,
                Rank = entity.Rank,
            };
    }

    extension(TplWorkItemFieldOption coreModel)
    {
        public TplWorkItemFieldOptionEntity ToEntityModel()
            => new()
            {
                Id = coreModel.Id,
                CreatedAt = coreModel.CreatedAt,
                UpdatedAt = coreModel.UpdatedAt,
                DeletedAt = coreModel.DeletedAt,
                FieldId = coreModel.FieldId,
                Value = coreModel.Value,
                Label = coreModel.Label,
                Rank = coreModel.Rank,
            };
    }
}

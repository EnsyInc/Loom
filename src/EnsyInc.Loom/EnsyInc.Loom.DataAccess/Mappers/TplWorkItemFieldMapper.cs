using System.Diagnostics.CodeAnalysis;

using EnsyInc.Loom.Core.Models;
using EnsyInc.Loom.DataAccess.Models;

namespace EnsyInc.Loom.DataAccess.Mappers;

[SuppressMessage("Naming", "CA1708:Identifiers should differ by more than case", Justification = "The compiler emits a member literally named 'extension' for each C# extension block; there's no way to rename a compiler-synthesized symbol.")]
public static class TplWorkItemFieldMapper
{
    extension(TplWorkItemFieldEntity entity)
    {
        public TplWorkItemField ToCoreModel()
            => new()
            {
                Id = entity.Id,
                CreatedAt = entity.CreatedAt,
                UpdatedAt = entity.UpdatedAt,
                DeletedAt = entity.DeletedAt,
                TypeId = entity.TypeId,
                Key = entity.Key,
                Label = entity.Label,
                DataType = entity.DataType,
                Required = entity.Required,
                DefaultValue = entity.DefaultValue,
            };
    }

    extension(TplWorkItemField coreModel)
    {
        public TplWorkItemFieldEntity ToEntityModel()
            => new()
            {
                Id = coreModel.Id,
                CreatedAt = coreModel.CreatedAt,
                UpdatedAt = coreModel.UpdatedAt,
                DeletedAt = coreModel.DeletedAt,
                TypeId = coreModel.TypeId,
                Key = coreModel.Key,
                Label = coreModel.Label,
                DataType = coreModel.DataType,
                Required = coreModel.Required,
                DefaultValue = coreModel.DefaultValue,
            };
    }
}

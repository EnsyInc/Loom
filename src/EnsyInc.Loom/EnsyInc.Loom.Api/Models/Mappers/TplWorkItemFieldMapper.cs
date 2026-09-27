using System.Diagnostics.CodeAnalysis;

using EnsyInc.Loom.Core.Models;

namespace EnsyInc.Loom.Api.Models.Mappers;

[SuppressMessage("Naming", "CA1708:Identifiers should differ by more than case", Justification = "The compiler emits a member literally named 'extension' for each C# extension block; there's no way to rename a compiler-synthesized symbol.")]
internal static class TplWorkItemFieldMapper
{
    extension(TplWorkItemField coreModel)
    {
        public GetWorkItemFieldResponse ToPublicModel()
            => new(
                Id: coreModel.Id,
                TypeId: coreModel.TypeId,
                Key: coreModel.Key,
                Label: coreModel.Label,
                DataType: coreModel.DataType,
                Required: coreModel.Required,
                DefaultValue: coreModel.DefaultValue,
                CreatedAt: coreModel.CreatedAt,
                UpdatedAt: coreModel.UpdatedAt);
    }

    extension(CreateWorkItemFieldRequest publicModel)
    {
        public TplWorkItemField ToCoreModel(Guid typeId)
            => new()
            {
                TypeId = typeId,
                Key = publicModel.Key,
                Label = publicModel.Label,
                DataType = publicModel.DataType,
                Required = publicModel.Required,
                DefaultValue = publicModel.DefaultValue,
            };
    }

}

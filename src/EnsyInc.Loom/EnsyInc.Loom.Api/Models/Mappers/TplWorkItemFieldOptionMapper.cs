using System.Diagnostics.CodeAnalysis;

using EnsyInc.Loom.Core.Models;

namespace EnsyInc.Loom.Api.Models.Mappers;

[SuppressMessage("Naming", "CA1708:Identifiers should differ by more than case", Justification = "The compiler emits a member literally named 'extension' for each C# extension block; there's no way to rename a compiler-synthesized symbol.")]
internal static class TplWorkItemFieldOptionMapper
{
    extension(TplWorkItemFieldOption coreModel)
    {
        public GetFieldOptionResponse ToPublicModel()
            => new(
                Id: coreModel.Id,
                FieldId: coreModel.FieldId,
                Value: coreModel.Value,
                Label: coreModel.Label,
                Rank: coreModel.Rank,
                CreatedAt: coreModel.CreatedAt,
                UpdatedAt: coreModel.UpdatedAt);
    }

    extension(CreateFieldOptionRequest publicModel)
    {
        public TplWorkItemFieldOption ToCoreModel(Guid fieldId)
            => new()
            {
                FieldId = fieldId,
                Value = publicModel.Value,
                Label = publicModel.Label,
                Rank = publicModel.Rank,
            };
    }
}

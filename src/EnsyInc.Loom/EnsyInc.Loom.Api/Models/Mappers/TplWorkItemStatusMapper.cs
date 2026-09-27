using System.Diagnostics.CodeAnalysis;

using EnsyInc.Loom.Core.Models;

namespace EnsyInc.Loom.Api.Models.Mappers;

[SuppressMessage("Naming", "CA1708:Identifiers should differ by more than case", Justification = "The compiler emits a member literally named 'extension' for each C# extension block; there's no way to rename a compiler-synthesized symbol.")]
internal static class TplWorkItemStatusMapper
{
    extension(TplWorkItemStatus coreModel)
    {
        public GetStatusResponse ToPublicModel()
            => new(
                Id: coreModel.Id,
                Name: coreModel.Name,
                Category: coreModel.Category,
                CreatedAt: coreModel.CreatedAt,
                UpdatedAt: coreModel.UpdatedAt);
    }

    extension(CreateStatusRequest publicModel)
    {
        public TplWorkItemStatus ToCoreModel()
            => new()
            {
                Name = publicModel.Name,
                Category = publicModel.Category,
            };
    }

    extension(UpdateStatusRequest publicModel)
    {
        public TplWorkItemStatus ToCoreModel(Guid id)
            => new()
            {
                Id = id,
                Name = publicModel.Name,
                Category = publicModel.Category,
            };
    }
}

using System.Diagnostics.CodeAnalysis;

using EnsyInc.Loom.Core.Models;

namespace EnsyInc.Loom.Api.Models.Mappers;

[SuppressMessage("Naming", "CA1708:Identifiers should differ by more than case", Justification = "The compiler emits a member literally named 'extension' for each C# extension block; there's no way to rename a compiler-synthesized symbol.")]
internal static class TplWorkItemMapper
{
    extension(TplWorkItem coreModel)
    {
        public GetWorkItemTypeResponse ToPublicModel()
            => new(
                Id: coreModel.Id,
                Name: coreModel.Name,
                IconUrl: coreModel.IconUrl,
                InitialStatusId: coreModel.InitialStatusId,
                CreatedAt: coreModel.CreatedAt,
                UpdatedAt: coreModel.UpdatedAt);
    }

    extension(CreateWorkItemTypeRequest publicModel)
    {
        public TplWorkItem ToCoreModel()
            => new()
            {
                Name = publicModel.Name,
                IconUrl = publicModel.IconUrl,
            };
    }

    extension(UpdateWorkItemTypeRequest publicModel)
    {
        public TplWorkItem ToCoreModel(Guid id)
            => new()
            {
                Id = id,
                Name = publicModel.Name,
                IconUrl = publicModel.IconUrl,
            };
    }
}

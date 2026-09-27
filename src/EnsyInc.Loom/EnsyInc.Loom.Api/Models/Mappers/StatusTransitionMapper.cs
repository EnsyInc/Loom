using System.Diagnostics.CodeAnalysis;

using EnsyInc.Loom.Core.Models;

namespace EnsyInc.Loom.Api.Models.Mappers;

[SuppressMessage("Naming", "CA1708:Identifiers should differ by more than case", Justification = "The compiler emits a member literally named 'extension' for each C# extension block; there's no way to rename a compiler-synthesized symbol.")]
internal static class StatusTransitionMapper
{
    extension(StatusTransition coreModel)
    {
        public GetStatusTransitionResponse ToPublicModel()
            => new(
                Id: coreModel.Id,
                TypeId: coreModel.TypeId,
                FromStatusId: coreModel.FromStatusId,
                ToStatusId: coreModel.ToStatusId,
                CreatedAt: coreModel.CreatedAt,
                UpdatedAt: coreModel.UpdatedAt);
    }

    extension(CreateStatusTransitionRequest publicModel)
    {
        public StatusTransition ToCoreModel(Guid typeId)
            => new()
            {
                TypeId = typeId,
                FromStatusId = publicModel.FromStatusId,
                ToStatusId = publicModel.ToStatusId,
            };
    }
}

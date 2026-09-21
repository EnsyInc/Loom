using System.Diagnostics.CodeAnalysis;

using EnsyInc.Loom.Core.Models;

namespace EnsyInc.Loom.Api.Models.Mappers;

[SuppressMessage("Naming", "CA1708:Identifiers should differ by more than case", Justification = "The compiler emits a member literally named 'extension' for each C# extension block; there's no way to rename a compiler-synthesized symbol.")]
internal static class ProjectMapper
{
    extension(Project coreModel)
    {
        public GetProjectResponse ToPublicModel()
            => new(
                Id: coreModel.Id,
                Name: coreModel.Name,
                CreatedAt: coreModel.CreatedAt,
                UpdatedAt: coreModel.UpdatedAt);
    }

    extension(CreateProjectRequest publicModel)
    {
        public Project ToCoreModel()
            => new()
            {
                Name = publicModel.Name,
            };
    }

    extension(UpdateProjectRequest publicModel)
    {
        public Project ToCoreModel(Guid id)
            => new()
            {
                Id = id,
                Name = publicModel.Name,
            };
    }
}

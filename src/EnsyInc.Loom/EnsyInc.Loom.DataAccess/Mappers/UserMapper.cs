using System.Diagnostics.CodeAnalysis;

using EnsyInc.Loom.Core.Models;
using EnsyInc.Loom.DataAccess.Models;

namespace EnsyInc.Loom.DataAccess.Mappers;

[SuppressMessage("Naming", "CA1708:Identifiers should differ by more than case", Justification = "The compiler emits a member literally named 'extension' for each C# extension block; there's no way to rename a compiler-synthesized symbol.")]
public static class UserMapper
{
    extension(UserEntity entity)
    {
        public User ToCoreModel()
            => new()
            {
                Id = entity.Id,
                CreatedAt = entity.CreatedAt,
                UpdatedAt = entity.UpdatedAt,
                DeletedAt = entity.DeletedAt,
                EntraObjectId = entity.EntraObjectId,
                FirstName = entity.FirstName,
                LastName = entity.LastName,
                Email = entity.Email,
            };
    }

    extension(User coreModel)
    {
        public UserEntity ToEntityModel()
            => new()
            {
                Id = coreModel.Id,
                CreatedAt = coreModel.CreatedAt,
                UpdatedAt = coreModel.UpdatedAt,
                DeletedAt = coreModel.DeletedAt,
                EntraObjectId = coreModel.EntraObjectId,
                FirstName = coreModel.FirstName,
                LastName = coreModel.LastName,
                Email = coreModel.Email,
            };
    }
}

using EnsyInc.Loom.Core.Models;

namespace EnsyInc.Loom.Api.Models.Mappers;

internal static class UserMapper
{
    extension(User coreModel)
    {
        public GetUserResponse ToPublicModel()
            => new(
                Id: coreModel.Id,
                FirstName: coreModel.FirstName,
                LastName: coreModel.LastName,
                Email: coreModel.Email,
                CreatedAt: coreModel.CreatedAt,
                UpdatedAt: coreModel.UpdatedAt);
    }
}

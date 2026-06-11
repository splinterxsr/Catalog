using Catalog.Api.Domain.Entities;

namespace Catalog.Api.Domain.Repositories
{
    public interface IUserCatalogRepository
    {
        Task<UserCatalog?> GetByIdAsync(int userId, int gameId, CancellationToken cancellationToken);
        Task CreateAsync(UserCatalog userCatalog, CancellationToken cancellationToken);
    }
}
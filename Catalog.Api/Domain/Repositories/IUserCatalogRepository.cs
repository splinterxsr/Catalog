using Catalog.Api.Domain.Entities;

namespace Catalog.Api.Domain.Repositories
{
    public interface IUserCatalogRepository
    {
        Task<IEnumerable<UserCatalog>> GetByIdAsync(int userId, CancellationToken cancellationToken);
        Task<UserCatalog?> GetByIdAsync(int userId, int gameId, CancellationToken cancellationToken);
    }
}
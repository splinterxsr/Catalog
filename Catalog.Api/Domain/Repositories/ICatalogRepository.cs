using Catalog.Api.Domain.Entities;

namespace Catalog.Api.Domain.Repositories
{
    public interface ICatalogRepository
    {
        Task<IEnumerable<UserGame>> GetByIdAsync(int userId, CancellationToken cancellationToken);
        Task<UserCatalog?> GetByIdAsync(int userId, string gameId, CancellationToken cancellationToken);
    }
}
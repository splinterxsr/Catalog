using Catalog.Api.Domain.Entities;

namespace Catalog.Api.Domain.Services
{
    public interface ICatalogService
    {
        Task<IEnumerable<UserGame>> GetByIdAsync(int userId, CancellationToken cancellationToken = default);
        Task AddToCatalogAsync(int userId, string userEmail, string gameId, decimal price, CancellationToken cancellationToken = default);
    }
}
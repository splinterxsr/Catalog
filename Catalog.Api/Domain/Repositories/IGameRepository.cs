using Catalog.Api.Domain.Entities;

namespace Catalog.Api.Domain.Repositories
{
    public interface IGameRepository
    {
        Task<IEnumerable<Game>> GetAsync(CancellationToken cancellationToken);
        Task<Game?> GetByIdAsync(string id, CancellationToken cancellationToken);
        Task<Game?> GetByNameAsync(string name, CancellationToken cancellationToken);
        Task CreateAsync(Game game, CancellationToken cancellationToken);
        Task UpdateAsync(Game game, CancellationToken cancellationToken);
        Task DeleteAsync(string id, CancellationToken cancellationToken);
    }
}
using Catalog.Api.Domain.Entities;

namespace Catalog.Api.Domain.Services
{
    public interface IGameService
    {
        Task AddAsync(Game game, CancellationToken cancellationToken);
        Task UpdateAsync(int id, string name, string description, string genre, DateOnly release, decimal price, CancellationToken cancellationToken);
        Task DeleteAsync(int id, CancellationToken cancellationToken);
    }
}
using Catalog.Api.Domain.Entities;

namespace Catalog.Api.Domain.Services
{
    public interface IGameService
    {
        Task AddAsync(Game game, CancellationToken cancellationToken);
        Task UpdateAsync(string id, string name, string description, string publisher, DateTime releaseDate, decimal price, string status, CancellationToken cancellationToken);
        Task DeleteAsync(string id, CancellationToken cancellationToken);
    }
}
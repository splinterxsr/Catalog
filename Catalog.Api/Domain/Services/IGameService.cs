using Catalog.Api.Domain.Entities;

namespace Catalog.Api.Domain.Services
{
    public interface IGameService
    {
        Task AddAsync(Game game, CancellationToken cancellationToken);
    }
}
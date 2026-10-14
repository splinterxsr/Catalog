using Catalog.Api.Models;

namespace Catalog.Api.Domain.Services
{
    public interface IGameSearchService
    {
        Task IndexOrUpdateGameAsync(GameSearchModel game);
        Task DeleteGameAsync(string id);
        Task<IEnumerable<GameSearchModel>> SearchAsync(string query, CancellationToken cancellationToken);
    }
}

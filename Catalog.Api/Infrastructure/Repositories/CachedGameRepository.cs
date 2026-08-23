using Catalog.Api.Domain.Entities;
using Catalog.Api.Domain.Repositories;
using Catalog.Api.Infrastructure.Cache;

namespace Catalog.Api.Infrastructure.Repositories
{
    public class CachedGameRepository(IGameRepository inner, IRedisCacheService cache) : IGameRepository
    {
        private readonly string _allGamesKey = "game:all";
        private readonly string _byIdPrefix = "game:id:";
        private readonly string _byNamePrefix = "game:name:";

        public async Task<IEnumerable<Game>> GetAsync(CancellationToken cancellationToken)
        {
            var cached = await cache.GetAsync<IEnumerable<Game>>(_allGamesKey);
            if (cached is not null) return cached;

            var itens = await inner.GetAsync(cancellationToken);
            if (itens.Any()) await cache.SetAsync(_allGamesKey, itens);
            return itens ?? [];
        }

        public async Task<Game?> GetByIdAsync(string id, CancellationToken cancellationToken)
        {
            var key = _byIdPrefix + id;
            var cached = await cache.GetAsync<Game?>(key);
            if (cached is not null) return cached;

            var item = await inner.GetByIdAsync(id, cancellationToken);
            if (item is not null) await cache.SetAsync(key, item);
            return item;
        }

        public async Task<Game?> GetByNameAsync(string name, CancellationToken cancellationToken)
        {
            var key = _byNamePrefix + name;
            var cached = await cache.GetAsync<Game?>(key);
            if (cached is not null) return cached;

            var item = await inner.GetByNameAsync(name, cancellationToken);
            if (item is not null) await cache.SetAsync(key, item);
            return item;
        }

        public async Task CreateAsync(Game game, CancellationToken cancellationToken)
        {
            await inner.CreateAsync(game, cancellationToken);
            await cache.RemoveAsync(_byNamePrefix + game.Name);
            await cache.RemoveAsync(_allGamesKey);
        }

        public async Task UpdateAsync(Game game, CancellationToken cancellationToken)
        {
            await inner.UpdateAsync(game, cancellationToken);
            await cache.RemoveAsync(_byIdPrefix + game.Id);
            await cache.RemoveAsync(_byNamePrefix + game.Name);
        }

        public async Task DeleteAsync(string id, CancellationToken cancellationToken)
        {
            var existing = await inner.GetByIdAsync(id, cancellationToken);
            if (existing is not null)
            {
                await cache.RemoveAsync(_byNamePrefix + existing.Name);
                await cache.RemoveAsync(_allGamesKey);
            }

            await inner.DeleteAsync(id, cancellationToken);
            await cache.RemoveAsync(_byIdPrefix + id);
        }
    }
}

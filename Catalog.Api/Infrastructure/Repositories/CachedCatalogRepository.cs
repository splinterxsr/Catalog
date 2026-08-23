using Catalog.Api.Domain.Entities;
using Catalog.Api.Domain.Repositories;
using Catalog.Api.Infrastructure.Cache;

namespace Catalog.Api.Infrastructure.Repositories
{
    public class CachedCatalogRepository(ICatalogRepository inner, IRedisCacheService cache) : ICatalogRepository
    {
        private readonly string _byUserPrefix = Environment.GetEnvironmentVariable("USER_CATALOG_PREFIX") ?? "catalog:user";

        public async Task<IEnumerable<UserGame>> GetByIdAsync(int userId, CancellationToken cancellationToken)
        {
            var key = $"{_byUserPrefix}{userId}";
            var cached = await cache.GetAsync<IEnumerable<UserGame>>(key);
            if (cached is not null) return cached;

            var itens = await inner.GetByIdAsync(userId, cancellationToken);
            if (itens.Any()) await cache.SetAsync(key, itens);
            return itens;
        }

        public async Task<UserCatalog?> GetByIdAsync(int userId, string gameId, CancellationToken cancellationToken)
        {
            var key = $"{_byUserPrefix}{userId}:game:{gameId}";
            var cached = await cache.GetAsync<UserCatalog>(key);
            if (cached is not null) return cached;

            var item = await inner.GetByIdAsync(userId, gameId, cancellationToken);
            if (item is not null) await cache.SetAsync(key, item);
            return item;
        }
    }
}
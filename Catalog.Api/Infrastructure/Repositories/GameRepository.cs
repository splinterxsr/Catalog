using Catalog.Api.Domain.Entities;
using Catalog.Api.Domain.Repositories;
using MongoDB.Driver;

namespace Catalog.Api.Infrastructure.Repositories
{
    public class GameRepository(IMongoDatabase database) : IGameRepository
    {
        private readonly string _collectionName = "games";

        public async Task<IEnumerable<Game>> GetAsync(CancellationToken cancellationToken) => await database.GetCollection<Game>(_collectionName).Find(_ => true).ToListAsync(cancellationToken);
        public async Task<Game?> GetByIdAsync(string id, CancellationToken cancellationToken) => await database.GetCollection<Game>(_collectionName).Find(g => g.Id == id).FirstOrDefaultAsync(cancellationToken);
        public async Task<Game?> GetByNameAsync(string name, CancellationToken cancellationToken) => await database.GetCollection<Game>(_collectionName).Find(g => g.Name == name).FirstOrDefaultAsync(cancellationToken);
        public async Task CreateAsync(Game game, CancellationToken cancellationToken) => await database.GetCollection<Game>(_collectionName).InsertOneAsync(game, new InsertOneOptions { }, cancellationToken);
        public async Task UpdateAsync(Game game, CancellationToken cancellationToken) => await database.GetCollection<Game>(_collectionName).ReplaceOneAsync(g => g.Id == game.Id, game, new ReplaceOptions { IsUpsert = true }, cancellationToken);
        public async Task DeleteAsync(string id, CancellationToken cancellationToken) => await database.GetCollection<Game>(_collectionName).DeleteOneAsync(g => g.Id == id, cancellationToken);
    }
}
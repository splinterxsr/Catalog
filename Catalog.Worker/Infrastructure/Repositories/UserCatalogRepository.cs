using Catalog.Worker.Domain.Entities;
using Catalog.Worker.Domain.Repositories;
using MongoDB.Driver;

namespace Catalog.Worker.Infrastructure.Repositories
{
    public class UserCatalogRepository(IMongoDatabase database) : IUserCatalogRepository
    {
        private readonly string _collectionName = "catalogs";

        public async Task CreateAsync(UserCatalog userCatalog) => await database.GetCollection<UserCatalog>(_collectionName).InsertOneAsync(userCatalog);
    }
}
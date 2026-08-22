using Catalog.Worker.Domain.Entities;
using Catalog.Worker.Domain.Repositories;
using MongoDB.Driver;

namespace Catalog.Worker.Infrastructure.Repositories
{
    public class OrderRepository(IMongoDatabase database) : IOrderRepository
    {
        private readonly string _collectionName = "orders";

        public async Task<GameOrder?> GetOrderByIdAsync(string id) => await database.GetCollection<GameOrder>(_collectionName)
            .Find(order => order.Id == id)
            .FirstOrDefaultAsync();

        public async Task UpdateAsync(GameOrder order)
        {
            await database.GetCollection<GameOrder>(_collectionName)
                .ReplaceOneAsync(o => o.Id == order.Id, order);
        }
    }
}
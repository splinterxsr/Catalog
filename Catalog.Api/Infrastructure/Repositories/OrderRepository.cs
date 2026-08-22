using Catalog.Api.Domain.Entities;
using Catalog.Api.Domain.Repositories;
using MongoDB.Driver;

namespace Catalog.Api.Infrastructure.Repositories
{
    public class OrderRepository(IMongoDatabase database) : IOrderRepository
    {
        private readonly string _collectionName = "orders";

        public async Task<GameOrder?> GetOrderByIdAsync(Guid orderId, CancellationToken cancellationToken) => await database.GetCollection<GameOrder>(_collectionName)
            .Find(order => order.Id == orderId.ToString())
            .FirstOrDefaultAsync(cancellationToken);

        public async Task AddAsync(GameOrder order, CancellationToken cancellationToken)
        {
            await database.GetCollection<GameOrder>(_collectionName).InsertOneAsync(order, cancellationToken: cancellationToken);
        }
    }
}
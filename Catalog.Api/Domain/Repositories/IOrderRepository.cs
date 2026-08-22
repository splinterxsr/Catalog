using Catalog.Api.Domain.Entities;

namespace Catalog.Api.Domain.Repositories
{
    public interface IOrderRepository
    {
        Task<GameOrder?> GetOrderByIdAsync(Guid orderId, CancellationToken cancellationToken);
        Task AddAsync(GameOrder order, CancellationToken cancellationToken);
    }
}
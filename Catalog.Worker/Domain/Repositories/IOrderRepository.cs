using Catalog.Worker.Domain.Entities;

namespace Catalog.Worker.Domain.Repositories
{
    public interface IOrderRepository
    {
        Task<GameOrder?> GetOrderByIdAsync(string id);
        Task UpdateAsync(GameOrder order);
    }
}
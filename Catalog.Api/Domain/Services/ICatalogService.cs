namespace Catalog.Api.Domain.Services
{
    public interface ICatalogService
    {
        Task AddToCatalogAsync(int userId, string userEmail, string gameId, decimal price, CancellationToken cancellationToken = default);
    }
}
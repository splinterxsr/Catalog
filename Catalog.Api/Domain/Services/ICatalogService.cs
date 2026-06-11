namespace Catalog.Api.Domain.Services
{
    public interface ICatalogService
    {
        Task AddToCatalogAsync(int userId, int gameId, decimal price, CancellationToken cancellationToken = default);
    }
}
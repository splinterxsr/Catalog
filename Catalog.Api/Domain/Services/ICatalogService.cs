namespace Catalog.Api.Domain.Services
{
    public interface ICatalogService
    {
        Task AddToCatalogAsync(int userId, string userEmail, int gameId, decimal price, CancellationToken cancellationToken = default);
    }
}
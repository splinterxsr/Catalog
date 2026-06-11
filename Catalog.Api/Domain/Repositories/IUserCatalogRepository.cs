using Catalog.Api.Domain.Entities;

namespace Catalog.Api.Domain.Repositories
{
    public interface IUserCatalogRepository
    {
        Task CreateAsync(UserCatalog userCatalog, CancellationToken cancellationToken);
    }
}
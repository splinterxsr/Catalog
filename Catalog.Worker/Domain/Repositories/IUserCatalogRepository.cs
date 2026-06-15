using Catalog.Worker.Domain.Entities;

namespace Catalog.Worker.Domain.Repositories
{
    public interface IUserCatalogRepository
    {
        Task CreateAsync(UserCatalog userCatalog);
    }
}
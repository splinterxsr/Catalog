using Catalog.Worker.Domain.Entities;
using Catalog.Worker.Domain.Repositories;
using Catalog.Worker.Infrastructure.Context;

namespace Catalog.Worker.Infrastructure.Repositories
{
    public class UserCatalogRepository : IUserCatalogRepository
    {
        private readonly AppDbContext _context;

        public UserCatalogRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task CreateAsync(UserCatalog userCatalog)
        {
            _context.UsersCatalogs.Add(userCatalog);
            await _context.SaveChangesAsync();
        }
    }
}
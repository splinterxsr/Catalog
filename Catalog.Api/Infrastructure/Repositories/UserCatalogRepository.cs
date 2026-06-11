using Catalog.Api.Domain.Entities;
using Catalog.Api.Domain.Repositories;
using Catalog.Api.Infrastructure.Context;

namespace Catalog.Api.Infrastructure.Repositories
{
    public class UserCatalogRepository : IUserCatalogRepository
    {
        private readonly AppDbContext _context;

        public UserCatalogRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task CreateAsync(UserCatalog userCatalog, CancellationToken cancellationToken)
        {
            _context.UsersCatalogs.Add(userCatalog);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
using Catalog.Api.Domain.Entities;
using Catalog.Api.Domain.Repositories;
using Catalog.Api.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

namespace Catalog.Api.Infrastructure.Repositories
{
    public class UserCatalogRepository : IUserCatalogRepository
    {
        private readonly AppDbContext _context;

        public UserCatalogRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<UserCatalog>> GetByIdAsync(int userId, CancellationToken cancellationToken) => await _context.UsersCatalogs.Include(uc => uc.Game).Where(uc => uc.UserId == userId).ToListAsync(cancellationToken);
        public async Task<UserCatalog?> GetByIdAsync(int userId, int gameId, CancellationToken cancellationToken) => await _context.UsersCatalogs.FirstOrDefaultAsync(uc => uc.UserId == userId && uc.GameId == gameId, cancellationToken);
    }
}
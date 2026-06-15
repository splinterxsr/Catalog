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

        public async Task<UserCatalog?> GetByIdAsync(int userId, int gameId, CancellationToken cancellationToken)
        {
            return await _context.UsersCatalogs.FirstOrDefaultAsync(uc => uc.UserId == userId && uc.GameId == gameId, cancellationToken);
        }
    }
}
using Catalog.Api.Domain.Entities;
using Catalog.Api.Domain.Repositories;
using Catalog.Api.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

namespace Catalog.Api.Infrastructure.Repositories
{
    public class GameRepository : IGameRepository
    {
        private readonly AppDbContext _context;

        public GameRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Game>> GetAsync(CancellationToken cancellationToken)
        {
            return await _context.Games.ToListAsync(cancellationToken);
        }

        public async Task<Game?> GetByIdAsync(int id, CancellationToken cancellationToken)
        {
            return await _context.Games.FindAsync(id, cancellationToken);
        }

        public async Task<Game?> GetByNameAsync(string name, CancellationToken cancellationToken)
        {
            return await _context.Games.FirstOrDefaultAsync(g => g.Name == name, cancellationToken);
        }

        public async Task CreateAsync(Game game, CancellationToken cancellationToken)
        {
            _context.Games.Add(game);
            await _context.SaveChangesAsync(cancellationToken);
        }        

        public async Task UpdateAsync(Game game, CancellationToken cancellationToken)
        {
            _context.Games.Update(game);
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task DeleteAsync(int id, CancellationToken cancellationToken)
        {
            var game = await GetByIdAsync(id, cancellationToken) ?? throw new Exception($"Game with id {id} not found.");

            _context.Games.Remove(game);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
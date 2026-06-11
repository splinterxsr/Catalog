using Catalog.Api.Domain.Entities;
using Catalog.Api.Domain.Repositories;
using Catalog.Api.Domain.Services;

namespace Catalog.Api.Infrastructure.Services
{
    public class GameService : IGameService
    {
        private readonly IGameRepository _repository;

        public GameService(IGameRepository repository)
        {
            _repository = repository;
        }

        public async Task AddAsync(Game game, CancellationToken cancellationToken)
        {
            var existingGame = await CheckIfExists(game, cancellationToken);

            if (existingGame)
            {
                throw new InvalidOperationException($"A game with the name '{game.Name}' already exists.");
            }

            await _repository.CreateAsync(game, cancellationToken);
        }

        private async Task<bool> CheckIfExists(Game game, CancellationToken cancellationToken)
        {
            var existingGame = await _repository.GetByNameAsync(game.Name, cancellationToken);

            return existingGame != null;
        }
    }
}

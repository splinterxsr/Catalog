using Catalog.Api.Domain.Entities;
using Catalog.Api.Domain.Repositories;
using Catalog.Api.Domain.Services;

namespace Catalog.Api.Infrastructure.Services
{
    public class GameService(IGameRepository repository) : IGameService
    {
        public async Task AddAsync(Game game, CancellationToken cancellationToken)
        {
            var existingGame = await CheckIfExists(game, cancellationToken);

            if (existingGame)
            {
                throw new InvalidOperationException($"A game with the name '{game.Name}' already exists.");
            }

            await repository.CreateAsync(game, cancellationToken);
        }

        public async Task UpdateAsync(string id, string name, string description, string publisher, DateTime releaseDate, decimal price, string status, CancellationToken cancellationToken)
        {
            var existingGame = await repository.GetByIdAsync(id, cancellationToken);

            if (existingGame is null)
            {
                throw new KeyNotFoundException($"Game with ID '{id}' not found.");
            }

            var existingGameWithName = await repository.GetByNameAsync(name, cancellationToken);

            if (existingGameWithName != null && existingGameWithName.Id != id)
            {
                throw new InvalidOperationException($"A game with the name '{name}' already exists.");
            }

            existingGame.Update(name, description, publisher, releaseDate, price, status);

            await repository.UpdateAsync(existingGame, cancellationToken);
        }

        public async Task DeleteAsync(string id, CancellationToken cancellationToken)
        {
            var existingGame = await repository.GetByIdAsync(id, cancellationToken);

            if (existingGame is null)
            {
                throw new KeyNotFoundException($"Game with ID '{id}' not found.");
            }

            await repository.DeleteAsync(id, cancellationToken);
        }

        /// <summary>
        /// Checks if a game with the same name already exists in the repository.
        /// </summary>
        private async Task<bool> CheckIfExists(Game game, CancellationToken cancellationToken)
        {
            var existingGame = await repository.GetByNameAsync(game.Name, cancellationToken);

            return existingGame != null;
        }
    }
}
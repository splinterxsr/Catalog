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

        public async Task UpdateAsync(int id, string name, string description, string genre, DateOnly release, decimal price, CancellationToken cancellationToken)
        {
            var existingGame = await _repository.GetByIdAsync(id, cancellationToken);

            if (existingGame is null)
            {
                throw new KeyNotFoundException($"Game with ID '{id}' not found.");
            }

            var existingGameWithName = await _repository.GetByNameAsync(name, cancellationToken);

            if (existingGameWithName != null && existingGameWithName.Id != id)
            {
                throw new InvalidOperationException($"A game with the name '{name}' already exists.");
            }

            existingGame.Update(name, description, genre, release, price);

            await _repository.UpdateAsync(existingGame, cancellationToken);
        }

        public async Task DeleteAsync(int id, CancellationToken cancellationToken)
        {
            var existingGame = await _repository.GetByIdAsync(id, cancellationToken);

            if (existingGame is null)
            {
                throw new KeyNotFoundException($"Game with ID '{id}' not found.");
            }

            await _repository.DeleteAsync(id, cancellationToken);
        }

        /// <summary>
        /// Checks if a game with the same name already exists in the repository.
        /// </summary>
        private async Task<bool> CheckIfExists(Game game, CancellationToken cancellationToken)
        {
            var existingGame = await _repository.GetByNameAsync(game.Name, cancellationToken);

            return existingGame != null;
        }
    }
}
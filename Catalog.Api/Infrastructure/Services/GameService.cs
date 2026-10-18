using Catalog.Api.Domain.Entities;
using Catalog.Api.Domain.Repositories;
using Catalog.Api.Domain.Services;
using Catalog.Api.Models;

namespace Catalog.Api.Infrastructure.Services
{
    public class GameService(IGameRepository repository, IGameSearchService gameSearchService) : IGameService
    {
        public async Task AddAsync(Game game, CancellationToken cancellationToken)
        {
            var existingGame = await CheckIfExists(game, cancellationToken);

            if (existingGame)
            {
                throw new InvalidOperationException($"A game with the name '{game.Name}' already exists.");
            }

            await repository.CreateAsync(game, cancellationToken);

            try
            {
                var searchModel = new GameSearchModel
                {
                    Id = game.Id,
                    Name = game.Name,
                    Description = game.Description,
                    Publisher = game.Publisher,
                    ReleaseDate = game.ReleaseDate,
                    Price = game.Price,
                    Status = game.Status
                };

                await gameSearchService.IndexOrUpdateGameAsync(searchModel);
            }
            catch (Exception ex)
            {
                await repository.DeleteAsync(game.Id!, cancellationToken);
                throw new InvalidOperationException($"Erro ao indexar jogo no Elasticsearch. O cadastro foi cancelado: {ex.Message}", ex);
            }
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

            var originalGame = existingGame.Clone();

            existingGame.Update(name, description, publisher, releaseDate, price, status);

            await repository.UpdateAsync(existingGame, cancellationToken);

            try
            {
                var searchModel = new GameSearchModel
                {
                    Id = existingGame.Id,
                    Name = existingGame.Name,
                    Description = existingGame.Description,
                    Publisher = existingGame.Publisher,
                    ReleaseDate = existingGame.ReleaseDate,
                    Price = existingGame.Price,
                    Status = existingGame.Status
                };

                await gameSearchService.IndexOrUpdateGameAsync(searchModel);
            }
            catch (Exception ex)
            {
                await repository.UpdateAsync(originalGame, cancellationToken);
                throw new InvalidOperationException($"Erro ao atualizar jogo no Elasticsearch. As alterações foram revertidas: {ex.Message}", ex);
            }
        }

        public async Task DeleteAsync(string id, CancellationToken cancellationToken)
        {
            var existingGame = await repository.GetByIdAsync(id, cancellationToken);

            if (existingGame is null)
            {
                throw new KeyNotFoundException($"Game with ID '{id}' not found.");
            }

            await repository.DeleteAsync(id, cancellationToken);

            try
            {
                await gameSearchService.DeleteGameAsync(id);
            }
            catch (Exception ex)
            {
                await repository.CreateAsync(existingGame, cancellationToken);
                throw new InvalidOperationException($"Erro ao remover jogo do Elasticsearch. A exclusão foi revertida: {ex.Message}", ex);
            }
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
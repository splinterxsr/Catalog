using Catalog.Api.Domain.Entities;
using Catalog.Api.Domain.Repositories;
using Catalog.Api.Domain.Services;
using MassTransit;

namespace Catalog.Api.Infrastructure.Services
{
    public class CatalogService : ICatalogService
    {
        private readonly IBus _bus;
        private readonly IUserCatalogRepository _userCatalogRepository;
        private readonly IGameRepository _gameRepository;

        public CatalogService(IBus bus, IUserCatalogRepository userCatalogRepository, IGameRepository gameRepository)
        {
            _bus = bus;
            _userCatalogRepository = userCatalogRepository;
            _gameRepository = gameRepository;
        }

        public async Task AddToCatalogAsync(int userId, int gameId, decimal price, CancellationToken cancellationToken)
        {
            var existingGame = await _gameRepository.GetByIdAsync(gameId, cancellationToken);

            if (existingGame is null)
            {
                throw new ArgumentException($"Game {gameId} not found.");
            }

            var gameOrder = new GameOrder(userId, gameId, price);

            await _bus.Publish(gameOrder, cancellationToken);
        }
    }
}
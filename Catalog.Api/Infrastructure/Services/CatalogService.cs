using Catalog.Api.Domain.Repositories;
using Catalog.Api.Domain.Services;
using MassTransit;
using Catalog.Api.Domain.Contracts;

namespace Catalog.Api.Infrastructure.Services
{
    public class CatalogService : ICatalogService
    {
        private readonly IBus _bus;
        private readonly IUserCatalogRepository _userCatalogRepository;
        private readonly IGameRepository _gameRepository;
        private readonly ILogger<CatalogService> _logger;

        public CatalogService(IBus bus, IUserCatalogRepository userCatalogRepository, IGameRepository gameRepository, ILogger<CatalogService> logger)
        {
            _bus = bus;
            _userCatalogRepository = userCatalogRepository;
            _gameRepository = gameRepository;
            _logger = logger;
        }

        public async Task AddToCatalogAsync(int userId, int gameId, decimal price, CancellationToken cancellationToken = default)
        {
            _ = await _gameRepository.GetByIdAsync(gameId, cancellationToken) ?? throw new ArgumentException($"Game {gameId} not found.");

            var existingGameCatalog = await _userCatalogRepository.GetByIdAsync(userId, gameId, cancellationToken);

            if (existingGameCatalog is not null)
            {
                throw new InvalidOperationException($"User {userId} already has game {gameId} in their catalog.");
            }

            _logger.LogInformation("Placing a new game order. UserId: {UserId}, GameId: {GameId}, Price: {Price}", userId, gameId, price);

            var gameOrder = new OrderPlacedEvent(userId, gameId, price);

            await _bus.Publish(gameOrder, cancellationToken);
        }
    }
}
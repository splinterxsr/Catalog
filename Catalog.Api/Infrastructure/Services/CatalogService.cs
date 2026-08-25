using Catalog.Api.Domain.Entities;
using Catalog.Api.Domain.Repositories;
using Catalog.Api.Domain.Services;
using Fcg.Contracts;
using MassTransit;
using MongoDB.Bson;

namespace Catalog.Api.Infrastructure.Services
{
    public class CatalogService(ISendEndpointProvider sendEndpointProvider, ICatalogRepository userCatalogRepository, IGameRepository gameRepository, IOrderRepository orderRepository, ILogger<CatalogService> logger, IConfiguration configuration) : ICatalogService
    {
        public async Task AddToCatalogAsync(int userId, string userEmail, string gameId, decimal price, CancellationToken cancellationToken = default)
        {
            _ = await gameRepository.GetByIdAsync(gameId, cancellationToken) ?? throw new ArgumentException($"Game {gameId} not found.");

            var existingGameCatalog = await userCatalogRepository.GetByIdAsync(userId, gameId, cancellationToken);

            if (existingGameCatalog is not null)
            {
                throw new InvalidOperationException($"User {userId} already has game {gameId} in their catalog.");
            }

            logger.LogInformation("Placing a new game order. UserId: {UserId}, GameId: {GameId}, Price: {Price}", userId, gameId, price);

            var id = ObjectId.GenerateNewId().ToString();

            var order = new GameOrder
            {
                Id = id.ToString(),
                UserId = userId,
                UserEmail = userEmail,
                GameId = gameId,
                Price = price,
                Status = "PENDING",
                OrderDate = DateTime.Now
            };

            await orderRepository.AddAsync(order, cancellationToken);

            var gameOrder = new OrderPlacedEvent(id, userId, userEmail, gameId, price);

            var queueName = configuration["ORDER_PLACED_QUEUE_NAME"] ?? "orders-placed-queue";

            var endpoint = await sendEndpointProvider.GetSendEndpoint(new Uri($"queue:{queueName}"));

            await endpoint.Send(gameOrder, cancellationToken);

            logger.LogInformation($"Evento 'OrderPlacedEvent' enviado com sucesso para a fila {queueName}!");
        }
    }
}
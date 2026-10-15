using Catalog.Api.Domain.Entities;
using Catalog.Api.Domain.Repositories;
using Catalog.Api.Domain.Services;
using Catalog.Api.Infrastructure.Clients;
using Fcg.Contracts;
using MassTransit;
using MongoDB.Bson;
using System.Security.Claims;

namespace Catalog.Api.Infrastructure.Services
{
    public class CatalogService(
        IHttpContextAccessor httpContextAccessor,
        IUserClientService userClientService,
        IPublishEndpoint publishEndpoint, 
        ICatalogRepository userCatalogRepository,
        IGameRepository gameRepository, 
        IOrderRepository orderRepository, 
        ILogger<CatalogService> logger) : ICatalogService
    {
        public async Task<IEnumerable<UserGame>> GetByIdAsync(int userId, CancellationToken cancellationToken = default)
        {
            var email = httpContextAccessor.HttpContext?.User?
                .FindFirstValue(ClaimTypes.Email) ?? string.Empty;

            var user = await userClientService.GetUserByEmailAsync(email, cancellationToken);

            return user is null
                ? throw new InvalidOperationException("Authenticated user not found.")
                : user.Email == email? await userCatalogRepository.GetByIdAsync(userId, cancellationToken) : throw new UnauthorizedAccessException("User email does not match the authenticated user.");
        }

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
            
            await publishEndpoint.Publish(gameOrder, cancellationToken);

            logger.LogInformation("Evento 'OrderPlacedEvent' enviado com sucesso para o tópico.");
        }
    }
}
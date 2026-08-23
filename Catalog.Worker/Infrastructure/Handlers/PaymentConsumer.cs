using Catalog.Worker.Domain.Entities;
using Catalog.Worker.Domain.Repositories;
using Fcg.Contracts;
using MassTransit;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace Catalog.Worker.Infrastructure.Handlers
{
    public class PaymentConsumer(IUserCatalogRepository userCatalogRepository, IOrderRepository orderRepository, IDatabase cache, ILogger<PaymentConsumer> logger) : IConsumer<PaymentProcessedEvent>
    {
        public async Task Consume(ConsumeContext<PaymentProcessedEvent> context)
        {
            var order = context.Message;

            logger.LogInformation("Payment Processed:");
            logger.LogInformation("   Transaction ID: {TransactionId}", order.TransactionId);
            logger.LogInformation("   User: {UserId}", order.UserId);
            logger.LogInformation("   Game: {GameId}", order.GameId);
            logger.LogInformation("   Status: {Status}", order.Status);

            if (order.Status == PaymentStatus.Approved)
            {
                logger.LogInformation("Payment approved succesfully!");
                logger.LogInformation("---");

                logger.LogInformation("Adding game to user catalog...");

                try
                {
                    var gameOrder = await orderRepository.GetOrderByIdAsync(context.Message.OrderId.ToString());

                    if (gameOrder is null)
                    {
                        logger.LogInformation("Game order not found!");

                        return;
                    }

                    gameOrder.Status = "APPROVED";

                    await orderRepository.UpdateAsync(gameOrder);

                    var catalog = new UserCatalog(order.GameId, order.OrderId, order.UserId);

                    await userCatalogRepository.CreateAsync(catalog);

                    // Remove cache
                    var keyPrefix = Environment.GetEnvironmentVariable("USER_CATALOG_PREFIX") ?? "catalog:user";
                    var key = $"{keyPrefix}{order.UserId}";
                    await cache.KeyDeleteAsync(keyPrefix);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "An error occurred while adding the game to the user catalog.");
                }
            }
            else
            {
                logger.LogInformation("Payment rejected!");
                logger.LogInformation("---");

                var gameOrder = await orderRepository.GetOrderByIdAsync(context.Message.OrderId.ToString());

                if (gameOrder is null)
                {
                    logger.LogInformation("Game order not found!");

                    return;
                }

                gameOrder.Status = "REJECTED";

                await orderRepository.UpdateAsync(gameOrder);
            }
        }
    }
}
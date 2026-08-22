using MassTransit;
using Microsoft.Extensions.Logging;
using Catalog.Worker.Domain.Repositories;
using Catalog.Worker.Domain.Entities;
using Fcg.Contracts;

namespace Catalog.Worker.Infrastructure.Handlers
{
    public class PaymentConsumer(IUserCatalogRepository userCatalogRepository, IOrderRepository orderRepository, ILogger<PaymentConsumer> logger) : IConsumer<PaymentProcessedEvent>
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
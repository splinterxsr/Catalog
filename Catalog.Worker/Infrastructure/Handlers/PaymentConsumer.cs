using MassTransit;
using Microsoft.Extensions.Logging;
using Catalog.Worker.Domain.Repositories;
using Catalog.Worker.Domain.Entities;
using Fcg.Contracts;

namespace Catalog.Worker.Infrastructure.Handlers
{
    public class PaymentConsumer : IConsumer<PaymentProcessedEvent>
    {
        private readonly IUserCatalogRepository _userCatalogRepository;
        private readonly ILogger<PaymentConsumer> _logger;

        public PaymentConsumer(IUserCatalogRepository userCatalogRepository, ILogger<PaymentConsumer> logger)
        {
            _userCatalogRepository = userCatalogRepository;
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<PaymentProcessedEvent> context)
        {
            var order = context.Message;

            _logger.LogInformation("Payment Processed:");
            _logger.LogInformation("   Transaction ID: {TransactionId}", order.TransactionId);
            _logger.LogInformation("   User: {UserId}", order.UserId);
            _logger.LogInformation("   Game: {GameId}", order.GameId);
            _logger.LogInformation("   Status: {Status}", order.Status);

            if (order.Status == PaymentStatus.Approved)
            {
                _logger.LogInformation("Payment approved succesfully!");
                _logger.LogInformation("---");

                _logger.LogInformation("Adding game to user catalog...");

                try
                {
                    var catalog = new UserCatalog(order.GameId, order.UserId);

                    await _userCatalogRepository.CreateAsync(catalog);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "An error occurred while adding the game to the user catalog.");
                }
            }
            else
            {
                _logger.LogInformation("Payment rejected!");
                _logger.LogInformation("---");
            }
        }
    }
}
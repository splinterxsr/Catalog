using Catalog.Worker.Domain.Entities;
using Catalog.Worker.Domain.Repositories;
using Catalog.Worker.Infrastructure.Handlers;
using Fcg.Contracts;
using FluentAssertions;
using MassTransit;
using Microsoft.Extensions.Logging;
using Moq;
using StackExchange.Redis;

namespace Catalog.Tests.Worker.Handlers
{
    public class PaymentConsumerTests
    {
        private readonly Mock<IUserCatalogRepository> _userCatalogRepositoryMock;
        private readonly Mock<IOrderRepository> _orderRepositoryMock;
        private readonly Mock<IDatabase> _cacheMock;
        private readonly Mock<ILogger<PaymentConsumer>> _loggerMock;
        private readonly PaymentConsumer _paymentConsumer;

        public PaymentConsumerTests()
        {
            _userCatalogRepositoryMock = new Mock<IUserCatalogRepository>();
            _orderRepositoryMock = new Mock<IOrderRepository>();
            _cacheMock = new Mock<IDatabase>();
            _loggerMock = new Mock<ILogger<PaymentConsumer>>();

            _paymentConsumer = new PaymentConsumer(
                _userCatalogRepositoryMock.Object,
                _orderRepositoryMock.Object,
                _cacheMock.Object,
                _loggerMock.Object
            );
        }

        [Fact]
        public async Task Consume_WhenPaymentApprovedAndOrderExists_UpdatesOrderCreatesCatalogAndClearsCache()
        {
            // Arrange
            var orderId = Guid.NewGuid();
            var paymentEvent = new PaymentProcessedEvent(
                Guid.NewGuid(),
                orderId.ToString(),
                1,
                "game-123",
                PaymentStatus.Approved
            );

            var contextMock = new Mock<ConsumeContext<PaymentProcessedEvent>>();
            contextMock.Setup(c => c.Message).Returns(paymentEvent);

            var existingOrder = new GameOrder { Id = orderId.ToString(), UserId = 1, GameId = "game-123", Price = 50m, Status = "PENDING" };

            _orderRepositoryMock.Setup(r => r.GetOrderByIdAsync(orderId.ToString()))
                .ReturnsAsync(existingOrder);

            _orderRepositoryMock.Setup(r => r.UpdateAsync(It.IsAny<GameOrder>()))
                .Returns(Task.CompletedTask);

            _userCatalogRepositoryMock.Setup(r => r.CreateAsync(It.IsAny<UserCatalog>()))
                .Returns(Task.CompletedTask);

            _cacheMock.Setup(c => c.KeyDeleteAsync(It.IsAny<RedisKey>(), It.IsAny<CommandFlags>()))
                .ReturnsAsync(true);

            // Act
            Func<Task> act = async () => await _paymentConsumer.Consume(contextMock.Object);

            // Assert
            await act.Should().NotThrowAsync();

            _orderRepositoryMock.Verify(r => r.GetOrderByIdAsync(orderId.ToString()), Times.Once);
            _orderRepositoryMock.Verify(r => r.UpdateAsync(It.Is<GameOrder>(o => o.Status == "APPROVED")), Times.Once);
            _userCatalogRepositoryMock.Verify(r => r.CreateAsync(It.IsAny<UserCatalog>()), Times.Once);
            _cacheMock.Verify(c => c.KeyDeleteAsync(It.IsAny<RedisKey>(), It.IsAny<CommandFlags>()), Times.Once);
        }

        [Fact]
        public async Task Consume_WhenPaymentApprovedButOrderNotFound_DoesNotUpdateOrCreateCharacter()
        {
            // Arrange
            var orderId = Guid.NewGuid();
            var paymentEvent = new PaymentProcessedEvent(
                Guid.NewGuid(),
                orderId.ToString(),
                1,
                "game-123",
                PaymentStatus.Approved
            );

            var contextMock = new Mock<ConsumeContext<PaymentProcessedEvent>>();
            contextMock.Setup(c => c.Message).Returns(paymentEvent);

            _orderRepositoryMock.Setup(r => r.GetOrderByIdAsync(orderId.ToString()))
                .ReturnsAsync((GameOrder?)null);

            // Act
            Func<Task> act = async () => await _paymentConsumer.Consume(contextMock.Object);

            // Assert
            await act.Should().NotThrowAsync();

            _orderRepositoryMock.Verify(r => r.GetOrderByIdAsync(orderId.ToString()), Times.Once);
            _orderRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<GameOrder>()), Times.Never);
            _userCatalogRepositoryMock.Verify(r => r.CreateAsync(It.IsAny<UserCatalog>()), Times.Never);
            _cacheMock.Verify(c => c.KeyDeleteAsync(It.IsAny<RedisKey>(), It.IsAny<CommandFlags>()), Times.Never);
        }

        [Fact]
        public async Task Consume_WhenPaymentRejected_UpdatesOrderStatusToRejected()
        {
            // Arrange
            var orderId = Guid.NewGuid();
            var paymentEvent = new PaymentProcessedEvent(
                Guid.NewGuid(),
                orderId.ToString(),
                1,
                "game-123",
                PaymentStatus.Rejected
            );

            var contextMock = new Mock<ConsumeContext<PaymentProcessedEvent>>();
            contextMock.Setup(c => c.Message).Returns(paymentEvent);

            var existingOrder = new GameOrder { Id = orderId.ToString(), UserId = 1, GameId = "game-123", Price = 50m, Status = "PENDING" };

            _orderRepositoryMock.Setup(r => r.GetOrderByIdAsync(It.IsAny<string>()))
                .ReturnsAsync(existingOrder);

            _orderRepositoryMock.Setup(r => r.UpdateAsync(It.IsAny<GameOrder>()))
                .Returns(Task.CompletedTask);

            // Act
            Func<Task> act = async () => await _paymentConsumer.Consume(contextMock.Object);

            // Assert
            await act.Should().NotThrowAsync();

            _orderRepositoryMock.Verify(r => r.UpdateAsync(It.Is<GameOrder>(o => o.Status == "REJECTED")), Times.Once);
        }
    }
}
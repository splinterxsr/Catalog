using System.Security.Claims;
using Catalog.Api.Domain.Entities;
using Catalog.Api.Domain.Repositories;
using Catalog.Api.Infrastructure.Clients;
using Catalog.Api.Infrastructure.Services;
using Fcg.Contracts;
using FluentAssertions;
using MassTransit;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;

namespace Catalog.Tests.Api.Services
{
    public class CatalogServiceTests
    {
        private readonly Mock<IHttpContextAccessor> _httpContextAccessorMock;
        private readonly Mock<IUserClientService> _userClientServiceMock;
        private readonly Mock<IPublishEndpoint> _publishEndpointMock;
        private readonly Mock<ICatalogRepository> _catalogRepositoryMock;
        private readonly Mock<IGameRepository> _gameRepositoryMock;
        private readonly Mock<IOrderRepository> _orderRepositoryMock;
        private readonly Mock<ILogger<CatalogService>> _loggerMock;
        private readonly CatalogService _catalogService;

        public CatalogServiceTests()
        {
            _httpContextAccessorMock = new Mock<IHttpContextAccessor>();
            _userClientServiceMock = new Mock<IUserClientService>();
            _publishEndpointMock = new Mock<IPublishEndpoint>();
            _catalogRepositoryMock = new Mock<ICatalogRepository>();
            _gameRepositoryMock = new Mock<IGameRepository>();
            _orderRepositoryMock = new Mock<IOrderRepository>();
            _loggerMock = new Mock<ILogger<CatalogService>>();

            _catalogService = new CatalogService(
                _httpContextAccessorMock.Object,
                _userClientServiceMock.Object,
                _publishEndpointMock.Object,
                _catalogRepositoryMock.Object,
                _gameRepositoryMock.Object,
                _orderRepositoryMock.Object,
                _loggerMock.Object
            );
        }

        [Fact]
        public async Task GetByIdAsync_WhenUserIsValidAndEmailMatches_ReturnsCatalogs()
        {
            int userId = 1;
            var email = "teste@fiap.com";
            SetupHttpContextWithEmail(email);

            var userResponse = new UserClientService.UsuarioResponse(userId, email);
            _userClientServiceMock.Setup(s => s.GetUserByEmailAsync(email, It.IsAny<CancellationToken>()))
                .ReturnsAsync(userResponse);

            var expectedCatalogs = new List<UserGame>();
            _catalogRepositoryMock.Setup(r => r.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedCatalogs);

            var result = await _catalogService.GetByIdAsync(userId, CancellationToken.None);

            result.Should().BeEquivalentTo(expectedCatalogs);
        }

        [Fact]
        public async Task GetByIdAsync_WhenUserNotFound_ThrowsInvalidOperationException()
        {
            var email = "teste@fiap.com";
            SetupHttpContextWithEmail(email);

            _userClientServiceMock.Setup(s => s.GetUserByEmailAsync(email, It.IsAny<CancellationToken>()))
                .ReturnsAsync((UserClientService.UsuarioResponse?)null);

            Func<Task> act = async () => await _catalogService.GetByIdAsync(1, CancellationToken.None);

            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("Authenticated user not found.");
        }

        [Fact]
        public async Task GetByIdAsync_WhenEmailDoesNotMatch_ThrowsUnauthorizedAccessException()
        {
            SetupHttpContextWithEmail("token@fiap.com");

            var userResponse = new UserClientService.UsuarioResponse(1, "outro@fiap.com");
            _userClientServiceMock.Setup(s => s.GetUserByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(userResponse);

            Func<Task> act = async () => await _catalogService.GetByIdAsync(1, CancellationToken.None);

            await act.Should().ThrowAsync<UnauthorizedAccessException>()
                .WithMessage("User email does not match the authenticated user.");
        }

        [Fact]
        public async Task AddToCatalogAsync_WhenValid_SavesOrderAndPublishesEvent()
        {
            int userId = 1;
            var userEmail = "teste@fiap.com";
            var gameId = "game-123";
            decimal price = 50.00m;

            var game = new Game("game-123", "GTA VI", "Desc", "Pub", DateTime.UtcNow, [], price, "ACTIVE");
            _gameRepositoryMock.Setup(r => r.GetByIdAsync(gameId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(game);

            _catalogRepositoryMock.Setup(r => r.GetByIdAsync(userId, gameId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((UserCatalog?)null);

            _orderRepositoryMock.Setup(r => r.AddAsync(It.IsAny<GameOrder>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            _publishEndpointMock.Setup(p => p.Publish(It.IsAny<OrderPlacedEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            Func<Task> act = async () => await _catalogService.AddToCatalogAsync(userId, userEmail, gameId, price, CancellationToken.None);

            await act.Should().NotThrowAsync();
            _orderRepositoryMock.Verify(r => r.AddAsync(It.IsAny<GameOrder>(), It.IsAny<CancellationToken>()), Times.Once);
            _publishEndpointMock.Verify(p => p.Publish(It.IsAny<OrderPlacedEvent>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task AddToCatalogAsync_WhenGameAlreadyInCatalog_ThrowsInvalidOperationException()
        {
            int userId = 1;
            var gameId = "game-123";

            var game = new Game("game-123", "GTA VI", "Desc", "Pub", DateTime.UtcNow, [], 79.99m, "ACTIVE");
            _gameRepositoryMock.Setup(r => r.GetByIdAsync(gameId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(game);

            var existingCatalog = Mock.Of<UserCatalog>(uc => uc.UserId == userId && uc.GameId == gameId);
            _catalogRepositoryMock.Setup(r => r.GetByIdAsync(userId, gameId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(existingCatalog);

            Func<Task> act = async () => await _catalogService.AddToCatalogAsync(userId, "teste@fiap.com", gameId, 50.00m, CancellationToken.None);

            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage($"User {userId} already has game {gameId} in their catalog.");
        }

        private void SetupHttpContextWithEmail(string email)
        {
            var claims = new List<Claim> { new(ClaimTypes.Email, email) };
            var identity = new ClaimsIdentity(claims, "TestAuth");
            var claimsPrincipal = new ClaimsPrincipal(identity);

            var httpContext = new DefaultHttpContext { User = claimsPrincipal };
            _httpContextAccessorMock.Setup(h => h.HttpContext).Returns(httpContext);
        }
    }
}
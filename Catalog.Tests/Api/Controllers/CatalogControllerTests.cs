using Catalog.Api.Controllers;
using Catalog.Api.Domain.Entities;
using Catalog.Api.Domain.Repositories;
using Catalog.Api.Domain.Services;
using Catalog.Api.Models;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace Catalog.Tests.Api.Controllers
{
    public class CatalogControllerTests
    {
        private readonly Mock<ICatalogRepository> _catalogRepositoryMock;
        private readonly Mock<ICatalogService> _catalogServiceMock;
        private readonly CatalogController _controller;

        public CatalogControllerTests()
        {
            _catalogRepositoryMock = new Mock<ICatalogRepository>();
            _catalogServiceMock = new Mock<ICatalogService>();
            _controller = new CatalogController(_catalogRepositoryMock.Object, _catalogServiceMock.Object);
        }

        [Fact]
        public async Task Get_WhenCatalogExists_ReturnsOkResultWithCatalogs()
        {
            // Arrange
            int userId = 1;
            var mockCatalogs = new List<UserGame>();

            _catalogServiceMock.Setup(s => s.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockCatalogs);

            // Act
            var result = await _controller.Get(userId, CancellationToken.None);

            // Assert
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            okResult.Value.Should().BeEquivalentTo(mockCatalogs);
        }

        [Fact]
        public async Task Post_WhenRequestIsValid_ReturnsOkResult()
        {
            // Arrange
            var request = new CatalogRequest
            {
                UserId = 1,
                UserEmail = "usuario.teste@fiap.com",
                GameId = "6ac43cd7c434256484799e1d",
                Price = 49.99m
            };

            _catalogServiceMock.Setup(s => s.AddToCatalogAsync(
                request.UserId!.Value,
                request.UserEmail!,
                request.GameId,
                request.Price!.Value,
                It.IsAny<CancellationToken>()
            )).Returns(Task.CompletedTask);

            // Act
            var result = await _controller.Post(request, CancellationToken.None);

            // Assert
            result.Should().BeOfType<OkResult>();
        }

        [Fact]
        public async Task Post_WhenServiceThrowsException_ReturnsBadRequest()
        {
            // Arrange
            var request = new CatalogRequest
            {
                UserId = 1,
                UserEmail = "usuario.teste@fiap.com",
                GameId = "6ac43cd7c434256484799e1d",
                Price = 49.99m
            };

            _catalogServiceMock.Setup(s => s.AddToCatalogAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<decimal>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new InvalidOperationException("Erro ao adicionar"));

            // Act
            var result = await _controller.Post(request, CancellationToken.None);

            // Assert
            var badRequestResult = result.Should().BeOfType<BadRequestObjectResult>().Subject;
            badRequestResult.Value.Should().Be("Erro ao adicionar");
        }

        [Fact]
        public async Task Post_WhenModelStateIsInvalid_ReturnsBadRequest()
        {
            // Arrange
            _controller.ModelState.AddModelError("UserId", "O campo UserId é obrigatório.");
            var request = new CatalogRequest();

            // Act
            var result = await _controller.Post(request, CancellationToken.None);

            // Assert
            var badRequestResult = result.Should().BeOfType<BadRequestObjectResult>().Subject;
            badRequestResult.Value.Should().BeEquivalentTo(request);
        }
    }
}
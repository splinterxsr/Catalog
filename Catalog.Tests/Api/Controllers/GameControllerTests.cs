using Catalog.Api.Controllers;
using Catalog.Api.Domain.Entities;
using Catalog.Api.Domain.Repositories;
using Catalog.Api.Domain.Services;
using Catalog.Api.Models;
using Catalog.Api.Profiles;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace Catalog.Tests.Api.Controllers
{
    public class GameControllerTests
    {
        private readonly Mock<IGameService> _serviceMock;
        private readonly Mock<IGameSearchService> _gameSearchServiceMock;
        private readonly Mock<IGameRepository> _repositoryMock;
        private readonly Mapper _mapper;
        private readonly GameController _controller;

        public GameControllerTests()
        {
            _serviceMock = new Mock<IGameService>();
            _gameSearchServiceMock = new Mock<IGameSearchService>();
            _repositoryMock = new Mock<IGameRepository>();
            _mapper = new Mapper();

            _controller = new GameController(
                _serviceMock.Object,
                _gameSearchServiceMock.Object,
                _repositoryMock.Object,
                _mapper
            );
        }

        [Fact]
        public async Task Get_AllGames_ReturnsOkWithList()
        {
            var games = new List<Game> { new("GTA VI", "Desc", "Publisher", DateTime.UtcNow, [], 79.99m) };
            _repositoryMock.Setup(r => r.GetAsync(It.IsAny<CancellationToken>())).ReturnsAsync(games);

            var result = await _controller.Get(CancellationToken.None);

            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            okResult.Value.Should().BeEquivalentTo(games);
        }

        [Fact]
        public async Task Get_ById_WhenExists_ReturnsOk()
        {
            var game = new Game("GTA VI", "Desc", "Publisher", DateTime.UtcNow, [], 79.99m);
            _repositoryMock.Setup(r => r.GetByIdAsync("1", It.IsAny<CancellationToken>())).ReturnsAsync(game);

            var result = await _controller.Get("1", CancellationToken.None);

            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            okResult.Value.Should().BeEquivalentTo(game);
        }

        [Fact]
        public async Task Get_ById_WhenNotFound_ReturnsNotFound()
        {
            _repositoryMock.Setup(r => r.GetByIdAsync("99", It.IsAny<CancellationToken>())).ReturnsAsync((Game?)null);

            var result = await _controller.Get("99", CancellationToken.None);

            result.Should().BeOfType<NotFoundResult>();
        }

        [Fact]
        public async Task Post_ValidRequest_ReturnsOk()
        {
            var request = new GameRequest
            {
                Name = "GTA VI",
                Description = "Desc",
                Publisher = "Publisher",
                ReleaseDate = DateTime.UtcNow,
                Genres = [new() { Name = "ACTION" }],
                Price = 79.99m
            };

            _serviceMock.Setup(s => s.AddAsync(It.IsAny<Game>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

            var result = await _controller.Post(request, CancellationToken.None);

            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            okResult.Value.Should().BeEquivalentTo(request);
        }

        [Fact]
        public async Task Put_ValidUpdate_ReturnsOk()
        {
            var update = new GameUpdate
            {
                Name = "Resident Evil X",
                Description = "Desc",
                Publisher = "Capcom",
                ReleaseDate = DateTime.UtcNow,
                Price = 70m,
                Status = "ACTIVE"
            };

            _serviceMock.Setup(s => s.UpdateAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<decimal>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var result = await _controller.Put("1", update, CancellationToken.None);

            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            okResult.Value.Should().BeEquivalentTo(update);
        }

        [Fact]
        public async Task Delete_ExistingId_ReturnsOk()
        {
            _serviceMock.Setup(s => s.DeleteAsync("1", It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

            var result = await _controller.Delete("1", CancellationToken.None);

            result.Should().BeOfType<OkResult>();
        }

        [Fact]
        public async Task Search_ValidQuery_ReturnsOk()
        {
            var searchResults = new List<GameSearchModel>();
            _gameSearchServiceMock.Setup(s => s.SearchAsync("GTA", It.IsAny<CancellationToken>())).ReturnsAsync(searchResults);

            var result = await _controller.Search("GTA", CancellationToken.None);

            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            okResult.Value.Should().BeEquivalentTo(searchResults);
        }

        [Fact]
        public async Task Search_EmptyQuery_ReturnsBadRequest()
        {
            var result = await _controller.Search("", CancellationToken.None);

            var badRequestResult = result.Should().BeOfType<BadRequestObjectResult>().Subject;
            badRequestResult.Value.Should().Be("O parâmetro de busca não pode ser vazio.");
        }
        [Fact]
        public async Task Post_WhenModelStateIsInvalid_ReturnsBadRequest()
        {
            _controller.ModelState.AddModelError("Name", "O campo Name é obrigatório.");
            var request = new GameRequest();

            var result = await _controller.Post(request, CancellationToken.None);

            var badRequestResult = result.Should().BeOfType<BadRequestObjectResult>().Subject;
            badRequestResult.Value.Should().BeEquivalentTo(request);
        }

        [Fact]
        public async Task Post_WhenServiceThrowsException_ReturnsBadRequest()
        {
            var request = new GameRequest
            {
                Name = "GTA VI",
                Description = "Desc",
                Publisher = "Publisher",
                ReleaseDate = DateTime.UtcNow,
                Genres = [new() { Name = "ACTION" }],
                Price = 79.99m
            };

            _serviceMock.Setup(s => s.AddAsync(It.IsAny<Game>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new InvalidOperationException("Erro ao cadastrar jogo"));

            var result = await _controller.Post(request, CancellationToken.None);

            var badRequestResult = result.Should().BeOfType<BadRequestObjectResult>().Subject;
            badRequestResult.Value.Should().Be("Erro ao cadastrar jogo");
        }

        [Fact]
        public async Task Put_WhenModelStateIsInvalid_ReturnsBadRequest()
        {
            _controller.ModelState.AddModelError("Name", "O campo Name é obrigatório.");
            var update = new GameUpdate
            {
                Name = "GTA VI",
                Description = "Desc",
                Publisher = "Rockstar"
            };

            var result = await _controller.Put("1", update, CancellationToken.None);

            var badRequestResult = result.Should().BeOfType<BadRequestObjectResult>().Subject;
            badRequestResult.Value.Should().BeEquivalentTo(update);
        }

        [Fact]
        public async Task Put_WhenServiceThrowsException_ReturnsBadRequest()
        {
            var update = new GameUpdate
            {
                Name = "Resident Evil X",
                Description = "Desc",
                Publisher = "Capcom",
                ReleaseDate = DateTime.UtcNow,
                Price = 70m,
                Status = "ACTIVE"
            };

            _serviceMock.Setup(s => s.UpdateAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<decimal>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new InvalidOperationException("Erro ao atualizar jogo"));

            var result = await _controller.Put("1", update, CancellationToken.None);

            var badRequestResult = result.Should().BeOfType<BadRequestObjectResult>().Subject;
            badRequestResult.Value.Should().Be("Erro ao atualizar jogo");
        }

        [Fact]
        public async Task Delete_WhenServiceThrowsException_ReturnsBadRequest()
        {
            _serviceMock.Setup(s => s.DeleteAsync("1", It.IsAny<CancellationToken>()))
                .ThrowsAsync(new InvalidOperationException("Erro ao excluir jogo"));

            var result = await _controller.Delete("1", CancellationToken.None);

            var badRequestResult = result.Should().BeOfType<BadRequestObjectResult>().Subject;
            badRequestResult.Value.Should().Be("Erro ao excluir jogo");
        }

        [Fact]
        public async Task Search_WhenServiceThrowsException_ReturnsBadRequest()
        {
            _gameSearchServiceMock.Setup(s => s.SearchAsync("GTA", It.IsAny<CancellationToken>()))
                .ThrowsAsync(new InvalidOperationException("Erro na busca"));

            var result = await _controller.Search("GTA", CancellationToken.None);

            var badRequestResult = result.Should().BeOfType<BadRequestObjectResult>().Subject;
            badRequestResult.Value.Should().Be("Erro na busca");
        }
    }
}
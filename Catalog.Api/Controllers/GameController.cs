using Catalog.Api.Domain.Enums;
using Catalog.Api.Domain.Repositories;
using Catalog.Api.Domain.Services;
using Catalog.Api.Infrastructure.Services;
using Catalog.Api.Models;
using Catalog.Api.Profiles;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Catalog.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class GameController : ControllerBase
    {
        private readonly IGameService _service;
        private readonly IGameSearchService _gameSearchService;
        private readonly IGameRepository _repository;
        private readonly Mapper _mapper;

        public GameController(IGameService service, IGameSearchService gameSearchService, IGameRepository repository, Mapper mapper)
        {
            _service = service;
            _gameSearchService = gameSearchService;
            _repository = repository;
            _mapper = mapper;
        }

        [HttpGet]
        [Authorize(Policy = nameof(Policy.All))]
        public async Task<IActionResult> Get(CancellationToken cancellationToken)
        {
            var games = await _repository.GetAsync(cancellationToken);

            return Ok(games);
        }

        [HttpGet]
        [Route("{id}")]
        [Authorize(Policy = nameof(Policy.All))]
        public async Task<IActionResult> Get(string id, CancellationToken cancellationToken)
        {
            var game = await _repository.GetByIdAsync(id, cancellationToken);

            if (game is null) return NotFound();

            return Ok(game);
        }

        [HttpPost]
        [Authorize(Policy = nameof(Policy.Admin))]
        public async Task<IActionResult> Post([FromBody] GameRequest request, CancellationToken cancellationToken)
        {
            if (ModelState.IsValid is false)
            {
                return BadRequest(request);
            }

            var game = _mapper.Map(request);

            try
            {
                game.Status = "ACTIVE";
                await _service.AddAsync(game, cancellationToken);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }

            return Ok(request);
        }

        [HttpPut]
        [Route("{id}")]
        [Authorize(Policy = nameof(Policy.Admin))]
        public async Task<IActionResult> Put(string id, [FromBody] GameUpdate update, CancellationToken cancellationToken)
        {
            if (ModelState.IsValid is false)
            {
                return BadRequest(update);
            }

            try
            {
                await _service.UpdateAsync(id, update.Name, update.Description, update.Publisher, update.ReleaseDate, update.Price, update.Status, cancellationToken);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }

            return Ok(update);
        }

        [HttpDelete]
        [Route("{id}")]
        [Authorize(Policy = nameof(Policy.Admin))]
        public async Task<IActionResult> Delete(string id, CancellationToken cancellationToken)
        {
            try
            {
                await _service.DeleteAsync(id, cancellationToken);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }

            return Ok();
        }

        [HttpGet]
        [Route("search")]
        [Authorize(Policy = nameof(Policy.All))]
        public async Task<IActionResult> Search([FromQuery] string q, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(q)) return BadRequest("O parâmetro de busca não pode ser vazio.");

            try
            {
                var results = await _gameSearchService.SearchAsync(q, cancellationToken);
                return Ok(results);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
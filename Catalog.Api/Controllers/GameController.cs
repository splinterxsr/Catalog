using Catalog.Api.Domain.Repositories;
using Catalog.Api.Domain.Services;
using Catalog.Api.Models;
using Catalog.Api.Profiles;
using Microsoft.AspNetCore.Mvc;

namespace Catalog.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class GameController : ControllerBase
    {
        private readonly IGameService _service;
        private readonly IGameRepository _repository;
        private readonly Mapper _mapper;

        public GameController(IGameService service, IGameRepository repository, Mapper mapper)
        {
            _service = service;
            _repository = repository;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<IActionResult> Get(CancellationToken cancellationToken)
        {
            var games = await _repository.GetAsync(cancellationToken);

            return Ok(games);
        }

        [HttpGet]
        [Route("{id}")]
        public async Task<IActionResult> Get(int id, CancellationToken cancellationToken)
        {
            var game = await _repository.GetByIdAsync(id, cancellationToken);

            if (game is null) return NotFound();

            return Ok(game);
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] GameRequest request, CancellationToken cancellationToken)
        {
            if (ModelState.IsValid is false)
            {
                return BadRequest(request);
            }

            var game = _mapper.Map(request);

            try
            {
                await _service.AddAsync(game, cancellationToken);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }

            return Ok(request);
        }

        [HttpPut]
        public async Task<IActionResult> Put([FromBody] GameUpdate update, CancellationToken cancellationToken)
        {
            if (ModelState.IsValid is false)
            {
                return BadRequest(update);
            }

            return Ok(update);
        }

        [HttpDelete]
        [Route("{id}")]
        public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
        {
            return Ok();
        }
    }
}
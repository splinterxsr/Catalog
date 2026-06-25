using Catalog.Api.Domain.Enums;
using Catalog.Api.Domain.Repositories;
using Catalog.Api.Domain.Services;
using Catalog.Api.Models;
using Catalog.Api.Profiles;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Catalog.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [Authorize(Policy = nameof(Policy.Admin))]
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
        [Route("{id}")]
        public async Task<IActionResult> Put(int id, [FromBody] GameUpdate update, CancellationToken cancellationToken)
        {
            if (ModelState.IsValid is false)
            {
                return BadRequest(update);
            }

            try
            {
                await _service.UpdateAsync(id, update.Name, update.Description, update.Genre, update.Release, update.Price, cancellationToken);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }

            return Ok(update);
        }

        [HttpDelete]
        [Route("{id}")]
        public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
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
    }
}
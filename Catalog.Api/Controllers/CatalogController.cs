using Catalog.Api.Domain.Enums;
using Catalog.Api.Domain.Repositories;
using Catalog.Api.Domain.Services;
using Catalog.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Catalog.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [Authorize(Policy = nameof(Policy.All))]
    public class CatalogController : ControllerBase
    {
        private readonly ICatalogRepository _catalogRepository;
        private readonly ICatalogService _catalogService;

        public CatalogController(ICatalogRepository catalogRepository, ICatalogService catalogService)
        {
            _catalogRepository = catalogRepository;
            _catalogService = catalogService;
        }

        [HttpGet]
        [Route("{userId}")]
        public async Task<IActionResult> Get(int userId, CancellationToken cancellationToken)
        {
            var catalogs = await _catalogRepository.GetByIdAsync(userId, cancellationToken);

            return Ok(catalogs);
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] CatalogRequest request, CancellationToken cancellationToken)
        {
            if (ModelState.IsValid is false)
            {
                return BadRequest(request);
            }

            try
            {
                await _catalogService.AddToCatalogAsync(request.UserId!.Value, request.UserEmail!, request.GameId, request.Price!.Value, cancellationToken);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }

            return Ok();
        }
    }
}
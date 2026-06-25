using Catalog.Api.Domain.Enums;
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
        private readonly ICatalogService _catalogService;

        public CatalogController(ICatalogService catalogService)
        {
            _catalogService = catalogService;
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
                await _catalogService.AddToCatalogAsync(request.UserId!.Value, request.UserEmail!, request.GameId!.Value, request.Price!.Value, cancellationToken);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }

            return Ok();
        }
    }
}
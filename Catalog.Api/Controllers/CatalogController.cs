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
    [Authorize(Policy = nameof(Policy.All))]
    public class CatalogController : ControllerBase
    {
        private readonly IUserCatalogRepository _userCatalogRepository;
        private readonly ICatalogService _catalogService;
        private readonly Mapper _mapper;

        public CatalogController(IUserCatalogRepository userCatalogRepository, ICatalogService catalogService, Mapper mapper)
        {
            _userCatalogRepository = userCatalogRepository;
            _catalogService = catalogService;
            _mapper = mapper;
        }

        [HttpGet]
        [Route("{userId}")]
        public async Task<IActionResult> Get(int userId, CancellationToken cancellationToken)
        {
            var userCatalog = await _userCatalogRepository.GetByIdAsync(userId, cancellationToken);

            return Ok(_mapper.Map(userCatalog));
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
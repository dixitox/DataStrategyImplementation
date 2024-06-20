using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.DataStrategy.Core.Models;
using Microsoft.DataStrategy.Core.Repositories.Interfaces;

namespace Microsoft.DataStrategy.Api.Controllers
{
    [Authorize(Policy = PlatformPolicies.Consumer)]
    [ApiController]
    [Route("[controller]")]
    [Produces("application/json")]
    [Consumes("application/json")]
    public class IndustriesController : CustomControllerBase
    {       
        private readonly IIndustryRepository _industryRepository;        
        private readonly ILogger<IndustriesController> _logger;

        public IndustriesController(ILogger<IndustriesController> logger, IIndustryRepository industryRepository)
        {
            _logger = logger;
            _industryRepository = industryRepository;
        }

        [Authorize(Roles = PlatformRoles.AdminOrProducer)]
        [HttpGet("")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ResponseCache(CacheProfileName = "ResponseCachingProfile")]
        public async Task<List<string>> Get()
        {
            return await _industryRepository.GetIndustriesAsync();
        }

        [Authorize(Roles = PlatformRoles.Admin)]
        [HttpPost("")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<string>> Post(string industry)
        {
            await _industryRepository.UpsertIndustryAsync(industry);
            return Ok();
        }

        [Authorize(Roles = PlatformRoles.Admin)]
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<Asset>> Put(string industry)
        {
            await _industryRepository.UpsertIndustryAsync(industry);
            return Ok();
        }


        [Authorize(Roles = PlatformRoles.Admin)]
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<Asset>> Delete(string id)
        {
            var tag = await _industryRepository.GetIndustryByIdAsync(id);
            if (tag == null)
                return NotFound();

            await _industryRepository.DeleteIndustryAsync(id);
            return Ok();
        }
    }
}
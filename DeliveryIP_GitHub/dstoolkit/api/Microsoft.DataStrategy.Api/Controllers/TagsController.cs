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
    public class TagsController : ControllerBase
    {
        private readonly ITagRepository _tagRepository;
        private readonly ILogger<TagsController> _logger;

        public TagsController(ILogger<TagsController> logger, ITagRepository tagRepository)
        {
            _logger = logger;
            _tagRepository = tagRepository;
        }

        [Authorize(Roles = PlatformRoles.AdminOrProducer)]
        [HttpGet("")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ResponseCache(CacheProfileName = "ResponseCachingProfile")]
        public async Task<List<string>> Get()
        {
            return await _tagRepository.GetTagsAsync();
        }

        [Authorize(Roles = PlatformRoles.Admin)]
        [HttpPost("")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<string>> Post(string tag)
        {
            await _tagRepository.UpsertTagAsync(tag);
            return Ok();
        }

        [Authorize(Roles = PlatformRoles.Admin)]
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<Asset>> Put(string tag)
        {
            await _tagRepository.UpsertTagAsync(tag);
            return Ok();
        }


        [Authorize(Roles = PlatformRoles.Admin)]
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<Asset>> Delete(string id)
        {
            var tag = await _tagRepository.GetTagByIdAsync(id);
            if (tag == null)
                return NotFound();

            await _tagRepository.DeleteTagAsync(id);
            return Ok();
        }
    }
}
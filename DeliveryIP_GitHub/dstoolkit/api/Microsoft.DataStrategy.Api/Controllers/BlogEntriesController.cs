using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.DataStrategy.Api.Models;
using Microsoft.DataStrategy.Core.Models;
using Microsoft.DataStrategy.Core.Services.Interfaces;
using System.ComponentModel.DataAnnotations;

namespace Microsoft.DataStrategy.Api.Controllers
{
    [Authorize(Policy = PlatformPolicies.Consumer)]
    [ApiController]
    [Route("[controller]")]
    [Produces("application/json")]
    [Consumes("application/json")]
    public class BlogEntriesController : CustomControllerBase
    {
        private readonly ILogger<AssetsController> _logger;
        private readonly IBlogEntriesService _blogEntriesService;
        private readonly IMapper _mapper;        

        public BlogEntriesController(ILogger<AssetsController> logger, IBlogEntriesService blogEntriesService, IMapper mapper)
        {
            _logger = logger;
            _blogEntriesService = blogEntriesService;
            _mapper = mapper;
        }
        
        [HttpGet("")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]        
        public async Task<ActionResult<PagedResult<AssetBase>>> Get(bool? onlyPublished,
            [Range(1, 100)] int pageSize = 20,
            [Range(1, 50)] int page = 1, string orderBy = "createdBy['on']", bool descending = true)
        {            
            if (!User.IsInRole(PlatformRoles.Admin))
                onlyPublished = true;

            var searchResults = await _blogEntriesService.GetBlogEntriesBaseAsync(pageSize, page, onlyPublished, orderBy, descending);
            return Ok(searchResults);
        }        

        //[HttpPost("Search")]
        //[ProducesResponseType(StatusCodes.Status200OK)]
        //[ProducesResponseType(StatusCodes.Status401Unauthorized)]
        //[ResponseCache(CacheProfileName = "ResponseCachingProfile")]
        //public async Task<ActionResult<PagedResult<AssetBase>>> Search(SearchModel searchModel)
        //{
        //    var searchResults = await _blogEntriesService.SearchAssetsAsync(searchModel.PageSize, searchModel.Page, searchModel.Search, searchModel.Filters, searchModel.SortBy);            
        //    return Ok(searchResults);
        //}

        //[HttpPost("Facets")]
        //[ProducesResponseType(StatusCodes.Status200OK)]
        //[ProducesResponseType(StatusCodes.Status401Unauthorized)]
        //[ResponseCache(CacheProfileName = "ResponseCachingProfile")]
        //public async Task<ActionResult<IEnumerable<SearchFacet>>> Facets()
        //{
        //    var searchResults = await _assetSearchService.GetAllFacetsAsync();
        //    return Ok(searchResults);
        //}        

        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ResponseCache(CacheProfileName = "ResponseCachingProfile")]
        public async Task<ActionResult<BlogEntry>> Get(string id)
        {
            var asset = await _blogEntriesService.GetBlogEntryAsync(id);

            if (asset == null)
                return NotFound();
            
            if (asset.Status != BlogEntryStatus.Published && !IsUserAuthorizedForAsset(asset.CreatedBy))
               return Forbid();
                        
            return Ok(asset);
        }



        [Authorize(Roles = PlatformRoles.Admin)]
        [HttpPost("")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<string>> Post(CreateBlogEntryModel entryRequest)
        {
            var entry = _mapper.Map<BlogEntry>(entryRequest);
            entry.CreatedBy = GetUserActionMetadata();
            entry.LastChangedBy = GetUserActionMetadata();
            entry.Id = Guid.NewGuid().ToString();
            entry.PartitionKey = DateTime.Now.Year.ToString();
            await _blogEntriesService.CreateBlogEntryAsync(entry);
            return Ok();
        }       

        [Authorize(Roles = PlatformRoles.Admin)]
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]       
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<Asset>> Put(CreateBlogEntryModel entryRequest, string id)
        {
            var originalEntry = await _blogEntriesService.GetBlogEntryAsync(id);

            if (originalEntry == null)
                return NotFound();
            
            var user = GetUserActionMetadata();

            originalEntry.LastChangedBy = user;            
            _mapper.Map(entryRequest, originalEntry);
            
            await _blogEntriesService.UpdateBlogEntryAsync(originalEntry);
            return Ok();
        }


        [Authorize(Roles = PlatformRoles.Admin)]
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<Asset>> Delete(string id)
        {
            var entry = await _blogEntriesService.GetBlogEntryAsync(id);

            if (entry == null)
                return NotFound();

            await _blogEntriesService.DeleteBlogEntryAsync(id, GetUserActionMetadata());
            return Ok();
        }

        [Authorize(Roles = PlatformRoles.Admin)]
        [HttpPost("Images")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<string>> PostImage(ImageModel imageModel)
        {
            var path = await _blogEntriesService.CreateBlogEntryImageAsync(imageModel.Base64);
            return path;
        }

        [Authorize(Roles = PlatformRoles.Admin)]
        [HttpGet("Images")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<List<string>>> GetImages()
        {
            var images = await _blogEntriesService.GetBlogEntryImagesAsync();
            return images;
        }

        [Authorize(Roles = PlatformRoles.Admin)]
        [HttpDelete("Images/{name}")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> DeleteImages(string name)
        {
            await _blogEntriesService.DeleteBlogEntryImageAsync(name);
            return Ok();
        }

    }
}
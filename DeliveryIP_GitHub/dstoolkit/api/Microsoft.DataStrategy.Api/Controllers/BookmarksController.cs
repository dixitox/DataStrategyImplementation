using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.DataStrategy.Core.Models;
using Microsoft.DataStrategy.Core.Services.Interfaces;
using Microsoft.Identity.Web;

namespace Microsoft.DataStrategy.Api.Controllers
{
    [Authorize(Policy = PlatformPolicies.Consumer)]
    [ApiController]
    [Route("[controller]")]
    [Produces("application/json")]
    [Consumes("application/json")]
    public class BookmarksController : CustomControllerBase
    {
        private readonly IBookmarksService _bookmarksService;
        private readonly IAssetSearchService _assetSearchService;
        private readonly ILogger<BookmarksController> _logger;

        public BookmarksController(ILogger<BookmarksController> logger, IBookmarksService bookmarkService, IAssetSearchService assetSearch)
        {
            _logger = logger;
            _bookmarksService = bookmarkService;
            _assetSearchService = assetSearch;
        }

        [Authorize]
        [HttpGet("")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<List<string>>> Get()
        {
            return await _bookmarksService.GetUserBookmarksAsync(User.GetObjectId());
        }

        [Authorize]
        [HttpGet("/Search")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<PagedResult<AssetBase>>> Search(SortBy sortBy)
        {
            var bookmarks = await _bookmarksService.GetUserBookmarksAsync(User.GetObjectId());
            if (bookmarks == null || !bookmarks.Any())
                return new PagedResult<AssetBase> { TotalResults = 0 };
            var filters = new SearchFilters() { AssetIds = bookmarks };
            var searchResults = await _assetSearchService.SearchAssetsAsync(100, 1, "*", filters, sortBy);
            return searchResults;
        }

        [Authorize]
        [HttpPost("{assetId}")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> Post(string assetId)
        {
            await _bookmarksService.AddBookmarkAsync(User.GetObjectId(), assetId);
            return Ok();
        }
        
        [Authorize]
        [HttpDelete("{assetId}")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<string>> Delete(string assetId)
        {
            await _bookmarksService.RemoveBookmarkAsync(User.GetObjectId(), assetId);
            return Ok();
        }

    }
}
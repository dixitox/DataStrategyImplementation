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
    public class AssetsController : CustomControllerBase
    {
        private readonly ILogger<AssetsController> _logger;
        private readonly IAssetsService _assetService;
        private readonly IAssetSearchService _assetSearchService;
        private readonly IBookmarksService _bookmarkService;
        private readonly IMapper _mapper;

        public AssetsController(ILogger<AssetsController> logger, IAssetsService assetService, IBookmarksService bookmarksService, IAssetSearchService assetSearchService, IMapper mapper)
        {
            _logger = logger;
            _assetService = assetService;
            _assetSearchService = assetSearchService;
            _bookmarkService = bookmarksService;
            _mapper = mapper;
        }

        [HttpGet("")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<PagedResult<AssetBase>>> Get(bool? enabled,
            [Range(1, 100)] int pageSize = 20,
            [Range(1, 50)] int page = 1, string orderBy = "createdBy['on']", bool descending = true)
        {
            if (!User.IsInRole(PlatformRoles.Admin))
                enabled = true;

            var searchResults = await _assetService.GetAssetsBaseAsync(pageSize, page, enabled, orderBy, descending);
            return Ok(searchResults);
        }

        [HttpPost("Search")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ResponseCache(CacheProfileName = "ResponseCachingProfile")]
        public async Task<ActionResult<PagedResult<AssetBase>>> Search(SearchModel searchModel)
        {
            var searchResults = await _assetSearchService.SearchAssetsAsync(searchModel.PageSize, searchModel.Page, searchModel.Search, searchModel.Filters, searchModel.SortBy);
            return Ok(searchResults);
        }

        [HttpPost("Facets")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ResponseCache(CacheProfileName = "ResponseCachingProfile")]
        public async Task<ActionResult<IEnumerable<SearchFacet>>> Facets()
        {
            var searchResults = await _assetSearchService.GetAllFacetsAsync();
            return Ok(searchResults);
        }

        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ResponseCache(CacheProfileName = "ResponseCachingProfile")]
        public async Task<ActionResult<Asset>> Get(string id)
        {
            var asset = await _assetService.GetAssetAsync(id);

            if (asset == null)
                return NotFound();

            if (!asset.Enabled && !IsUserAuthorizedForAsset(asset.CreatedBy))
                return Forbid();

            return Ok(asset);
        }





        [Authorize(Roles = PlatformRoles.AdminOrProducer)]
        [HttpGet("Administrables")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<PagedResult<AssetBase>>> GetAdministrables(bool? enabled,
            [Range(1, 100)] int pageSize = 20,
            [Range(1, 50)] int page = 1, string orderBy = "createdBy['on']", bool descending = true)
        {
            var filterByUser = string.Empty;

            if (!User.IsInRole(PlatformRoles.Admin))
                filterByUser = GetUserActionMetadata().ObjectId;

            var searchResults = await _assetService.GetAssetsBaseAsync(pageSize, page, enabled, orderBy, descending, filterByUser);
            return Ok(searchResults);
        }

        [Authorize(Roles = PlatformRoles.AdminOrProducer)]
        [HttpPost("")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<string>> Post(CreateAssetModel createAssetRequest)
        {
            var asset = _mapper.Map<Asset>(createAssetRequest);
            asset.CreatedBy = GetUserActionMetadata();
            asset.LastChangedBy = GetUserActionMetadata();
            asset.Id = Guid.NewGuid().ToString();
            await _assetService.CreateAssetAsync(asset);
            return Ok();
        }

        [Authorize(Roles = PlatformRoles.AdminOrProducer)]
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<Asset>> Put(UpdateAssetModel updateAssetModel, string id)
        {
            var asset = await _assetService.GetAssetAsync(id);

            if (asset == null)
                return NotFound();

            if (!IsUserAuthorizedForAsset(asset.CreatedBy))
                return Forbid();

            var user = GetUserActionMetadata();
            asset.LastChangedBy = user;

            if (!asset.Enabled && updateAssetModel.Enabled && !User.IsInRole(PlatformRoles.Admin))
                updateAssetModel.Enabled = false;

            if (!asset.Enabled && updateAssetModel.Enabled)
                asset.ReleasedBy = asset.LastChangedBy;

            _mapper.Map(updateAssetModel, asset);

            await _assetService.UpdateAssetAsync(asset);
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
            var asset = await _assetService.GetAssetAsync(id);
            if (asset == null)
                return NotFound();

            await _assetSearchService.DeleteAssetAsync(id);
            await _assetService.DeleteAssetAsync(asset.Id, GetUserActionMetadata());
            return Ok();
        }
    }
}
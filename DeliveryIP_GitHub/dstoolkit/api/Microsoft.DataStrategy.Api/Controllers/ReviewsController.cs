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
    public class ReviewsController : CustomControllerBase
    {

        private readonly ILogger<AssetsController> _logger;
        private readonly IReviewsService _reviewsService;        

        public ReviewsController(ILogger<AssetsController> logger, IReviewsService reviewsService)
        {
            _logger = logger;
            _reviewsService = reviewsService;
        }

        [HttpGet("/assets/{assetId}/reviews")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ResponseCache(CacheProfileName = "ResponseCachingProfile")]
        public async Task<ActionResult<PagedResult<AssetReview>>> GetAssetReviews(string assetId, [Range(1, 100)] int pageSize = 20, [Range(1, 50)] int page = 1)
        {
            var reviews = await _reviewsService.GetAssetReviewsAsync(assetId, pageSize, page);
            return Ok(reviews);
        }

        [Authorize(Roles = PlatformRoles.Admin)]
        [HttpGet("pendingreviews")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<PendingAssetReview>> GetPendingAssetReviews([Range(1, 100)] int pageSize = 20, [Range(1, 50)] int page = 1)
        {
            var pendingReviews = await _reviewsService.GetPendingReviewsAsync(pageSize, page);
            return Ok(pendingReviews);
        }

        [Authorize(Roles = PlatformRoles.AdminOrProducer)]
        [HttpPost("/assets/{assetId}/reviews")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult> PostAssetReview(SubmitReviewModel reviewModel)
        {
            var pendingReview = new PendingAssetReview(reviewModel.AssetId, reviewModel.Review);            
            await _reviewsService.AddNewReviewAsync(pendingReview, GetUserActionMetadata());            
            return Ok();
        }

        [Authorize(Roles = PlatformRoles.Admin)]
        [HttpPost("/assets/{assetId}/reviews/{reviewId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult> ApprovePendingReview(string assetId, string reviewId)
        {
            await _reviewsService.ApprovePendingReviewAsync(reviewId, GetUserActionMetadata());
            return Ok();
        }

        [Authorize(Roles = PlatformRoles.Admin)]
        [HttpDelete("/assets/{assetId}/reviews/{reviewId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult> RejectPendingReview(string assetId, string reviewId)
        {
            await _reviewsService.RejectPendingReviewAsync(reviewId, GetUserActionMetadata());
            return Ok();
        }
    }
}
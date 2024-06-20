using AutoMapper;
using Azure.Identity;
using Azure.Messaging.ServiceBus;
using Microsoft.DataStrategy.Core.Models;
using Microsoft.DataStrategy.Core.Models.ServiceBusModels;
using Microsoft.DataStrategy.Core.Repositories.Interfaces;
using Microsoft.DataStrategy.Core.Services.Interfaces;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace Microsoft.DataStrategy.Core.Services
{
    public class ReviewsService : IReviewsService
    {
        private readonly IPendingReviewsRepository _pendingReviewsRepository;
        private readonly IAssetReviewsRepository _reviewsRepository;
        private readonly IAssetsRepository _assetsRepository;
        private readonly ServiceBusSender _serviceBusMailSender;
        private readonly ServiceBusSender _serviceBusTeamNotificationSender;
        private readonly IMapper _mapper;
        private readonly string _appUrl;
        private ILogger<ReviewsService> _logger;

        public ReviewsService(IPendingReviewsRepository pendingReviewsRepository, IAssetReviewsRepository reviewsRepository, IAssetsRepository assetsRepository, IMapper mapper, ILogger<ReviewsService> logger, AppConfig config)
        {
            var serviceBusClient = new ServiceBusClient(config.ServiceBusConnection.FullyQualifiedNamespace, new DefaultAzureCredential());
            _serviceBusMailSender = serviceBusClient.CreateSender(config.ServiceBusConnection.MailQueue);
            _serviceBusTeamNotificationSender = serviceBusClient.CreateSender(config.ServiceBusConnection.AssetReviewNotificationQueue);            
            _pendingReviewsRepository = pendingReviewsRepository;
            _reviewsRepository = reviewsRepository;
            _assetsRepository = assetsRepository;
            _appUrl = config.AppUrl;
            _logger = logger;
            _mapper = mapper;
        }
        
        public async Task<PagedResult<AssetReview>> GetAssetReviewsAsync(string assetId, int pageSize, int page)
        {
            return await _reviewsRepository.GetReviewsAsync(assetId, pageSize, page);
        }

        public async Task<PagedResult<PendingAssetReview>> GetPendingReviewsAsync(int pageSize, int page)
        {
            return await _pendingReviewsRepository.GetPendingReviewsAsync(pageSize, page);
        }
        
        public async Task AddNewReviewAsync(PendingAssetReview review, UserActionMetadata submitterMetadata)
        {
            review.SubmittedBy = submitterMetadata;
            await _pendingReviewsRepository.UpsertPendingReviewAsync(review);

            var asset = await _assetsRepository.GetAssetAsync(review.AssetId);
            if(!asset.UsedBy.Any(u => u.ObjectId == submitterMetadata.ObjectId))
            {
                asset.UsedBy.Add(_mapper.Map<IndexedUserMetadata>(submitterMetadata));
                asset.UsedByCount++;
                await _assetsRepository.UpsertAssetAsync(asset);
            }
            
            var message = new PendingReviewMessage(review.AssetId, review.Review, submitterMetadata);
            await SendTeamNotificationAsync(message);
        }

        public async Task ApprovePendingReviewAsync(string reviewId, UserActionMetadata approverMetadata)
        {
            if (string.IsNullOrEmpty(reviewId))
            {
                throw new ArgumentNullException("reviewId is null");
            }

            var pendingReview = await _pendingReviewsRepository.GetPendingReviewAsync(reviewId);

            var assetReview = _mapper.Map<AssetReview>(pendingReview);
            assetReview.ApprovedBy = approverMetadata;

            await _reviewsRepository.UpsertAssetReviewAsync(assetReview);
            await _pendingReviewsRepository.DeletePendingReviewAsync(reviewId);

            var asset = await _assetsRepository.GetAssetAsync(assetReview.AssetId);
            asset.Reviewed++;
            await _assetsRepository.UpsertAssetAsync(asset);

            var message = new MailServiceBusMessage
            {
                Subject = "Data Science Toolkit - Review APPROVED",
                Body = $"Your review for asset <a href='{_appUrl}assets/{asset.Id}' target='_blank'>{asset.Name}</a> has been approved. <br /><br />" +
                $"Review: {assetReview.Review}",
                To = new List<string> { assetReview.SubmittedBy.Mail }
            };

            await SendMailMessageAsync(message);
        }

        public async Task RejectPendingReviewAsync(string reviewId, UserActionMetadata approverMetadata)
        {
            if (string.IsNullOrEmpty(reviewId))
            {
                throw new ArgumentNullException("reviewId is null");
            }
            
            var review = await _pendingReviewsRepository.GetPendingReviewAsync(reviewId);
            await _pendingReviewsRepository.DeletePendingReviewAsync(reviewId);

            var message = new MailServiceBusMessage
            {
                Subject = "Data Science Toolkit - Review REJECTED",
                Body = $"Your review for this <a href='{_appUrl}assets/{review.AssetId}' target='_blank'>asset</a> has been rejected. <br /><br />" +
                $"Review: {review.Review}",
                To = new List<string> { review.SubmittedBy.Mail }
            };

            await SendMailMessageAsync(message);
        }

        private async Task SendMailMessageAsync(MailServiceBusMessage message)
        {
            var serviceBusMessage = new ServiceBusMessage(JsonSerializer.Serialize(message));
            await _serviceBusMailSender.SendMessageAsync(serviceBusMessage);
        }

        private async Task SendTeamNotificationAsync(PendingReviewMessage message)
        {
            var serviceBusMessage = new ServiceBusMessage(JsonSerializer.Serialize(message));
            await _serviceBusTeamNotificationSender.SendMessageAsync(serviceBusMessage);
        }
        
    }
}

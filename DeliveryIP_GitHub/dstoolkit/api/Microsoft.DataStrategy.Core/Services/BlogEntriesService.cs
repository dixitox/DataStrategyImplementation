using Azure.Identity;
using Azure.Messaging.ServiceBus;
using Microsoft.DataStrategy.Core.Facades;
using Microsoft.DataStrategy.Core.Models;
using Microsoft.DataStrategy.Core.Models.ServiceBusModels;
using Microsoft.DataStrategy.Core.Repositories.Interfaces;
using Microsoft.DataStrategy.Core.Services.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Graph;
using System.Text.Json;
using Image = Microsoft.DataStrategy.Core.Models.Image;

namespace Microsoft.DataStrategy.Core.Services
{
    public class BlogEntriesService : IBlogEntriesService
    {
        private readonly IStorageFacade _storageFacade;
        private readonly IBlogEntriesRepository _blogEntriesRepository;   
        private readonly ServiceBusSender _serviceBusSender;
        private ILogger<AssetsService> _logger;
        private readonly string _blogImgsDirectory;

        public BlogEntriesService(IStorageFacade storageFacade, IBlogEntriesRepository blogEntriesRepository, ILogger<AssetsService> logger, AppConfig config)
        {
            _storageFacade = storageFacade;
            _blogEntriesRepository = blogEntriesRepository;
            var serviceBusClient = new ServiceBusClient(config.ServiceBusConnection.FullyQualifiedNamespace, new DefaultAzureCredential());
            _blogImgsDirectory = config.Storage.BlogImagesDirectory;
            _logger = logger;
        }

        public async Task<BlogEntry?> GetBlogEntryAsync(string id) => await _blogEntriesRepository.GetBlogEntryAsync(id);
        public async Task<PagedResult<BlogEntryBase>> GetBlogEntriesBaseAsync(int pageSize, int page, bool? onlyPublished, string? orderBy, bool descending = true) => await _blogEntriesRepository.GetBlogEntryBaseAsync(pageSize, page, onlyPublished, orderBy, descending);
        
        public async Task CreateBlogEntryAsync(BlogEntry entry)
        {
            await UpsertBlogEntryAsync(entry);
        }
        
        public async Task<string> CreateBlogEntryImageAsync(string base64)
        {
            var contentType = base64.Substring(0, base64.IndexOf(';')).Replace("data:", "");
            var content = Convert.FromBase64String(base64.Split(',')[1]);
            var path = $"{_blogImgsDirectory}/{Guid.NewGuid()}.{GetFileExtensionFromContentType(contentType)}";
            await _storageFacade.UploadFileAsync(path, content);
            return path;            
        }
        public async Task<List<string>> GetBlogEntryImagesAsync()
        {
            return await _storageFacade.ListFilesAsync(_blogImgsDirectory);            
        }

        public async Task UpdateBlogEntryAsync(BlogEntry entry)
        {
            await UpsertBlogEntryAsync(entry);
        }

        private async Task UpsertBlogEntryAsync(BlogEntry entry)
        {
            if (!string.IsNullOrEmpty(entry.HeroImage?.Base64))
            {
                var contentType = entry.HeroImage.Base64.Substring(0, entry.HeroImage.Base64.IndexOf(';')).Replace("data:", "");
                var content = Convert.FromBase64String(entry.HeroImage.Base64.Split(',')[1]);
                entry.HeroImage.Path = $"{_blogImgsDirectory}/{entry.Id}.{GetFileExtensionFromContentType(contentType)}";
                await _storageFacade.UploadFileAsync(entry.HeroImage.Path, content);
            }

            await _blogEntriesRepository.UpsertBlogEntryAsync(entry);
        }

        public async Task DeleteBlogEntryAsync(string id, UserActionMetadata userInfo)
        {
            var asset = await _blogEntriesRepository.GetBlogEntryAsync(id);
            if (asset == null) return;         

            //if (asset.Screenshot != null)
            //{
            //    await _storageFacade.DeleteFileAsync(asset.Screenshot.Path);
            //}

            await _blogEntriesRepository.DeleteBlogEntryAsync(id);
        }

        public async Task DeleteBlogEntryImageAsync(string name)
        {
            await _storageFacade.DeleteFileAsync($"{_blogImgsDirectory}/{name}");
        }        

        private string GetFileExtensionFromContentType(string contentType)
        {
            var mappings = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase) {
                {"image/bmp", "bmp"},
                {"image/jpeg", "jpg"},
                {"image/png", "png"},
                {"image/x-png", "png"}
                };
            return mappings[contentType];
        }

        private async Task SendMailMessageAsync(MailServiceBusMessage message)
        {
            //var internalMessage = new MailServiceBusMessage
            //{
            //    Bcc = _mailRecipients,
            //    Subject = "Data Science Toolkit - Contact Request",
            //    Body = $"New Contact request from {User.GetDisplayName()} <br /><br />{message.Body}",
            //    To = new List<string> { User.GetDisplayName() }
            //};

            var serviceBusMessage = new ServiceBusMessage(JsonSerializer.Serialize(message));
            await _serviceBusSender.SendMessageAsync(serviceBusMessage);
        }

    }
}

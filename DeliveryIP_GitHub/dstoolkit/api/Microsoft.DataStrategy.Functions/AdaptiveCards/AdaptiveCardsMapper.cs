using AdaptiveCards;
using Microsoft.DataStrategy.Core;
using Microsoft.DataStrategy.Core.Models;
using Microsoft.DataStrategy.Core.Models.ServiceBusModels;
using System;
using System.Collections.Generic;

namespace Microsoft.DataStrategy.Functions.AdaptiveCards
{
    public class AdaptiveCardsMapper : IAdaptiveCardsMapper
    {
        private string _teamsImagePath { get; set; }
        private string _appAssetUrl { get; set; }
        private string _appPendingReviewsUrl { get; set; }
        private string _manageAssetsUrl { get; set; }


        private string _userDeeplinkUrl { get; set; }
        private const string _demoActionText = "Demo";
        private const string _gitHubActionText = "Go to GitHub";
        private const string _goToReviewManager = "Go To Pending Reviews";
        private const string _goToAcceleratorActionText = "Go To Accelerator";
        private const string _reviewAcceleratorActionText = "Review Accelerator";
        private const string _manageAssetsActionText = "Manage Assets";
        private const string _chatWithActionText = "Chat with {0}";



        public AdaptiveCardsMapper(AppConfig config)
        {
            _teamsImagePath = config.Teams.ImagePath;
            _appAssetUrl = config.AppUrl + "/assets/{0}?login";
            _appPendingReviewsUrl = config.AppUrl + "/pendingReviews?login";
            _userDeeplinkUrl = config.Teams.UserDeepLink;
            _manageAssetsUrl = config.AppUrl + "/manage?login";
        }

        public string MapToAdaptiveCard(AssetOperationMessage message)
        {
            string title;
            AdaptiveCard card = null;
            switch (message.Operation)
            {
                case AssetOperation.Created:
                    title = $"New Request: {message.Asset.Name}";
                    card = MapToAssetOperationAdaptiveCard(title, message);
                    card.Actions.Add(GoToBotton(_reviewAcceleratorActionText, string.Format(_appAssetUrl, message.Asset.Id)));
                    card.Actions.Add(GoToBotton(_manageAssetsActionText, _manageAssetsUrl));
                    if (!string.IsNullOrEmpty(message.Asset.RepositoryUrl) && Uri.IsWellFormedUriString(message.Asset.RepositoryUrl, UriKind.Absolute))
                        card.Actions.Add(GoToBotton(_gitHubActionText, message.Asset.RepositoryUrl));
                    if (!string.IsNullOrEmpty(message.Asset.DemoUrl) && Uri.IsWellFormedUriString(message.Asset.DemoUrl, UriKind.Absolute))
                        card.Actions.Add(GoToBotton(_demoActionText, message.Asset.DemoUrl));
                    card.Actions.Add(ChatWithAction(message.UserMetadata));
                    break;
                case AssetOperation.Updated:
                    title = $"Asset update: {message.Asset.Name}";
                    card = MapToAssetOperationAdaptiveCard(title, message);
                    card.Actions.Add(GoToBotton(_reviewAcceleratorActionText, string.Format(_appAssetUrl, message.Asset.Id)));
                    card.Actions.Add(GoToBotton(_manageAssetsActionText, _manageAssetsUrl));
                    if (!string.IsNullOrEmpty(message.Asset.RepositoryUrl) && Uri.IsWellFormedUriString(message.Asset.RepositoryUrl, UriKind.Absolute))
                        card.Actions.Add(GoToBotton(_gitHubActionText, message.Asset.RepositoryUrl));
                    if (!string.IsNullOrEmpty(message.Asset.DemoUrl) && Uri.IsWellFormedUriString(message.Asset.DemoUrl, UriKind.Absolute))
                        card.Actions.Add(GoToBotton(_demoActionText, message.Asset.DemoUrl));
                    card.Actions.Add(ChatWithAction(message.UserMetadata));
                    break;
                case AssetOperation.Deleted:
                    title = $"Asset deleted: {message.Asset.Name}";
                    card = MapToAssetOperationAdaptiveCard(title, message);
                    card.Actions.Add(GoToBotton(_manageAssetsActionText, _manageAssetsUrl));
                    if (!string.IsNullOrEmpty(message.Asset.RepositoryUrl) && Uri.IsWellFormedUriString(message.Asset.RepositoryUrl, UriKind.Absolute))
                        card.Actions.Add(GoToBotton(_gitHubActionText, message.Asset.RepositoryUrl));
                    if (!string.IsNullOrEmpty(message.Asset.DemoUrl) && Uri.IsWellFormedUriString(message.Asset.DemoUrl, UriKind.Absolute))
                        card.Actions.Add(GoToBotton(_demoActionText, message.Asset.DemoUrl));
                    card.Actions.Add(ChatWithAction(message.UserMetadata));
                    break;
            }

            var id = Guid.NewGuid().ToString("N");
            //var teamsMessage = $"{{ \"type\":\"message\", \"attachments\":[{{\"contentType\":\"application/vnd.microsoft.card.adaptive\",\"contentUrl\":null,\"content\":{card.ToJson()}}}]}}";
            var teamsMessage = $"{{ \"type\":\"message\", \"body\":{{ \"contentType\": \"html\", \"content\": \"<attachment id='{id}'></attachment>\" }}, \"attachments\":[{{\"id\":\"{id}\", \"contentType\":\"application/vnd.microsoft.card.adaptive\",\"contentUrl\":null,\"content\":{card.ToJson()}}}]}}";

            return teamsMessage;
        }

        public string MapToAdaptiveCard(PendingReviewMessage message)
        {
            var title = $"New Review from {message.UserMetadata.Mail}";
            var card = MapToAssetReviewAdaptiveCard(title, message);
            card.Actions.Add(GoToBotton(_goToReviewManager, string.Format(_appAssetUrl, _appPendingReviewsUrl)));
            card.Actions.Add(GoToBotton(_goToAcceleratorActionText, string.Format(_appAssetUrl, message.AssetId)));
            card.Actions.Add(ChatWithAction(message.UserMetadata));            
            var id = Guid.NewGuid().ToString("N");
            var teamsMessage = $"{{ \"type\":\"message\", \"body\":{{ \"contentType\": \"html\", \"content\": \"<attachment id='{id}'></attachment>\" }}, \"attachments\":[{{\"id\":\"{id}\", \"contentType\":\"application/vnd.microsoft.card.adaptive\",\"contentUrl\":null,\"content\":{card.ToJson()}}}]}}";
            return teamsMessage;
        }

        private AdaptiveAction ChatWithAction(UserActionMetadata user)
        {
            return new AdaptiveOpenUrlAction()
            {
                Title = string.Format(_chatWithActionText, user.DisplayName),
                Url = new Uri(string.Format(_userDeeplinkUrl, user.Mail))
            };
        }
        private AdaptiveAction GoToBotton(string text, string url)
        {            
            return new AdaptiveOpenUrlAction()
            {
                Title = text,
                Url = new Uri(url)
            };
        }

        private AdaptiveCard MapToAssetOperationAdaptiveCard(string title, AssetOperationMessage message)
        {
            AdaptiveCard card = new AdaptiveCard(new AdaptiveSchemaVersion(1, 4));
            card.VerticalContentAlignment = AdaptiveVerticalContentAlignment.Center;
            card.Body.Add(new AdaptiveTextBlock()
            {
                Text = title,
                Size = AdaptiveTextSize.Medium,
                Weight = AdaptiveTextWeight.Bolder
            });

            card.Body.Add(new AdaptiveColumnSet()
            {
                Columns = new List<AdaptiveColumn>()
                {
                    new AdaptiveColumn()
                    {
                        Items = new List<AdaptiveElement>()
                        {
                            new AdaptiveImage()
                            {
                                Style = AdaptiveImageStyle.Person,
                                Size = AdaptiveImageSize.Small,
                                Url = new Uri(string.Format(_teamsImagePath, message.UserMetadata.ObjectId, message.UserMetadata.DisplayName))
                            }
                        },
                        Width = "auto"
                    },
                    new AdaptiveColumn()
                    {
                        Items = new List<AdaptiveElement>()
                        {
                            new AdaptiveTextBlock()
                            {
                                Text = $"{message.UserMetadata.DisplayName}",
                                Weight = AdaptiveTextWeight.Bolder,
                                Wrap = true
                            },
                            new AdaptiveTextBlock()
                            {
                                Text = $"{message.Operation} {message.UserMetadata.On}",
                                Spacing = AdaptiveSpacing.None,
                                Wrap = true,
                                IsSubtle = true
                            }
                        },
                        Width = "stretch"
                    }
                }
            });

            card.Body.Add(new AdaptiveFactSet()
            {
                Facts = new List<AdaptiveFact>()
                {
                    new AdaptiveFact()
                    {
                        Title = "Name",
                        Value = message.Asset.Name
                    },
                    new AdaptiveFact()
                    {
                        Title = "Tagline",
                        Value = message.Asset.Tagline
                    },
                    //new AdaptiveFact()
                    //{
                    //    Title = "Business Problem",
                    //    Value = message.Asset.BusinessProblem
                    //},
                    //new AdaptiveFact()
                    //{
                    //    Title = "Business Value",
                    //    Value = message.Asset.BusinessValue != null ? string.Join("\n ", message.Asset.BusinessValue) : string.Empty
                    //},
                    new AdaptiveFact()
                    {
                        Title = "Description",
                        Value = message.Asset.Description
                    },
                    new AdaptiveFact()
                    {
                        Title = "Industries",
                        Value = message.Asset.Industries != null ? string.Join(", ", message.Asset.Industries) : string.Empty
                    },
                    new AdaptiveFact()
                    {
                        Title = "Tags",
                        Value = message.Asset.Tags != null ? string.Join(", ", message.Asset.Tags) : string.Empty
                    }
                    //,new AdaptiveFact()
                    //{
                    //    Title = "Modeling approach and training",
                    //    Value = message.Asset.ModelingApproachAndTraining
                    //},
                    //new AdaptiveFact()
                    //{
                    //    Title = "Value",
                    //    Value = message.Asset.Value != null ? string.Join("\n ", message.Asset.Value) : string.Empty
                    //},
                    //new AdaptiveFact()
                    //{
                    //    Title = "Data",
                    //    Value = message.Asset.Data
                    //},
                    //new AdaptiveFact()
                    //{
                    //    Title = "Architecture",
                    //    Value = message.Asset.Architecture != null ? string.Join("\n ", message.Asset.Architecture) : string.Empty
                    //}
                },
                IsVisible = true,


            }); ;

            return card;
        }


        private AdaptiveCard MapToAssetReviewAdaptiveCard(string title, PendingReviewMessage message)
        {
            AdaptiveCard card = new AdaptiveCard(new AdaptiveSchemaVersion(1, 4));
            card.VerticalContentAlignment = AdaptiveVerticalContentAlignment.Center;
            card.Body.Add(new AdaptiveTextBlock()
            {
                Text = title,
                Size = AdaptiveTextSize.Medium,
                Weight = AdaptiveTextWeight.Bolder
            });

            card.Body.Add(new AdaptiveColumnSet()
            {
                Columns = new List<AdaptiveColumn>()
                {
                    new AdaptiveColumn()
                    {
                        Items = new List<AdaptiveElement>()
                        {
                            new AdaptiveImage()
                            {
                                Style = AdaptiveImageStyle.Person,
                                Size = AdaptiveImageSize.Small,
                                Url = new Uri(string.Format(_teamsImagePath, message.UserMetadata.ObjectId, message.UserMetadata.DisplayName))
                            }
                        },
                        Width = "auto"
                    },
                    new AdaptiveColumn()
                    {
                        Items = new List<AdaptiveElement>()
                        {
                            new AdaptiveTextBlock()
                            {
                                Text = $"{message.UserMetadata.DisplayName}",
                                Weight = AdaptiveTextWeight.Bolder,
                                Wrap = true
                            },
                            new AdaptiveTextBlock()
                            {
                                Text = $"On {message.UserMetadata.On}",
                                Spacing = AdaptiveSpacing.None,
                                Wrap = true,
                                IsSubtle = true
                            }
                        },
                        Width = "stretch"
                    }
                }
            });

            card.Body.Add(new AdaptiveTextBlock()
            {
                Text = message.Review,
                Size = AdaptiveTextSize.Default,
                Weight = AdaptiveTextWeight.Default
            });

            return card;
        }






    }
}

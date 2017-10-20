using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.YouTube.v3;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using YTApp.Classes;
using static YTApp.MainPage;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace YTApp.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class HomePage : Page
    {
        public MainPage MainPageReference;

        private YouTubeService service;
        private List<YoutubeItemDataType> YTItemsList = new List<YoutubeItemDataType>();

        public HomePage()
        {
            this.InitializeComponent();
            GetService();
        }

        public async void GetService()
        {
            UserCredential credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(new ClientSecrets
            {
                ClientId = "957928808020-pa0lopl3crh565k6jd4djaj36rm1d9i5.apps.googleusercontent.com",
                ClientSecret = "oB9U6yWFndnBqLKIRSA0nYGm"
            }, new[] { YouTubeService.Scope.Youtube }, "user", CancellationToken.None);

            // Create the service.
            service = new YouTubeService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = "Youtube Viewer",
            });
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            Params result = (Params)e.Parameter;
            base.OnNavigatedTo(e);
            MainPageReference = result.mainPageRef;
            UpdateHomeItems();
        }

        private async void UpdateHomeItems()
        {
            VideoItemGridView.Items.Clear();
            foreach (SubscriptionDataType subscription in MainPageReference.subscriptionsList)
            {
                if (subscription.NewVideosCount != "")
                {
                    var tempService = service.Search.List("snippet");
                    tempService.ChannelId = subscription.Id;
                    tempService.Order = SearchResource.ListRequest.OrderEnum.Date;
                    tempService.MaxResults = 5;
                    var tempList = await tempService.ExecuteAsync();
                    foreach (var video in tempList.Items)
                    {
                        DateTime now = DateTime.Now;
                        if (video.Snippet.PublishedAt > now.AddHours(-24) && video.Snippet.PublishedAt <= now)
                        {
                            var VideoToAdd = new YoutubeItemDataType();
                            VideoToAdd.Author = video.Snippet.ChannelTitle;
                            VideoToAdd.Description = video.Snippet.Description;
                            VideoToAdd.Thumbnail = video.Snippet.Thumbnails.Medium.Url;
                            VideoToAdd.Title = video.Snippet.Title;
                            VideoToAdd.Ylink = video.Id.VideoId;
                            VideoToAdd.ViewsAndDate = "wow";
                            VideoItemGridView.Items.Add(VideoToAdd);
                        }
                    }
                }
            }
        }

        private async Task Run()
        {
            var youtubeService = new YouTubeService(new BaseClientService.Initializer()
            {
                ApiKey = "AIzaSyCXOZJH2GUbdqwxZwsjTU93lFvgdnMOVD0",
                ApplicationName = this.GetType().ToString()
            });

            var RecommendedVideosRequest = youtubeService.Activities.List("snippet");
            RecommendedVideosRequest.Mine = true;
            RecommendedVideosRequest.OauthToken = App.OAuthCode;

            var RecommendedVideosResults = await RecommendedVideosRequest.ExecuteAsync();

            string VideoIDs = "";
            //foreach (var Result in RecommendedVideosResults.Items) { VideoIDs += Result.Id. + ","; }
            var getViewsRequest = youtubeService.Videos.List("statistics");
            getViewsRequest.Id = VideoIDs.Remove(VideoIDs.Length - 1);

            var videoListResponse = await getViewsRequest.ExecuteAsync();
            List<string> VideoIDsSplit = VideoIDs.Split(new String[] { "," }, StringSplitOptions.RemoveEmptyEntries).ToList();

            foreach (var result in RecommendedVideosResults.Items)
            {
            }
        }
    }
}

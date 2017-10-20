using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.YouTube.v3;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using System.Threading.Tasks;
using VideoLibrary;
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
        private ObservableCollection<YoutubeItemDataType> YTItemsList = new ObservableCollection<YoutubeItemDataType>();
        private ObservableCollection<YoutubeItemDataType> YTItemsListTemp = new ObservableCollection<YoutubeItemDataType>();

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

        private void YoutubeItemsGridView_ItemClick(object sender, ItemClickEventArgs e)
        {
            var item = (YoutubeItemDataType)e.ClickedItem;
            var youTube = YouTube.Default;
            var video = youTube.GetVideo(item.Ylink);
            MainPageReference.StartVideo(video.Uri);
        }

        private async void UpdateHomeItems()
        {
            VideoItemGridView.Items.Clear();
            Parallel.ForEach(MainPageReference.subscriptionsList, subscription =>
            {
                if (subscription.NewVideosCount != "")
                {
                    var tempService = service.Search.List("snippet");
                    tempService.ChannelId = subscription.Id;
                    tempService.Order = SearchResource.ListRequest.OrderEnum.Date;
                    tempService.MaxResults = 5;
                    var tempList = tempService.Execute();
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
                            VideoToAdd.Id = video.Id.VideoId;
                            VideoToAdd.Ylink = "https://www.youtube.com/watch?v=" + video.Id.VideoId;
                            VideoToAdd.ViewsAndDate = " Views • " + TimeSinceDate(video.Snippet.PublishedAt);
                            YTItemsListTemp.Add(VideoToAdd);
                        }
                    }
                }
            });
            string VideoIDs = "";
            foreach (var video in YTItemsListTemp) { VideoIDs += video.Id + ","; }
            var getViewsRequest = service.Videos.List("statistics");
            getViewsRequest.Id = VideoIDs.Remove(VideoIDs.Length - 1);

            var videoListResponse = getViewsRequest.Execute();

            for (int i = 0; i < YTItemsListTemp.Count; i++)
            {
                YTItemsListTemp[i].ViewsAndDate = ViewCountShortner(videoListResponse.Items[i].Statistics.ViewCount) + YTItemsListTemp[i].ViewsAndDate;
            }

            YTItemsList = new ObservableCollection<YoutubeItemDataType>(YTItemsListTemp.OrderByDescending(d => d.DateSubmitted).ToList() as List<YoutubeItemDataType>);
        }

        private string ViewCountShortner(ulong? viewCount)
        {
            if (viewCount > 1000000)
            {
                return Convert.ToString(Math.Round(Convert.ToDouble(viewCount / 1000000), 1)) + "M";
            }
            else if (viewCount > 1000)
            {
                return Convert.ToString(Math.Round(Convert.ToDouble(viewCount / 1000), 1)) + "K";
            }
            else
            {
                return Convert.ToString(viewCount);
            }
        }

        private string TimeSinceDate(DateTime? date)
        {
            try
            {
                TimeSpan ts = DateTime.Now.Subtract(Convert.ToDateTime(date));
                if (ts.TotalDays > 365)
                    return String.Format("{0} years ago", (int)ts.TotalDays / 365);
                else if (ts.TotalDays > 30)
                    return String.Format("{0} months ago", (int)ts.TotalDays / 30);
                else if (ts.TotalDays > 1)
                    return String.Format("{0} days ago", (int)ts.TotalDays);
                else if (ts.TotalHours > 1)
                    return String.Format("{0} hours ago", (int)ts.TotalHours);
                else if (ts.TotalMinutes > 1)
                    return String.Format("{0} minutes ago", (int)ts.TotalMinutes);
                else
                    return String.Format("{0} seconds ago", (int)ts.TotalSeconds);
            }
            catch { return "unkown date"; }
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

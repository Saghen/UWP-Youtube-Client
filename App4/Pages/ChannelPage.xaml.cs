using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.YouTube.v3;
using Google.Apis.YouTube.v3.Data;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
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
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;
using YTApp.Classes;
using YTApp.Classes.DataTypes;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace YTApp.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ChannelPage : Page
    {
        public MainPage MainPageReference;
        public string ChannelID;
        public bool isSubscribed;

        public ChannelPage()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            NavigateParams result = (NavigateParams)e.Parameter;
            base.OnNavigatedTo(e);
            MainPageReference = result.mainPageRef;
            ChannelID = result.ID;
            UpdateChannel();
            UpdateChannelHome();
            UpdateVideos();
        }

        #region Main Channel Area
        public async void UpdateChannel()
        {
            var methods = new YoutubeItemMethods();

            UserCredential credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(new ClientSecrets
            {
                ClientId = "957928808020-pa0lopl3crh565k6jd4djaj36rm1d9i5.apps.googleusercontent.com",
                ClientSecret = "oB9U6yWFndnBqLKIRSA0nYGm"
            }, new[] { YouTubeService.Scope.Youtube }, "user", System.Threading.CancellationToken.None);

            // Create the service.
            var service = new YouTubeService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = "Youtube Viewer",
            });

            var GetChannelInfo = service.Channels.List("snippet, brandingSettings, statistics");
            GetChannelInfo.Id = ChannelID;
            var ChannelInfoResults = await GetChannelInfo.ExecuteAsync();

            //Profile Image
            var ProfileImageBrush = new ImageBrush();
            ProfileImageBrush.ImageSource = new BitmapImage(new Uri(ChannelInfoResults.Items[0].Snippet.Thumbnails.High.Url));
            ProfileImage.Fill = ProfileImageBrush;

            //Channel Name
            ChannelName.Text = ChannelInfoResults.Items[0].Snippet.Title;

            //Subscribe Button
            var CheckIfSubscribed = service.Subscriptions.List("snippet");
            CheckIfSubscribed.Mine = true;
            CheckIfSubscribed.ForChannelId = ChannelID;
            var IsSubscribed = await CheckIfSubscribed.ExecuteAsync();

            if(IsSubscribed.Items.Count == 0)
            {
                SubscribeButton.Content = "Subscribe " + methods.ViewCountShortner(ChannelInfoResults.Items[0].Statistics.SubscriberCount);
                isSubscribed = false;
            }
            else
            {
                SubscribeButton.Content = "Subscribed " + methods.ViewCountShortner(ChannelInfoResults.Items[0].Statistics.SubscriberCount);

                var DarkRedBackground = new SolidColorBrush();
                DarkRedBackground.Color = Windows.UI.Color.FromArgb(255, 153, 34, 34);
                SubscribeButton.Background = DarkRedBackground;
                isSubscribed = true;
            }

            //Banner Image
            SplashImage.Source = new BitmapImage(new Uri(ChannelInfoResults.Items[0].BrandingSettings.Image.BannerImageUrl));
        }

        public async void UpdateChannelHome()
        {
            var methods = new YoutubeItemMethods();
            List<YoutubeItemDataType> YoutubeItemsTemp = new List<YoutubeItemDataType>();

            UserCredential credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(new ClientSecrets
            {
                ClientId = "957928808020-pa0lopl3crh565k6jd4djaj36rm1d9i5.apps.googleusercontent.com",
                ClientSecret = "oB9U6yWFndnBqLKIRSA0nYGm"
            }, new[] { YouTubeService.Scope.Youtube }, "user", System.Threading.CancellationToken.None);

            // Create the service.
            var service = new YouTubeService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = "Youtube Viewer",
            });

            #region Uploads
            var GetChannelVideosUploads = service.Search.List("snippet");
            GetChannelVideosUploads.ChannelId = ChannelID;
            GetChannelVideosUploads.Order = SearchResource.ListRequest.OrderEnum.Date;
            GetChannelVideosUploads.Type = "video";
            GetChannelVideosUploads.MaxResults = 15;
            var ChannelVideosResultUploads = await GetChannelVideosUploads.ExecuteAsync();
            foreach (var video in ChannelVideosResultUploads.Items)
            {
                if (video.Id.Kind == "youtube#video" && video.Id.VideoId != null && video.Snippet.LiveBroadcastContent != "live")
                    YoutubeItemsTemp.Add(methods.VideoToYoutubeItem(video));
            }
            methods.FillInViews(YoutubeItemsTemp, service);
            var PlaylistUserControlUploads = new ChannelPlaylistGridView(YoutubeItemsTemp, "Uploads");
            HomeGridView.Children.Add(PlaylistUserControlUploads);
            #endregion

            #region Popular Uploads
            YoutubeItemsTemp.Clear();
            var GetChannelVideosPopular = service.Search.List("snippet");
            GetChannelVideosPopular.ChannelId = ChannelID;
            GetChannelVideosPopular.Order = SearchResource.ListRequest.OrderEnum.ViewCount;
            GetChannelVideosPopular.Type = "video";
            GetChannelVideosPopular.MaxResults = 15;
            var ChannelVideosResultPopular = await GetChannelVideosPopular.ExecuteAsync();
            foreach (var video in ChannelVideosResultPopular.Items)
            { 
                if (video.Id.Kind == "youtube#video" && video.Id.VideoId != null && video.Snippet.LiveBroadcastContent != "live")
                    YoutubeItemsTemp.Add(methods.VideoToYoutubeItem(video));
            }
            methods.FillInViews(YoutubeItemsTemp, service);
            var PlaylistUserControlPopular = new ChannelPlaylistGridView(YoutubeItemsTemp, "Popular Uploads");
            HomeGridView.Children.Add(PlaylistUserControlPopular);
            #endregion

            #region Playlists
            //Get the playlists for the channel
            var GetChannelPlaylists = service.Playlists.List("snippet");
            GetChannelPlaylists.ChannelId = ChannelID;
            GetChannelPlaylists.MaxResults = 3;
            var ChannelPlaylistsResult = GetChannelPlaylists.Execute();

            //Go through each playlist and get all its items
            foreach(var playlist in ChannelPlaylistsResult.Items)
            {
                YoutubeItemsTemp.Clear();
                var GetPlaylistVideos = service.PlaylistItems.List("snippet");
                GetPlaylistVideos.PlaylistId = playlist.Id;
                GetPlaylistVideos.MaxResults = 15;
                var PlaylistVideosResult = await GetPlaylistVideos.ExecuteAsync();
                if(PlaylistVideosResult.Items.Count == 0) { break; }
                foreach (var video in PlaylistVideosResult.Items)
                {
                        YoutubeItemsTemp.Add(methods.VideoToYoutubeItem(video));
                }
                methods.FillInViews(YoutubeItemsTemp, service);
                var PlaylistUserControlPlaylist = new ChannelPlaylistGridView(YoutubeItemsTemp, playlist.Snippet.Title);
                HomeGridView.Children.Add(PlaylistUserControlPlaylist);
            }
            #endregion
        }

        private async void SubscribeButton_Click(object sender, RoutedEventArgs e)
        {
            UserCredential credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(new ClientSecrets
            {
                ClientId = "957928808020-pa0lopl3crh565k6jd4djaj36rm1d9i5.apps.googleusercontent.com",
                ClientSecret = "oB9U6yWFndnBqLKIRSA0nYGm"
            }, new[] { YouTubeService.Scope.Youtube }, "user", System.Threading.CancellationToken.None);

            // Create the service.
            var service = new YouTubeService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = "Youtube Viewer",
            });

            if (isSubscribed == true)
            {
                var unsubscribe = service.Subscriptions.Delete(ChannelID);
                unsubscribe.Execute();
            }
            else
            {
                var subscription = new Subscription();
                subscription.Id = ChannelID;
                var subscribe = service.Subscriptions.Insert(subscription, "snippet");
                subscribe.Execute();
            }
        }
        #endregion

        #region Videos

        public async void UpdateVideos()
        {
            List<YoutubeItemDataType> tempList = new List<YoutubeItemDataType>();

            YoutubeItemMethods methods = new YoutubeItemMethods();

            var youtubeService = new YouTubeService(new BaseClientService.Initializer()
            {
                ApiKey = "AIzaSyCXOZJH2GUbdqwxZwsjTU93lFvgdnMOVD0",
                ApplicationName = this.GetType().ToString()
            });

            var searchListRequest = youtubeService.Search.List("snippet");
            searchListRequest.ChannelId = ChannelID;
            searchListRequest.Type = "video";
            searchListRequest.MaxResults = 50;

            // Call the search.list method to retrieve results matching the specified query term.
            var searchListResponse = await searchListRequest.ExecuteAsync();

            foreach (var video in searchListResponse.Items)
            {
                tempList.Add(methods.VideoToYoutubeItem(video));
            }
            methods.FillInViews(tempList, youtubeService);

            VideosGridView.ItemsSource = tempList;
        }

        private void PlayVideoEvent(object sender, ItemClickEventArgs e)
        {
            var item = (YoutubeItemDataType)e.ClickedItem;
            var youTube = YouTube.Default;
            var video = youTube.GetVideo(item.Ylink);
            MainPageReference.StartVideo(video.Uri);
        }

        #endregion
    }
}

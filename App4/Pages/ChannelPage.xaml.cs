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
using YTApp.Classes.EventsArgs;

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
        public string nextPageToken;
        ObservableCollection<YoutubeItemDataType> VideosList = new ObservableCollection<YoutubeItemDataType>();
        public Channel channel;
        public bool addingVideos = false;

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
            channel = ChannelInfoResults.Items[0];

            //View Count
            VideoCount.Text = channel.Statistics.VideoCount + " videos";

            //Profile Image
            var ProfileImageBrush = new ImageBrush();
            ProfileImageBrush.ImageSource = new BitmapImage(new Uri(channel.Snippet.Thumbnails.High.Url));
            ProfileImage.Fill = ProfileImageBrush;

            //Channel Name
            ChannelName.Text = channel.Snippet.Title;

            //Subscribe Button
            var CheckIfSubscribed = service.Subscriptions.List("snippet");
            CheckIfSubscribed.Mine = true;
            CheckIfSubscribed.ForChannelId = ChannelID;
            var IsSubscribed = await CheckIfSubscribed.ExecuteAsync();

            if (IsSubscribed.Items.Count == 0)
            {
                SubscribeButton.Content = "Subscribe " + YoutubeItemMethodsStatic.ViewCountShortner(channel.Statistics.SubscriberCount);
                isSubscribed = false;
            }
            else
            {
                SubscribeButton.Content = "Subscribed " + YoutubeItemMethodsStatic.ViewCountShortner(channel.Statistics.SubscriberCount);
                isSubscribed = true;
            }

            //Banner Image
            SplashImage.Source = new BitmapImage(new Uri(channel.BrandingSettings.Image.BannerImageUrl));
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
            GetChannelVideosUploads.MaxResults = 10;
            var ChannelVideosResultUploads = await GetChannelVideosUploads.ExecuteAsync();
            foreach (var video in ChannelVideosResultUploads.Items)
            {
                if (video.Id.Kind == "youtube#video" && video.Id.VideoId != null && video.Snippet.LiveBroadcastContent != "live")
                    YoutubeItemsTemp.Add(methods.VideoToYoutubeItem(video));
            }
            methods.FillInViews(YoutubeItemsTemp, service);
            var PlaylistUserControlUploads = new ChannelPlaylistGridView(YoutubeItemsTemp, "Uploads");
            PlaylistUserControlUploads.ItemClicked += HomePageItemClicked;
            HomeGridView.Children.Add(PlaylistUserControlUploads);
            #endregion

            #region Popular Uploads
            YoutubeItemsTemp.Clear();
            var GetChannelVideosPopular = service.Search.List("snippet");
            GetChannelVideosPopular.ChannelId = ChannelID;
            GetChannelVideosPopular.Order = SearchResource.ListRequest.OrderEnum.ViewCount;
            GetChannelVideosPopular.Type = "video";
            GetChannelVideosPopular.MaxResults = 10;
            var ChannelVideosResultPopular = await GetChannelVideosPopular.ExecuteAsync();
            foreach (var video in ChannelVideosResultPopular.Items)
            {
                if (video.Id.Kind == "youtube#video" && video.Id.VideoId != null && video.Snippet.LiveBroadcastContent != "live")
                    YoutubeItemsTemp.Add(methods.VideoToYoutubeItem(video));
            }
            methods.FillInViews(YoutubeItemsTemp, service);
            var PlaylistUserControlPopular = new ChannelPlaylistGridView(YoutubeItemsTemp, "Popular Uploads");
            PlaylistUserControlPopular.ItemClicked += HomePageItemClicked;
            HomeGridView.Children.Add(PlaylistUserControlPopular);
            #endregion

            #region Playlists
            //Get the playlists for the channel
            var GetChannelPlaylists = service.Playlists.List("snippet");
            GetChannelPlaylists.ChannelId = ChannelID;
            GetChannelPlaylists.MaxResults = 3;
            var ChannelPlaylistsResult = GetChannelPlaylists.Execute();

            //Go through each playlist and get all its items
            foreach (var playlist in ChannelPlaylistsResult.Items)
            {
                YoutubeItemsTemp.Clear();
                var GetPlaylistVideos = service.PlaylistItems.List("snippet,status");
                GetPlaylistVideos.PlaylistId = playlist.Id;
                GetPlaylistVideos.MaxResults = 10;
                var PlaylistVideosResult = await GetPlaylistVideos.ExecuteAsync();
                if (PlaylistVideosResult.Items.Count == 0) { break; }
                foreach (var video in PlaylistVideosResult.Items)
                {
                    if (video.Status.PrivacyStatus != "private")
                    {
                        YoutubeItemsTemp.Add(methods.VideoToYoutubeItem(video));
                    }
                }
                methods.FillInViews(YoutubeItemsTemp, service);
                var PlaylistUserControlPlaylist = new ChannelPlaylistGridView(YoutubeItemsTemp, playlist.Snippet.Title);
                PlaylistUserControlPlaylist.ItemClicked += HomePageItemClicked;
                HomeGridView.Children.Add(PlaylistUserControlPlaylist);
            }
            #endregion
        }

        public void HomePageItemClicked(object sender, RoutedEventArgsWithID e)
        {
            MainPageReference.StartVideo(e.ID);
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
                SubscribeButton.Content = "Subscribe " + YoutubeItemMethodsStatic.ViewCountShortner(channel.Statistics.SubscriberCount);

                var getSubscription = service.Subscriptions.List("snippet");
                getSubscription.Mine = true;
                var subscriptions = await getSubscription.ExecuteAsync();
                Subscription subscription = new Subscription();

                await MainPageReference.LoadSubscriptions();
                try
                {
                    var sub = MainPageReference.subscriptionsList.Single(x => x.Id == ChannelID);
                    try
                    {
                        var unsubscribe = service.Subscriptions.Delete(sub.SubscriptionID);
                        await unsubscribe.ExecuteAsync();

                        isSubscribed = false;
                    }
                    catch
                    {
                        //Fires if subscription could not be removed for whatever reason.
                        SubscribeButton.Content = "Subscribed " + YoutubeItemMethodsStatic.ViewCountShortner(channel.Statistics.SubscriberCount + 1);
                    }
                }
                catch
                {
                    //Fires if subscription doesn't exist.
                    SubscribeButton.Content = "Subscribe " + YoutubeItemMethodsStatic.ViewCountShortner(channel.Statistics.SubscriberCount);
                    isSubscribed = false;
                }
            }
            else
            {
                Subscription subscription = new Subscription();
                SubscriptionSnippet snippet = new SubscriptionSnippet();
                ResourceId resourceId = new ResourceId();

                resourceId.ChannelId = ChannelID;
                resourceId.Kind = "youtube#channel";

                snippet.ResourceId = resourceId;
                subscription.Snippet = snippet;

                var subscribe = service.Subscriptions.Insert(subscription, "snippet");
                subscribe.Execute();

                SubscribeButton.Content = "Subscribed " + YoutubeItemMethodsStatic.ViewCountShortner(channel.Statistics.SubscriberCount + 1);

                isSubscribed = true;
            }
        }
        #endregion

        #region Videos

        public async void UpdateVideos()
        {
            if (addingVideos == false)
            {
                addingVideos = true;
            }
            else { return; }
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
            searchListRequest.Order = SearchResource.ListRequest.OrderEnum.Date;
            searchListRequest.MaxResults = 50;

            // Call the search.list method to retrieve results matching the specified query term.
            var searchListResponse = await searchListRequest.ExecuteAsync();

            foreach (var video in searchListResponse.Items)
            {
                tempList.Add(methods.VideoToYoutubeItem(video));
            }
            methods.FillInViews(tempList, youtubeService);

            nextPageToken = searchListResponse.NextPageToken;

            foreach (var item in tempList)
            {
                VideosList.Add(item);
            }
            addingVideos = false;
        }

        public async void AddMoreVideos()
        {
            if (addingVideos == false)
            {
                addingVideos = true;
            }
            else { return; }

            List<YoutubeItemDataType> tempList = new List<YoutubeItemDataType>();
            if (VideosGridView.ItemsSource != null && nextPageToken != null && nextPageToken != "")
            {

            }
            else
            {
                UpdateVideos();
                return;
            }

            YoutubeItemMethods methods = new YoutubeItemMethods();

            var youtubeService = new YouTubeService(new BaseClientService.Initializer()
            {
                ApiKey = "AIzaSyCXOZJH2GUbdqwxZwsjTU93lFvgdnMOVD0",
                ApplicationName = this.GetType().ToString()
            });

            var searchListRequest = youtubeService.Search.List("snippet");
            searchListRequest.ChannelId = ChannelID;
            searchListRequest.Type = "video";
            searchListRequest.Order = SearchResource.ListRequest.OrderEnum.Date;
            searchListRequest.PageToken = nextPageToken;
            searchListRequest.MaxResults = 25;

            // Call the search.list method to retrieve results matching the specified query term.
            var searchListResponse = await searchListRequest.ExecuteAsync();

            foreach (var video in searchListResponse.Items)
            {
                tempList.Add(methods.VideoToYoutubeItem(video));
            }
            methods.FillInViews(tempList, youtubeService);
            nextPageToken = searchListResponse.NextPageToken;

            foreach (var video in tempList)
            {
                VideosList.Add(video);
            }
            addingVideos = false;
        }

        private void PlayVideoEvent(object sender, ItemClickEventArgs e)
        {
            var item = (YoutubeItemDataType)e.ClickedItem;
            MainPageReference.StartVideo(item.Id);
        }
        #endregion

        private void ScrollViewer_ViewChanged(object sender, ScrollViewerViewChangedEventArgs e)
        {
            if (rootPivot.SelectedItem == VideoPivot)
            {
                var verticalOffset = MainScrollViewer.VerticalOffset;
                var maxVerticalOffset = MainScrollViewer.ScrollableHeight - 1000; //sv.ExtentHeight - sv.ViewportHeight;

                if (maxVerticalOffset < 0 ||
                    verticalOffset >= maxVerticalOffset)
                {
                    AddMoreVideos();
                }
            }
        }
    }
}

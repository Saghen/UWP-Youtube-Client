using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.YouTube.v3;
using Google.Apis.YouTube.v3.Data;
using MetroLog;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
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
        public bool isSubscribed;
        public string nextPageToken;

        private ILogger Log = LogManagerFactory.DefaultLogManager.GetLogger<ChannelPage>();

        ObservableCollection<YoutubeItemDataType> VideosList = new ObservableCollection<YoutubeItemDataType>();

        ObservableCollection<PlaylistDataType> playlists = new ObservableCollection<PlaylistDataType>();
        BlockingCollection<PlaylistDataType> playlistsTemp = new BlockingCollection<PlaylistDataType>();

        ObservableCollection<YoutubeChannelDataType> featuredChannels = new ObservableCollection<YoutubeChannelDataType>();
        BlockingCollection<YoutubeChannelDataType> featuredChannelsTemp = new BlockingCollection<YoutubeChannelDataType>();

        public Channel channel;
        public bool addingVideos = false;

        public ChannelPage()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            UpdateChannel();
            UpdateChannelHome();
            UpdateVideos();
        }

        #region Home
        public async void UpdateChannel()
        {
            var GetChannelInfo = (await YoutubeItemMethodsStatic.GetServiceAsync()).Channels.List("snippet, brandingSettings, statistics");
            GetChannelInfo.Id = Constants.activeChannelID;
            var ChannelInfoResults = GetChannelInfo.Execute();
            channel = ChannelInfoResults.Items[0];

            //View Count
            VideoCount.Text = channel.Statistics.VideoCount + " videos";

            //Profile Image
            var ProfileImageBrush = new ImageBrush{ ImageSource = new BitmapImage(new Uri(channel.Snippet.Thumbnails.High.Url)) };
            ProfileImage.Fill = ProfileImageBrush;

            //Channel Name
            ChannelName.Text = channel.Snippet.Title;

            //Subscribe Button
            var CheckIfSubscribed = (await YoutubeItemMethodsStatic.GetServiceAsync()).Subscriptions.List("snippet");
            CheckIfSubscribed.Mine = true;
            CheckIfSubscribed.ForChannelId = Constants.activeChannelID;
            var IsSubscribed = CheckIfSubscribed.Execute();

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

            //About Page
            if (channel.BrandingSettings.Channel.Description != null)
                ChannelAboutText.Text = channel.BrandingSettings.Channel.Description;
            else
                ChannelAboutText.Text = "This channel does not have a description.";
        }

        public async void UpdateChannelHome()
        {
            Task t1 = Task.Run(() => UpdatePopularUploads());
            Task t2 = Task.Run(() => UpdateFeaturedChannels());
            Task t3 = Task.Run(() => UpdateChannelSections());

            await Task.WhenAll(t1, t2);

            foreach (var playlist in playlistsTemp)
                playlists.Add(playlist);

            foreach (var channel in featuredChannelsTemp)
                featuredChannels.Add(channel);

            await Task.WhenAll(t3);

            foreach (var playlist in playlistsTemp)
            {
                if (playlist.Title == "Popular Uploads")
                    continue;
                playlists.Add(playlist);
            }
        }

        public async void UpdatePopularUploads()
        {
            var service = await YoutubeItemMethodsStatic.GetServiceAsync();

            var methods = new YoutubeItemMethods();
            ObservableCollection<YoutubeItemDataType> YoutubeItemsTemp = new ObservableCollection<YoutubeItemDataType>();

            var GetChannelVideosPopular = service.Search.List("snippet");
            GetChannelVideosPopular.ChannelId = Constants.activeChannelID;
            GetChannelVideosPopular.Order = SearchResource.ListRequest.OrderEnum.ViewCount;
            GetChannelVideosPopular.Type = "video";
            GetChannelVideosPopular.MaxResults = 10;
            var ChannelVideosResultPopular = GetChannelVideosPopular.Execute();
            foreach (var video in ChannelVideosResultPopular.Items)
            {
                if (video.Id.Kind == "youtube#video" && video.Id.VideoId != null && video.Snippet.LiveBroadcastContent != "live")
                    YoutubeItemsTemp.Add(methods.VideoToYoutubeItem(video));
            }
            methods.FillInViews(YoutubeItemsTemp, service);
            playlistsTemp.Add(new PlaylistDataType() { Title = "Popular Uploads", Items = YoutubeItemsTemp });
        }

        public async void UpdateChannelSections()
        {
            var service = await YoutubeItemMethodsStatic.GetServiceAsync();

            var methods = new YoutubeItemMethods();

            //Get the playlists for the channel
            var GetChannelPlaylists = service.ChannelSections.List("snippet,contentDetails");
            GetChannelPlaylists.ChannelId = Constants.activeChannelID;
            var ChannelPlaylistsResult = GetChannelPlaylists.Execute();

            //Check if there are no playlists to process
            if (ChannelPlaylistsResult.Items.Count == 0)
                return;

            List<ObservableCollection<YoutubeItemDataType>> tempGridViews = new List<ObservableCollection<YoutubeItemDataType>>();
            string tempPlaylistIds = "";
            //Go through each playlist and get all its items
            Parallel.ForEach(ChannelPlaylistsResult.Items, playlist =>
            {
                try
                {
                    ObservableCollection<YoutubeItemDataType> tempPlaylistVideos = new ObservableCollection<YoutubeItemDataType>();
                    tempPlaylistVideos.Clear();
                    var GetPlaylistVideos = service.PlaylistItems.List("snippet,status");
                    if (playlist.ContentDetails == null || playlist.ContentDetails.Playlists[0] == null || playlist.Snippet.Type != "singlePlaylist")
                        return;
                    GetPlaylistVideos.PlaylistId = playlist.ContentDetails.Playlists[0];
                    GetPlaylistVideos.MaxResults = 10;
                    var PlaylistVideosResult = GetPlaylistVideos.Execute();
                    foreach (var video in PlaylistVideosResult.Items)
                    {
                        if (video.Status.PrivacyStatus != "private")
                        {
                            tempPlaylistVideos.Add(methods.VideoToYoutubeItem(video));
                        }
                    }
                    methods.FillInViews(tempPlaylistVideos, service);
                    tempGridViews.Add(tempPlaylistVideos);

                    //Add the playlist ID for getting the title later
                    tempPlaylistIds += playlist.ContentDetails.Playlists[0] + ",";
                }
                catch { return; }
            });

            //Check if there are no playlists were outputed
            if (tempPlaylistIds == "")
                return;

            //Gets the title of the playlists
            var getPlaylistTitles = service.Playlists.List("snippet");
            getPlaylistTitles.Id = tempPlaylistIds.Remove(tempPlaylistIds.Length - 1, 1);
            var playlistTitlesList = getPlaylistTitles.Execute();

            for (int i = 0; i < tempGridViews.Count; i++)
            {
                try { playlistsTemp.Add(new PlaylistDataType() { Title = playlistTitlesList.Items[i].Snippet.Title, Items = tempGridViews[i] }); }
                catch { }
            }
        }

        public async void UpdateFeaturedChannels()
        {
            var service = await YoutubeItemMethodsStatic.GetServiceAsync();

            try
            {
                var methods = new YoutubeItemMethods();

                string FeaturedChannelIds = "";
                foreach (var Id in channel.BrandingSettings.Channel.FeaturedChannelsUrls)
                    FeaturedChannelIds += Id + ",";

                var getChannels = service.Channels.List("snippet,statistics");
                getChannels.Id = FeaturedChannelIds.Remove(FeaturedChannelIds.Length - 1, 1);
                var featuredChannelsResponse = getChannels.Execute();

                foreach (var featuredChannel in featuredChannelsResponse.Items)
                    featuredChannelsTemp.Add(methods.ChannelToYoutubeChannel(featuredChannel));
            }
            catch (Exception ex)
            {
                Log.Error("Featured channels failed to load");
                Log.Error(JsonConvert.SerializeObject(channel));
                Log.Error(ex.Message);
            }
        }

        public void HomePageItemClicked(object sender, RoutedEventArgsWithID e)
        {
            Constants.MainPageRef.StartVideo(e.ID);
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

                Constants.MainPageRef.LoadSubscriptions();
                try
                {
                    var sub = Constants.MainPageRef.subscriptionsList.Single(x => x.Id == Constants.activeChannelID);
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
                ResourceId resourceId = new ResourceId { ChannelId = Constants.activeChannelID, Kind = "youtube#channel" };

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
            try
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
                searchListRequest.ChannelId = Constants.activeChannelID;
                searchListRequest.Type = "video";
                searchListRequest.Order = SearchResource.ListRequest.OrderEnum.Date;
                searchListRequest.MaxResults = 25;

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
            catch (Exception ex)
            {
                Log.Error(String.Format("An error occured while updating videos for the channel with the ID {0}", channel.Id));
                Log.Error(ex.Message);
            }
        }

        public async void AddMoreVideos()
        {
            try
            {
                if (addingVideos == false && !(nextPageToken == null || nextPageToken == ""))
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
                searchListRequest.ChannelId = Constants.activeChannelID;
                searchListRequest.Type = "video";
                searchListRequest.Order = SearchResource.ListRequest.OrderEnum.Date;
                searchListRequest.PageToken = nextPageToken;
                searchListRequest.MaxResults = 50;

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
            catch (Exception ex)
            {
                Log.Error(String.Format("An error occured while adding more videos to the channel page with the ID {0}", channel.Id));
                Log.Error(ex.Message);
            }
        }

        private void PlayVideoEvent(object sender, ItemClickEventArgs e)
        {
            var item = (YoutubeItemDataType)e.ClickedItem;
            var gridView = (GridView)sender;
            gridView.PrepareConnectedAnimation("videoThumb", item, "ImageControl");
            Constants.MainPageRef.StartVideo(item.Id);
        }

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
        #endregion

        #region Home Page Playlist Events

        private void PlaylistMoveRight_Click(object sender, RoutedEventArgs e)
        {
            var obj = (Button)e.OriginalSource;
            var parent = (Grid)obj.Parent;
            var gridView = (Controls.GridViewWithXProperty)parent.FindName("Playlist");

            if (-(gridView.Items.Count * 250 - gridView.ActualWidth - 500) < gridView.XValue)
                gridView.XValue += -500;

            else
                gridView.XValue = -(gridView.Items.Count * 250 - gridView.ActualWidth + 50);
        }

        private void PlaylistMoveLeft_Click(object sender, RoutedEventArgs e)
        {
            var obj = (Button)e.OriginalSource;
            var parent = (Grid)obj.Parent;
            var gridView = (Controls.GridViewWithXProperty)parent.FindName("Playlist");

            if (gridView.XValue <= -500)
                gridView.XValue += 500;
            else if (gridView.XValue < 0)
                gridView.XValue = 0;
        }

        private void Playlist_ItemClick(object sender, ItemClickEventArgs e)
        {
            var item = (YoutubeItemDataType)e.ClickedItem;
            var gridView = (Controls.GridViewWithXProperty)sender;
            gridView.PrepareConnectedAnimation("videoThumb", item, "ImageControl");
            Constants.MainPageRef.StartVideo(item.Id);
        }

        private void Playlist_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            var obj = (Microsoft.Toolkit.Uwp.UI.Controls.HeaderedContentControl)sender;
            var parent = (Grid)obj.Parent;
            var gridView = (Controls.GridViewWithXProperty)parent.FindName("Playlist");

            if (gridView.ActualWidth > gridView.Items.Count * 250 && gridView.XValue == 0)
            {
                var btn1 = (Button)parent.FindName("btnMoveLeft");
                btn1.Visibility = Visibility.Collapsed;

                var btn2 = (Button)parent.FindName("btnMoveRight");
                btn2.Visibility = Visibility.Collapsed;
            }
            else
            {
                var btn1 = (Button)parent.FindName("btnMoveLeft");
                btn1.Visibility = Visibility.Visible;

                var btn2 = (Button)parent.FindName("btnMoveRight");
                btn2.Visibility = Visibility.Visible;
            }
        }

        #endregion

        #region Featured Channels Events

        private void FeaturedChannelsMoveRight_Click(object sender, RoutedEventArgs e)
        {
            var obj = (Button)e.OriginalSource;
            var parent = (Grid)obj.Parent;
            var gridView = (Controls.GridViewWithXProperty)parent.FindName("featuredChannelsCtrl");

            if (-(gridView.Items.Count * 180 - gridView.ActualWidth - 320) < gridView.XValue)
                gridView.XValue += -360;
            else
                gridView.XValue = -(gridView.Items.Count * 180 - gridView.ActualWidth + 50);
        }

        private void FeaturedChannelsMoveLeft_Click(object sender, RoutedEventArgs e)
        {
            var obj = (Button)e.OriginalSource;
            var parent = (Grid)obj.Parent;
            var gridView = (Controls.GridViewWithXProperty)parent.FindName("featuredChannelsCtrl");

            if (gridView.XValue <= -360)
                gridView.XValue += 360;
            else if (gridView.XValue < 0)
                gridView.XValue = 0;
        }

        private void FeaturedChannels_ItemClick(object sender, ItemClickEventArgs e)
        {
            var item = (YoutubeChannelDataType)e.ClickedItem;
            Constants.MainPageRef.contentFrame.Navigate(typeof(ChannelPage));
        }

        private void FeaturedChannels_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            var obj = (Microsoft.Toolkit.Uwp.UI.Controls.HeaderedContentControl)sender;
            var parent = (Grid)obj.Parent;
            var gridView = (Controls.GridViewWithXProperty)parent.FindName("featuredChannelsCtrl");

            if (gridView.ActualWidth > gridView.Items.Count * 180 && gridView.XValue == 0)
            {
                var btn1 = (Button)parent.FindName("btnMoveLeft");
                btn1.Visibility = Visibility.Collapsed;

                var btn2 = (Button)parent.FindName("btnMoveRight");
                btn2.Visibility = Visibility.Collapsed;
            }
            else
            {
                var btn1 = (Button)parent.FindName("btnMoveLeft");
                btn1.Visibility = Visibility.Visible;

                var btn2 = (Button)parent.FindName("btnMoveRight");
                btn2.Visibility = Visibility.Visible;
            }
        }

        #endregion
    }
}

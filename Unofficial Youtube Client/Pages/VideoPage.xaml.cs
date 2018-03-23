using Google.Apis.YouTube.v3;
using Google.Apis.YouTube.v3.Data;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Windows.Media;
using Windows.Media.Core;
using Windows.Media.Editing;
using Windows.Media.Playback;
using Windows.Networking.BackgroundTransfer;
using Windows.UI.Core;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;
using YoutubeExplode;
using YTApp.Classes;
using YTApp.Classes.DataTypes;
using YTApp.UserControls;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace YTApp.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class VideoPage : Page
    {
        Windows.Storage.ApplicationDataContainer localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;

        Channel channel;

        PlaylistDataType relatedVideos = new PlaylistDataType();

        ObservableCollection<CommentContainerDataType> commentCollection = new ObservableCollection<CommentContainerDataType>();

        public VideoPage()
        {
            this.InitializeComponent();

            //Keep the page in memory so we don't have to reload it everytime
            NavigationCacheMode = NavigationCacheMode.Enabled;

            //Link to the video complete event
            viewer.controller.videoPlayer.MediaEnded += VideoPlayer_MediaEnded;

            //Set autoplay value
            try { SwitchAutoplay.IsOn = (bool)localSettings.Values["Autoplay"]; } catch { }

        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            ConnectedAnimation imageAnimation = ConnectedAnimationService.GetForCurrentView().GetAnimation("videoThumb");

            if (imageAnimation != null)
            {
                imageAnimation.TryStart(MediaElementContainer);
            }

            Constants.MainPageRef.contentFrame.Navigated += ContentFrame_Navigated;
            SystemNavigationManager.GetForCurrentView().BackRequested += VideoPage_BackRequested;
            Constants.MainPageRef.Frame.Navigated += Frame_Navigated;

            //Set the focus to the video viewer
            viewer.Focus(FocusState.Programmatic);

            //Get the video data and play it
            StartVideo(Constants.activeVideoID);

            //Update likes/dislikes
            LikeDislikeControl.UpdateData();

            //Update comments
            if (CommentsOptionComboBox.SelectedIndex == 0)
                UpdateComments(CommentThreadsResource.ListRequest.OrderEnum.Relevance);
            else
                UpdateComments(CommentThreadsResource.ListRequest.OrderEnum.Time);
        }

        private void Frame_Navigated(object sender, NavigationEventArgs e)
        {
            //If the user logs out, we need to stop the video
            viewer.StopVideo();
        }

        private void VideoPage_BackRequested(object sender, BackRequestedEventArgs e)
        {
            ChangePlayerSize(false);
        }

        private void ContentFrame_Navigated(object sender, NavigationEventArgs e)
        {
            ChangePlayerSize(false);
        }

        #region Methods
        public async void StartVideo(string ID)
        {
            //Make the player cover the entire frame
            ChangePlayerSize(true);

            //Set the ID of our viewer to the new ID
            viewer.Source = ID;
 

            var service = await YoutubeMethodsStatic.GetServiceAsync();

            var getVideoInfo = service.Videos.List("snippet, statistics, contentDetails");
            getVideoInfo.Id = ID;
            var videoList = await getVideoInfo.ExecuteAsync();
            Constants.ActiveVideo = videoList.Items[0];

            //Channel Info
            await Task.Run(() =>
            {
                var getChannelInfo = service.Channels.List("snippet");
                getChannelInfo.Id = Constants.ActiveVideo.Snippet.ChannelId;
                var channelInfo = getChannelInfo.Execute();
                channel = channelInfo.Items[0];
            });

            UpdatePageInfo(service);

            UpdateRelatedVideos(service);
        }

        public void UpdatePageInfo(YouTubeService service)
        {
            var methods = new YoutubeMethods();

            Title.Text = Constants.ActiveVideo.Snippet.Title;
            Views.Text = string.Format("{0:#,###0.#}", Constants.ActiveVideo.Statistics.ViewCount) + " Views";

            ChannelTitle.Text = channel.Snippet.Title;
            DatePosted.Text = Constants.ActiveVideo.Snippet.PublishedAt.Value.ToString("MMMM d, yyyy");
            Description.Text = Constants.ActiveVideo.Snippet.Description;
            DescriptionShowMore.Visibility = Visibility.Visible;
            var image = new BitmapImage(new Uri(channel.Snippet.Thumbnails.High.Url));
            var imageBrush = new ImageBrush { ImageSource = image };
            ChannelProfileIcon.Fill = imageBrush;
        }

        public async void UpdateRelatedVideos(YouTubeService service)
        {
            System.Collections.ObjectModel.ObservableCollection<YoutubeItemDataType> relatedVideosList = new System.Collections.ObjectModel.ObservableCollection<YoutubeItemDataType>();
            await Task.Run(() =>
            {
                var getRelatedVideos = service.Search.List("snippet");
                getRelatedVideos.RelatedToVideoId = Constants.activeVideoID;
                getRelatedVideos.MaxResults = 15;
                getRelatedVideos.Type = "video";
                var relatedVideosResponse = getRelatedVideos.Execute();

                var methods = new YoutubeMethods();
                foreach (SearchResult video in relatedVideosResponse.Items)
                {
                    relatedVideosList.Add(methods.VideoToYoutubeItem(video));
                }
                methods.FillInViews(relatedVideosList, service);
            });
            relatedVideos.Items = relatedVideosList;
        }

        private void YoutubeItemsGridView_ItemClick(object sender, ItemClickEventArgs e)
        {
            var item = (YoutubeItemDataType)e.ClickedItem;
            Constants.MainPageRef.StartVideo(item.Id);
        }

        //AChangePlayerSize takes a bool allowing you to set it to fullscreen (true) or to a small view (false)
        public void ChangePlayerSize(bool MakeFullScreen)
        {
            if (!MakeFullScreen)
            {
                viewer.transportControls.Visibility = Visibility.Collapsed;

                Scrollviewer.ChangeView(0, 0, 1, true);
                Scrollviewer.VerticalScrollMode = ScrollMode.Disabled;
                Scrollviewer.VerticalScrollBarVisibility = ScrollBarVisibility.Disabled;

                Frame.HorizontalAlignment = HorizontalAlignment.Right;
                Frame.VerticalAlignment = VerticalAlignment.Bottom;
                Frame.Width = 640;
                Frame.Height = 360;

                //Saves the current Media Player height
                Windows.Storage.ApplicationDataContainer localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
                localSettings.Values["MediaViewerHeight"] = MediaRow.Height.Value;

                MediaRow.Height = new GridLength(360);

                //Disable the taps on the viewer
                viewer.IsHitTestVisible = false;
            }
            else
            {
                viewer.transportControls.Visibility = Visibility.Visible;

                Scrollviewer.VerticalScrollMode = ScrollMode.Auto;
                Scrollviewer.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;

                Frame.HorizontalAlignment = HorizontalAlignment.Stretch;
                Frame.VerticalAlignment = VerticalAlignment.Stretch;
                Frame.Width = Double.NaN;
                Frame.Height = Double.NaN;

                //Set the media viewer to the previous height or to the default if a custom height is not found
                Windows.Storage.ApplicationDataContainer localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
                if (localSettings.Values["MediaViewerHeight"] != null && (double)localSettings.Values["MediaViewerHeight"] > 360)
                    MediaRow.Height = new GridLength(Convert.ToDouble(localSettings.Values["MediaViewerHeight"]));
                else
                    MediaRow.Height = new GridLength(600);

                //Enable the taps on the viewer
                viewer.IsHitTestVisible = true;
            }
        }

        #endregion

        #region Events

        private void MinimizeMediaElement_Click(object sender, RoutedEventArgs e)
        {
            if (MediaRow.Height.Value == 360) { ChangePlayerSize(true); }
            else { ChangePlayerSize(false); }
        }

        private void CloseMediaElement_Click(object sender, RoutedEventArgs e)
        {
            viewer.StopVideo();
            Frame.Visibility = Visibility.Collapsed;
        }
        #endregion

        #region MediaElementButton Management
        private void Viewer_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            FadeIn.Begin();
        }

        private void Viewer_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            FadeOut.Begin();
        }
        #endregion

        #region Channel Clicked

        private void OpenChannel(object sender, TappedRoutedEventArgs e)
        {
            Constants.activeChannelID = channel.Id;
            Constants.MainPageRef.contentFrame.Navigate(typeof(ChannelPage));
        }

        private void ChannelProfileIcon_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            Window.Current.CoreWindow.PointerCursor = new CoreCursor(CoreCursorType.Hand, 1);
        }

        private void ChannelProfileIcon_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            Window.Current.CoreWindow.PointerCursor = new CoreCursor(CoreCursorType.Arrow, 2);
        }

        #endregion

        #region Description
        private void DescriptionShowMore_Click(object sender, RoutedEventArgs e)
        {
            if ((string)DescriptionShowMore.Content == "Show less")
            {
                Description.MaxHeight = 200;
                DescriptionShowMore.Content = "Show more";
            }
            else
            {
                Description.MaxHeight = 10000;
                DescriptionShowMore.Content = "Show less";
            }
        }

        private async void Description_LinkClicked(object sender, Microsoft.Toolkit.Uwp.UI.Controls.LinkClickedEventArgs e)
        {
            await Windows.System.Launcher.LaunchUriAsync(new Uri(e.Link));
        }
        #endregion Description

        #region Fullscreen

        private void viewer_EnteringFullscreen(object sender, EventArgs e)
        {
            Constants.MainPageRef.Toolbar.Visibility = Visibility.Collapsed;
            MediaRow.Height = new GridLength();
            Scrollviewer.ChangeView(0, 0, 1, true);
            Scrollviewer.VerticalScrollMode = ScrollMode.Disabled;
            Scrollviewer.VerticalScrollBarVisibility = ScrollBarVisibility.Disabled;

            //Hide the close and minimize controls
            MinimizeMediaElement.Visibility = Visibility.Collapsed;
            CloseMediaElement.Visibility = Visibility.Collapsed;
        }

        private void viewer_ExitingFullscren(object sender, EventArgs e)
        {
            Constants.MainPageRef.Toolbar.Visibility = Visibility.Visible;

            Scrollviewer.VerticalScrollMode = ScrollMode.Enabled;
            Scrollviewer.VerticalScrollBarVisibility = ScrollBarVisibility.Visible;

            Windows.Storage.ApplicationDataContainer localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
            if (localSettings.Values["MediaViewerHeight"] != null && (double)localSettings.Values["MediaViewerHeight"] > 360)
                MediaRow.Height = new GridLength(Convert.ToDouble(localSettings.Values["MediaViewerHeight"]));
            else
                MediaRow.Height = new GridLength(600);

            //Show the close and minimize controls
            MinimizeMediaElement.Visibility = Visibility.Visible;
            CloseMediaElement.Visibility = Visibility.Visible;
        }

        #endregion

        #region Autoplay
        private void SwitchAutoplay_Toggled(object sender, RoutedEventArgs e)
        {
            Windows.Storage.ApplicationDataContainer localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
            localSettings.Values["Autoplay"] = SwitchAutoplay.IsOn;
        }

        private async void VideoPlayer_MediaEnded(object sender, RoutedEventArgs e)
        {
            Windows.Storage.ApplicationDataContainer localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
            try
            {
                if ((bool)localSettings.Values["Autoplay"])
                {
                    Constants.activeVideoID = ((YoutubeItemDataType)RelatedVideosGridView.Items[0]).Id;

                    //Reload the page
                    await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => {
                        //Get the video data and play it
                        StartVideo(Constants.activeVideoID);

                        //Update likes/dislikes
                        LikeDislikeControl.UpdateData();
                    });
                }
            }
            catch { }
        }
        #endregion

        private async void CustomMediaTransportControls_SwitchedToFullSize(object sender, EventArgs e)
        {
            //Make the toolbar visible
            Constants.MainPageRef.Toolbar.Visibility = Visibility.Visible;

            //Stop the PiP media element
            pipViewer.Visibility = Visibility.Collapsed;
            pipViewer.Source = null;

            //Set the window to be full sized
            await ApplicationView.GetForCurrentView().TryEnterViewModeAsync(ApplicationViewMode.Default);

            //Reset title to bar to it's normal state
            var coreTitleBar = Windows.ApplicationModel.Core.CoreApplication.GetCurrentView().TitleBar;
            coreTitleBar.ExtendViewIntoTitleBar = false;

            //Set the position of this video page's viewer to the position of the compact view's one and the volume
            viewer.controller.Start(pipViewer.Position);
        }

        #region PiP

        private async void viewer_EnteringPiP(object sender, EventArgs e)
        {
            //Enter compact mode
            ViewModePreferences compactOptions = ViewModePreferences.CreateDefault(ApplicationViewMode.CompactOverlay);
            compactOptions.CustomSize = new Windows.Foundation.Size(500, 281);
            await ApplicationView.GetForCurrentView().TryEnterViewModeAsync(ApplicationViewMode.CompactOverlay, compactOptions);

            //Pause the video
            viewer.controller.Pause();

            //Make the title bar smaller
            var coreTitleBar = Windows.ApplicationModel.Core.CoreApplication.GetCurrentView().TitleBar;
            coreTitleBar.ExtendViewIntoTitleBar = true;

            //Make the toolbar collapsed
            Constants.MainPageRef.Toolbar.Visibility = Visibility.Collapsed;

            pipViewer.Visibility = Visibility.Visible;
            pipViewer.Source = new Uri(Constants.videoInfo.Muxed[0].Url);
            pipViewer.MediaOpened += PipViewer_MediaOpened;
        }

        private void PipViewer_MediaOpened(object sender, RoutedEventArgs e)
        {
            //Sets position to the viewer's position once it loads
            pipViewer.Position = viewer.controller.audioPlayer.PlaybackSession.Position;
        }

        #endregion

        #region Comments
        string commentNextPageToken;
        bool isAdding = false;

        private async void UpdateComments(CommentThreadsResource.ListRequest.OrderEnum option)
        {
            //Clear the current comments
            commentCollection.Clear();

            var methods = new YoutubeMethods();

            //Get the comments
            var service = YoutubeMethodsStatic.GetServiceNoAuth();
            var getComments = service.CommentThreads.List("snippet,replies");
            getComments.VideoId = Constants.activeVideoID;
            getComments.Order = option;
            getComments.TextFormat = CommentThreadsResource.ListRequest.TextFormatEnum.PlainText;
            getComments.MaxResults = 10;
            var response = await getComments.ExecuteAsync();

            //Save the next page token
            commentNextPageToken = response.NextPageToken;

            //Add them to our collection so that they will be updated on the UI
            foreach (var commentThread in response.Items)
            {
                commentCollection.Add(new CommentContainerDataType(methods.CommentToDataType(commentThread)));
            }
        }

        private async void AddComments(CommentThreadsResource.ListRequest.OrderEnum option)
        {
            if (isAdding)
                return;

            isAdding = true;

            //Check if there are more comments
            if (commentNextPageToken == null || commentNextPageToken == "")
                return;

            var methods = new YoutubeMethods();

            //Get the comments
            var service = YoutubeMethodsStatic.GetServiceNoAuth();
            var getComments = service.CommentThreads.List("snippet,replies");
            getComments.VideoId = Constants.activeVideoID;
            getComments.Order = option;
            getComments.TextFormat = CommentThreadsResource.ListRequest.TextFormatEnum.PlainText;
            getComments.PageToken = commentNextPageToken;
            getComments.MaxResults = 15;
            var response = await getComments.ExecuteAsync();

            //Save the next page token
            commentNextPageToken = response.NextPageToken;

            //Add them to our collection so that they will be updated on the UI
            foreach (var commentThread in response.Items)
            {
                commentCollection.Add(new CommentContainerDataType(methods.CommentToDataType(commentThread)));
            }

            isAdding = false;
        }

        private void CommentsOptionComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CommentsOptionComboBox.SelectedIndex == 0)
                UpdateComments(CommentThreadsResource.ListRequest.OrderEnum.Relevance);
            else
                UpdateComments(CommentThreadsResource.ListRequest.OrderEnum.Time);
        }

        /// <summary>
        /// Checks if we need to add more comments to the end of the page if the user has scrolled that far
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Scrollviewer_ViewChanged(object sender, ScrollViewerViewChangedEventArgs e)
        {
            if (Scrollviewer.VerticalOffset > Scrollviewer.ScrollableHeight - 700)
            {
                if (CommentsOptionComboBox.SelectedIndex == 0)
                    AddComments(CommentThreadsResource.ListRequest.OrderEnum.Relevance);
                else
                    AddComments(CommentThreadsResource.ListRequest.OrderEnum.Time);
            }
        }
        #endregion
    }
}

using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.YouTube.v3;
using Google.Apis.YouTube.v3.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using VideoLibrary;
using Windows.ApplicationModel.DataTransfer;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.Core;
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
    public sealed partial class VideoPage : Page
    {
        MainPage MainPageReference;
        string VideoID;
        Google.Apis.YouTube.v3.Data.Video video;
        Channel channel;
        SearchListResponse relatedVideos;

        public VideoPage()
        {
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            NavigateParams result = (NavigateParams)e.Parameter;
            base.OnNavigatedTo(e);
            MainPageReference = result.mainPageRef;
            this.InitializeComponent();
            MainPageReference.contentFrame.Navigated += ContentFrame_Navigated;
            if (Frame.Width == 640)
            {
                Scrollviewer.VerticalScrollMode = ScrollMode.Disabled;
                MediaElementContainer.MaxHeight = 360;
            }
            VideoID = result.ID;
            StartVideo(result.ID);
            GetVideoInfo(result.ID);
        }

        private void ContentFrame_Navigated(object sender, NavigationEventArgs e)
        {
            if(Frame.Width != 640 && Frame.Visibility == Visibility.Visible)
            {
                Frame.Width = 640;
                Frame.Height = 360;
                MediaElementContainer.MaxHeight = 360;
                Scrollviewer.VerticalScrollMode = ScrollMode.Disabled;
            }
        }

        #region Methods
        public void StartVideo(string ID)
        {
            try
            {
                var youTube = YouTube.Default;
                var video = youTube.GetVideo("https://www.youtube.com/watch?v=" + ID);
                viewer.Source = new Uri(video.GetUri());
                viewer.Visibility = Visibility.Visible;
                viewer.TransportControls.Focus(FocusState.Programmatic);
            }
            catch { }
        }

        public async void GetVideoInfo(string ID)
        {
            var credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(new ClientSecrets
            {
                ClientId = "957928808020-pa0lopl3crh565k6jd4djaj36rm1d9i5.apps.googleusercontent.com",
                ClientSecret = "oB9U6yWFndnBqLKIRSA0nYGm"
            }, new[] { YouTubeService.Scope.Youtube }, "user", CancellationToken.None);

            // Create the service.
            var service =  new YouTubeService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = "Youtube Viewer",
            });

            var getVideoInfo = service.Videos.List("snippet, statistics, contentDetails");
            getVideoInfo.Id = ID;
            var videoList = await getVideoInfo.ExecuteAsync();
            video = videoList.Items[0];

            var getChannelInfo = service.Channels.List("snippet");
            getChannelInfo.Id = video.Snippet.ChannelId;
            var channelInfo = await getChannelInfo.ExecuteAsync();
            channel = channelInfo.Items[0];

            var getRelatedVideos = service.Search.List("snippet");
            getRelatedVideos.RelatedToVideoId = VideoID;
            getRelatedVideos.MaxResults = 15;
            getRelatedVideos.Type = "video";
            relatedVideos = await getRelatedVideos.ExecuteAsync();

            UpdatePageInfo();

            UpdateRelatedVideos();
        }

        public void UpdatePageInfo()
        {
            var methods = new YoutubeItemMethods();

            Title.Text = video.Snippet.Title;
            Views.Text = video.Statistics.ViewCount + " Views";
            LikeCount.Text = methods.ViewCountShortner(video.Statistics.LikeCount);
            DislikeCount.Text = methods.ViewCountShortner(video.Statistics.DislikeCount);

            var likeDislikeRatio = Convert.ToDecimal(video.Statistics.LikeCount) / Convert.ToDecimal(video.Statistics.DislikeCount + video.Statistics.LikeCount);
            LikesBar.Value = Convert.ToDouble(likeDislikeRatio * 100);

            ChannelTitle.Text = channel.Snippet.Title;
            DatePosted.Text = video.Snippet.PublishedAt.Value.ToString("MMMM d, yyyy");
            Description.Text = video.Snippet.Description;
            var image = new BitmapImage(new Uri(channel.Snippet.Thumbnails.High.Url));
            var imageBrush = new ImageBrush();
            imageBrush.ImageSource = image;
            ChannelProfileIcon.Fill = imageBrush;
        }

        public void UpdateRelatedVideos()
        {
            List<YoutubeItemDataType> relatedVideosList = new List<YoutubeItemDataType>();

            var methods = new YoutubeItemMethods();
            foreach(SearchResult video in relatedVideos.Items)
            {
                relatedVideosList.Add(methods.VideoToYoutubeItem(video));
            }

            RelatedVideosGridView.ItemsSource = relatedVideosList;
        }

        private void YoutubeItemsGridView_ItemClick(object sender, ItemClickEventArgs e)
        {
            var item = (YoutubeItemDataType)e.ClickedItem;
            MainPageReference.StartVideo(item.Id);
        }

        public void ChangePlayerSize()
        {
            if (Frame.Width != 640)
            {
                Frame.Width = 640;
                Frame.Height = 360;
                MediaElementContainer.MaxHeight = 360;
                Scrollviewer.VerticalScrollMode = ScrollMode.Disabled;
            }
            else
            {
                Frame.Width = Double.NaN;
                Frame.Height = Double.NaN;
                MediaElementContainer.MaxHeight = 700;
                Scrollviewer.VerticalScrollMode = ScrollMode.Auto;
            }
        }

        private void Storyboard_Completed(object sender, object e)
        {
            MediaElementContainer.Height = Double.NaN;
            MediaElementContainer.Width = Double.NaN;
        }

        #endregion

        #region Events


        private void MinimizeMediaElement_Click(object sender, RoutedEventArgs e)
        {
            ChangePlayerSize();
        }

        private void CloseMediaElement_Click(object sender, RoutedEventArgs e)
        {
            viewer.Stop();
            Frame.Visibility = Visibility.Collapsed;
        }

        private void viewer_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            viewer.IsFullWindow = !viewer.IsFullWindow;
            if (viewer.CurrentState == Windows.UI.Xaml.Media.MediaElementState.Playing)
            {
                viewer.Pause();
            }
            else
            {
                viewer.Play();
            }
        }

        private void Event_KeyDown(object sender, KeyEventArgs e)
        {
            if (viewer.IsFullWindow && e.VirtualKey == Windows.System.VirtualKey.Escape) { viewer.IsFullWindow = false; }
            if (viewer.CurrentState == Windows.UI.Xaml.Media.MediaElementState.Playing && e.VirtualKey == Windows.System.VirtualKey.Space && viewer.Visibility == Visibility.Visible)
            {
                viewer.Pause();
                e.Handled = true;
            }
            else if (e.VirtualKey == Windows.System.VirtualKey.Space && viewer.Visibility == Visibility.Visible)
            {
                viewer.Play();
                e.Handled = true;
            }
        }

        private void viewer_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (viewer.CurrentState == Windows.UI.Xaml.Media.MediaElementState.Playing)
            {
                viewer.Pause();
            }
            else
            {
                viewer.Play();
            }
        }

        private void viewer_RightTapped(object sender, RightTappedRoutedEventArgs e)
        {
            var tappedItem = (UIElement)e.OriginalSource;
            var attachedFlyout = (MenuFlyout)FlyoutBase.GetAttachedFlyout(viewer.TransportControls);

            attachedFlyout.ShowAt(tappedItem, e.GetPosition(tappedItem));
        }
        #endregion

        #region MediaElementButton Management
        private void viewer_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            MinimizeMediaElement.Visibility = Visibility.Visible;
            FadeIn.Begin();
        }

        private void viewer_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            FadeOut.Completed += MediaButtonCompleted;
            FadeOut.Begin();
        }

        private void MediaButtonCompleted(object sender, object e)
        {
            if (MinimizeMediaElement.Opacity == 0) { MinimizeMediaElement.Visibility = Visibility.Collapsed; }
        }
        #endregion

        #region Flyout

        private void Flyout_CopyLink(object sender, RoutedEventArgs e)
        {
            var dataPackage = new DataPackage();
            dataPackage.SetText("https://youtu.be/" + VideoID);
            Clipboard.SetContent(dataPackage);
        }

        private void Flyout_CopyLinkAtTime(object sender, RoutedEventArgs e)
        {
            var dataPackage = new DataPackage();
            dataPackage.SetText("https://youtu.be/" + VideoID + "?t=" + Convert.ToInt32(viewer.Position.TotalSeconds) + "s");
            Clipboard.SetContent(dataPackage);
        }

        #endregion

        private async void LikeIcon_Tapped(object sender, TappedRoutedEventArgs e)
        {
            var credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(new ClientSecrets
            {
                ClientId = "957928808020-pa0lopl3crh565k6jd4djaj36rm1d9i5.apps.googleusercontent.com",
                ClientSecret = "oB9U6yWFndnBqLKIRSA0nYGm"
            }, new[] { YouTubeService.Scope.Youtube }, "user", CancellationToken.None);

            // Create the service.
            var service = new YouTubeService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = "Youtube Viewer",
            });

            var LikeVideo = service.Videos.Rate(VideoID, VideosResource.RateRequest.RatingEnum.Like);
            LikeVideo.ExecuteAsync();

            LikeIcon.Foreground = new SolidColorBrush(Color.FromArgb(255, 190, 40, 40));
            DislikeIcon.Foreground = new SolidColorBrush(Color.FromArgb(255, 255, 255, 255));
        }

        private async void DislikeIcon_Tapped(object sender, TappedRoutedEventArgs e)
        {
            var credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(new ClientSecrets
            {
                ClientId = "957928808020-pa0lopl3crh565k6jd4djaj36rm1d9i5.apps.googleusercontent.com",
                ClientSecret = "oB9U6yWFndnBqLKIRSA0nYGm"
            }, new[] { YouTubeService.Scope.Youtube }, "user", CancellationToken.None);

            // Create the service.
            var service = new YouTubeService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = "Youtube Viewer",
            });

            var DislikeVideo = service.Videos.Rate(VideoID, VideosResource.RateRequest.RatingEnum.Dislike);
            DislikeVideo.ExecuteAsync();

            DislikeIcon.Foreground = new SolidColorBrush(Color.FromArgb(255, 190, 40, 40));
            LikeIcon.Foreground = new SolidColorBrush(Color.FromArgb(255, 255, 255, 255));
        }
    }
}

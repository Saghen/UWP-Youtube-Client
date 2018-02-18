using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.YouTube.v3;
using Google.Apis.YouTube.v3.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using System.Threading.Tasks;
using VideoLibrary;
using Windows.ApplicationModel;
using Windows.ApplicationModel.DataTransfer;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Networking.BackgroundTransfer;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.ViewManagement;
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
using YTApp.UserControls;

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

        PlaylistDataType relatedVideos = new PlaylistDataType();

        public VideoPage()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            NavigateParams result = (NavigateParams)e.Parameter;
            base.OnNavigatedTo(e);
            MainPageReference = result.MainPageRef;

            var LikeDislike = new LikeDislikeUserControl(result.ID);

            TitleGrid.Children.Add(LikeDislike);
            Grid.SetColumn(LikeDislike, 1);

            MainPageReference.contentFrame.Navigated += ContentFrame_Navigated;
            SystemNavigationManager.GetForCurrentView().BackRequested += VideoPage_BackRequested; ;

            //Store the video ID for future use
            VideoID = result.ID;

            //Get the video data and play it
            StartVideo(result.ID);

            MainPageReference.SwitchToFullSize += CustomMediaTransportControls_SwitchedToFullSize;
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

            try
            {
                var youTube = YouTube.Default;
                var videos = await youTube.GetAllVideosAsync("https://www.youtube.com/watch?v=" + ID);
                var maxRes = videos.OrderByDescending(v => v.Resolution).FirstOrDefault();
                viewer.Source = new Uri(maxRes.GetUri());
                viewer.Visibility = Visibility.Visible;
                viewer.TransportControls.Focus(FocusState.Programmatic);
            }
            catch
            {
                InAppNotif.Show();
                return;
            }

            var service = await YoutubeItemMethodsStatic.GetServiceAsync();

            var getVideoInfo = service.Videos.List("snippet, statistics, contentDetails");
            getVideoInfo.Id = ID;
            var videoList = await getVideoInfo.ExecuteAsync();

            //Checks to see if video exists and sets the video variable if it does or returns if it doesn't
            try { video = videoList.Items[0]; }
            catch
            {
                InAppNotif.Show();
                return;
            }

            //Channel Info
            await Task.Run(() =>
            {
                var getChannelInfo = service.Channels.List("snippet");
                getChannelInfo.Id = video.Snippet.ChannelId;
                var channelInfo = getChannelInfo.Execute();
                channel = channelInfo.Items[0];
            });

            UpdatePageInfo(service);

            UpdateRelatedVideos(service);

        }

        private async void InAppNotifButton_Click(object sender, RoutedEventArgs e)
        {
            await Windows.System.Launcher.LaunchUriAsync(new Uri("https://youtu.be/" + VideoID));
            InAppNotif.Dismiss();
        }

        private void InAppNotifButton2_Click(object sender, RoutedEventArgs e)
        {
            InAppNotif.Dismiss();
        }

        public void UpdatePageInfo(YouTubeService service)
        {
            var methods = new YoutubeItemMethods();

            Title.Text = video.Snippet.Title;
            Views.Text = string.Format("{0:#,###0.#}", video.Statistics.ViewCount) + " Views";

            ChannelTitle.Text = channel.Snippet.Title;
            DatePosted.Text = video.Snippet.PublishedAt.Value.ToString("MMMM d, yyyy");
            Description.Text = video.Snippet.Description;
            DescriptionShowMore.Visibility = Visibility.Visible;
            var image = new BitmapImage(new Uri(channel.Snippet.Thumbnails.High.Url));
            var imageBrush = new ImageBrush();
            imageBrush.ImageSource = image;
            ChannelProfileIcon.Fill = imageBrush;

        }

        public async void UpdateRelatedVideos(YouTubeService service)
        {
            System.Collections.ObjectModel.ObservableCollection<YoutubeItemDataType> relatedVideosList = new System.Collections.ObjectModel.ObservableCollection<YoutubeItemDataType>();
            await System.Threading.Tasks.Task.Run(() =>
            {
                var getRelatedVideos = service.Search.List("snippet");
                getRelatedVideos.RelatedToVideoId = VideoID;
                getRelatedVideos.MaxResults = 15;
                getRelatedVideos.Type = "video";
                var relatedVideosResponse = getRelatedVideos.Execute();

                var methods = new YoutubeItemMethods();
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
            MainPageReference.StartVideo(item.Id);
        }

        //Another version of the ChangePlayerSize that takes a bool allowing you to set it to fullscreen (true) or to a small view (false)
        public void ChangePlayerSize(bool MakeFullScreen)
        {
            if (!MakeFullScreen)
            {
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
            }
            else
            {
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
            viewer.Stop();
            Frame.Visibility = Visibility.Collapsed;
        }

        private void viewer_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            viewer.IsFullWindow = !viewer.IsFullWindow;
        }

        private void viewer_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (viewer.IsFullWindow && e.Key == Windows.System.VirtualKey.Escape) { viewer.IsFullWindow = false; }
            if (viewer.CurrentState == Windows.UI.Xaml.Media.MediaElementState.Playing && e.Key == Windows.System.VirtualKey.Space && viewer.Visibility == Visibility.Visible)
            {
                viewer.Pause();
                e.Handled = true;
            }
            else if (e.Key == Windows.System.VirtualKey.Space && viewer.Visibility == Visibility.Visible)
            {
                viewer.Play();
                e.Handled = true;
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

        private async void Flyout_DownloadVideo(object sender, RoutedEventArgs e)
        {
            var savePicker = new Windows.Storage.Pickers.FileSavePicker();
            savePicker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.Downloads;
            savePicker.FileTypeChoices.Add("Video File", new List<string>() { ".mp4" });
            savePicker.SuggestedFileName = video.Snippet.Title;

            Windows.Storage.StorageFile file = await savePicker.PickSaveFileAsync();
            if (file != null)
            {
                // Prevent updates to the remote version of the file until
                // we finish making changes and call CompleteUpdatesAsync.
                Windows.Storage.CachedFileManager.DeferUpdates(file);
                // write to file
                BackgroundDownloader downloader = new BackgroundDownloader();
                DownloadOperation download = downloader.CreateDownload(viewer.Source, file);
                await download.StartAsync();
            }
        }

        #endregion

        private void CustomMediaTransportControls_SwitchedToCompact(object sender, EventArgs e)
        {
            MainPageReference.viewer.Source = viewer.Source;
            MainPageReference.viewer.Visibility = Visibility.Visible;
            MainPageReference.viewer.MediaOpened += MainPageViewer_MediaOpened;
            var coreTitleBar = Windows.ApplicationModel.Core.CoreApplication.GetCurrentView().TitleBar;
            coreTitleBar.ExtendViewIntoTitleBar = true;
        }

        private void MainPageViewer_MediaOpened(object sender, RoutedEventArgs e)
        {
            MainPageReference.viewer.Position = viewer.Position;
            viewer.Source = new Uri("about:blank");
        }

        private void CustomMediaTransportControls_SwitchedToFullSize(object sender, EventArgs e)
        {
            viewer.Source = MainPageReference.viewer.Source;
            MainPageReference.viewer.Visibility = Visibility.Collapsed;
            viewer.MediaOpened += Viewer_MediaOpened;
            var coreTitleBar = Windows.ApplicationModel.Core.CoreApplication.GetCurrentView().TitleBar;
            coreTitleBar.ExtendViewIntoTitleBar = false;
        }

        private void Viewer_MediaOpened(object sender, RoutedEventArgs e)
        {
            viewer.Position = MainPageReference.viewer.Position;
            MainPageReference.viewer.Source = new Uri("about:blank");
        }

        private void OpenChannel(object sender, TappedRoutedEventArgs e)
        {
            MainPageReference.contentFrame.Navigate(typeof(ChannelPage), new NavigateParams() { MainPageRef = MainPageReference, ID = video.Snippet.ChannelId });
        }

        private void ChannelProfileIcon_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            Window.Current.CoreWindow.PointerCursor = new Windows.UI.Core.CoreCursor(Windows.UI.Core.CoreCursorType.Hand, 1);
        }

        private void ChannelProfileIcon_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            Window.Current.CoreWindow.PointerCursor = new Windows.UI.Core.CoreCursor(Windows.UI.Core.CoreCursorType.Arrow, 2);
        }

        private void DescriptionShowMore_Click(object sender, RoutedEventArgs e)
        {
            if ((string)DescriptionShowMore.Content == "Show less")
            {
                Description.MaxLines = 6;
                DescriptionShowMore.Content = "Show more";
            }
            else
            {
                Description.MaxLines = 400;
                DescriptionShowMore.Content = "Show less";
            }
        }
    }
}

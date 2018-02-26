using System;
using System.Threading;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Media;
using Windows.Media.Core;
using Windows.Media.Playback;
using Windows.UI.Core;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Input;
using YoutubeExplode;
using YoutubeExplode.Models.MediaStreams;
using YTApp.Classes;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace YTApp.UserControls
{
    public sealed partial class VideoViewer : UserControl
    {
        public event EventHandler EnteringFullscreen;

        public event EventHandler ExitingFullscren;

        public event EventHandler EnteringPiP;

        private static readonly DependencyProperty SourceProperty = DependencyProperty.Register("Source", typeof(string), typeof(VideoViewer), null);

        private MediaStreamInfoSet videoStreams;

        public MediaPlayerController controller = new MediaPlayerController();

        //Timer that will update our progress slider on our custom controls
        public DispatcherTimer timer = new DispatcherTimer();

        //Data used for control fading
        private Point previousMouseLocation;

        private int mouseHasntMoved = 0;
        private DispatcherTimer pointerCheckTimer = new DispatcherTimer();

        //Timer for storing the current position of the video in the cloud storage
        DispatcherTimer storePositionTimer = new DispatcherTimer();

        public string Source
        {
            get { return (string)GetValue(SourceProperty); }
            set { SetValue(SourceProperty, value); LoadVideo(); }
        }

        public VideoViewer()
        {
            this.InitializeComponent();

            //Update progress bar 30 times per second
            timer.Interval = new TimeSpan(0, 0, 0, 0, 32);
            timer.Tick += Timer_Tick;

            //Check if the mouse is over the viewer and control the transport controls accordingly
            pointerCheckTimer.Interval = new TimeSpan(0, 0, 0, 0, 100);
            pointerCheckTimer.Tick += PointerCheckTimer_Tick;

            controller.videoPlayer.PlaybackSession.PlaybackStateChanged += PlaybackSession_PlaybackStateChanged;

            //Update the position of the video in the cloud data store
            
            storePositionTimer.Interval = new TimeSpan(0, 0, 0, 10);
            storePositionTimer.Tick += StorePositionTimer_Tick;
        }

        private async void PlaybackSession_PlaybackStateChanged(MediaPlaybackSession sender, object args)
        {
            if(controller.videoPlayer.PlaybackSession.PlaybackState == MediaPlaybackState.Playing)
            {
                await Dispatcher.RunAsync(CoreDispatcherPriority.High, () => { ButtonPlay.Icon = new SymbolIcon() { Symbol = Symbol.Pause }; LoadingRing.IsActive = false; });
            }
            else if (controller.videoPlayer.PlaybackSession.PlaybackState == MediaPlaybackState.Buffering)
            {
                await Dispatcher.RunAsync(CoreDispatcherPriority.High, () => { LoadingRing.IsActive = true; });
            }
            else if (controller.videoPlayer.PlaybackSession.PlaybackState == MediaPlaybackState.Paused)
            {
                await Dispatcher.RunAsync(CoreDispatcherPriority.High, () => { ButtonPlay.Icon = new SymbolIcon() { Symbol = Symbol.Play }; LoadingRing.IsActive = false; });
            }
        }

        #region Transport Control Management

        private void ButtonPlay_Click(object sender, RoutedEventArgs e)
        {
            if (controller.videoPlayer.PlaybackSession.PlaybackState == MediaPlaybackState.Playing || controller.videoPlayer.PlaybackSession.PlaybackState == MediaPlaybackState.Buffering)
                controller.Pause();
            else
                controller.Start();
        }

        #region Picture in Picture

        private void ButtonPiP_Click(object sender, RoutedEventArgs e)
        {
            //Call event that the parent page has captured
            EnteringPiP.Invoke(this, new EventArgs());
        }

        #endregion Picture in Picture

        private void ButtonSettings_Click(object sender, RoutedEventArgs e)
        {
            ButtonSettings.Flyout.ShowAt(ButtonSettings);
        }

        private void ButtonFullscreen_Click(object sender, RoutedEventArgs e)
        {
            var view = ApplicationView.GetForCurrentView();
            if (view.IsFullScreenMode)
            {
                view.ExitFullScreenMode();
                ExitingFullscren.Invoke(this, new EventArgs());
            }
            else
            {
                view.TryEnterFullScreenMode();
                EnteringFullscreen.Invoke(this, new EventArgs());
            }
        }

        private void ButtonCopy_Tapped(object sender, TappedRoutedEventArgs e)
        {
            var link = new Windows.ApplicationModel.DataTransfer.DataPackage();
            link.SetText("https://youtu.be/" + Constants.activeVideoID + "?t=" + Convert.ToInt32(controller.audioPlayer.PlaybackSession.Position.TotalSeconds) + "s");
            Windows.ApplicationModel.DataTransfer.Clipboard.SetContent(link);
            Constants.MainPageRef.ShowNotifcation("Link copied to clipboard.");
        }

        #region Manage Transport Control Fading

        private void PointerCheckTimer_Tick(object sender, object e)
        {
            try
            {
                if (previousMouseLocation != Window.Current.CoreWindow.PointerPosition)
                {
                    mouseHasntMoved = 0;
                    Window.Current.CoreWindow.PointerCursor = new CoreCursor(CoreCursorType.Arrow, 1);
                    if (transportControls.Opacity == 0)
                        FadeIn.Begin();
                }
                else if (mouseHasntMoved == 20 && transportControls.Opacity == 1)
                {
                    FadeOut.Begin();
                    Window.Current.CoreWindow.PointerCursor = null;
                }
                mouseHasntMoved += 1;
                previousMouseLocation = Window.Current.CoreWindow.PointerPosition;
            }
            catch { }
        }

        private void MediaViewerParent_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            FadeIn.Begin();
        }

        private void MediaViewerParent_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            FadeOut.Begin();
        }

        private void viewer_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            pointerCheckTimer.Start();
        }

        private void viewer_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            pointerCheckTimer.Stop();
        }

        #endregion Manage Transport Control Fading

        #region Slider

        private async void Timer_Tick(object sender, object e)
        {
            try
            {
                if (Window.Current.CoreWindow.GetKeyState(Windows.System.VirtualKey.LeftButton).HasFlag(CoreVirtualKeyStates.Down))
                    return;
                await Dispatcher.RunAsync(CoreDispatcherPriority.High, () =>
                {
                    if(controller.audioPlayer.PlaybackSession.NaturalDuration.TotalSeconds > 0)
                        viewerProgress.Value = (controller.audioPlayer.PlaybackSession.Position.TotalSeconds / controller.audioPlayer.PlaybackSession.NaturalDuration.TotalSeconds) * 10000;
                });
            }
            catch { }
        }

        private void viewerProgress_SliderOnComplete(object sender, PointerRoutedEventArgs e)
        {
            //Set new position to the one that was just selected
            controller.SetPosition(new TimeSpan(0, 0, 0, Convert.ToInt32((viewerProgress.Value / 10000) * controller.audioPlayer.PlaybackSession.NaturalDuration.TotalSeconds)));
        }

        #endregion Slider

        #region Quality Button

        private void QualityList_ItemClick(object sender, ItemClickEventArgs e)
        {
            controller.videoPlayer.Source = MediaSource.CreateFromUri(new Uri(YoutubeMethodsStatic.GetVideoQuality((VideoQuality)e.ClickedItem, false)));
            ButtonSettings.Flyout.Hide();
        }

        #endregion Quality Button

        #region Volume Button

        private void VolumeSlider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            try
            {
                controller.audioPlayer.Volume = ((Slider)sender).Value / 1000;
            }
            catch { }
        }

        private void ButtonVolume_Click(object sender, RoutedEventArgs e)
        {
            ButtonVolume.Flyout.ShowAt(ButtonVolume);
        }

        #endregion Volume Button

        #region Viewer Events

        private void viewer_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (controller.audioPlayer.PlaybackSession.PlaybackState == MediaPlaybackState.Playing)
                controller.Pause();
            else
                controller.Start();
        }

        private void viewer_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            var view = ApplicationView.GetForCurrentView();
            if (view.IsFullScreenMode)
            {
                view.ExitFullScreenMode();
                ExitingFullscren.Invoke(this, new EventArgs());
            }
            else
            {
                view.TryEnterFullScreenMode();
                EnteringFullscreen.Invoke(this, new EventArgs());
            }

            //The first tap will pause the player
            controller.Start();
        }

        private void MediaViewerParent_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            var view = ApplicationView.GetForCurrentView();
            if (e.Key == Windows.System.VirtualKey.Escape && view.IsFullScreenMode)
            {
                view.ExitFullScreenMode();
                ExitingFullscren.Invoke(this, new EventArgs());
            }
        }

        #endregion Viewer Events

        #region Download Video Event

        private void DownloadVideo_Tapped(object sender, TappedRoutedEventArgs e)
        {
            Constants.MainPageRef.DownloadVideo();
        }

        #endregion Download Video Event

        #endregion Transport Control Management

        #region Video Source Management

        private async Task<bool> GetVideoData()
        {
            var client = new YoutubeClient();
            string id = Source;

            //Convert it to a regular ID if it is a youtube link
            try { id = YoutubeClient.ParseVideoId(Source); }
            catch { }

            //Get the video
            try { videoStreams = await client.GetVideoMediaStreamInfosAsync(id); }
            catch { return false; }

            //Store the video urls and info
            Constants.videoInfo = videoStreams;

            return true;
        }

        private async void LoadVideo()
        {
            if (!(await GetVideoData()))
                return;

            controller.Load(new Uri(Constants.videoInfo.Video[0].Url), new Uri(Constants.videoInfo.Audio[0].Url));

            viewer.SetMediaPlayer(controller.videoPlayer);

            controller.Start();

            timer.Start();
            storePositionTimer.Start();
        }

        public void StopVideo()
        {
            //Stop all timers
            timer.Stop();
            storePositionTimer.Stop();

            controller.Stop();
        }

        #endregion Video Source Management

        private void StorePositionTimer_Tick(object sender, object e)
        {
            //Save the percentage of the video watched
            Constants.syncedData.history[0].WatchTime = (controller.audioPlayer.PlaybackSession.Position.TotalSeconds / controller.audioPlayer.PlaybackSession.NaturalDuration.TotalSeconds) * 100;
        }
    }
}
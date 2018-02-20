using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Media;
using Windows.Media.Core;
using Windows.Media.Playback;
using Windows.Networking.BackgroundTransfer;
using Windows.UI.Core;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
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

        private static readonly DependencyProperty SourceProperty = DependencyProperty.Register( "Source", typeof(string), typeof(VideoViewer), null );

        MediaStreamInfoSet videoStreams;

        MediaPlayer audioPlayer = new MediaPlayer();
        MediaPlayer videoPlayer = new MediaPlayer();
        public MediaTimelineController timelineController = new MediaTimelineController();

        //Timer that will update our progress slider on our custom controls
        DispatcherTimer timer = new DispatcherTimer();

        bool OverViewer = false;

        public string Source
        {
            get { return (string)GetValue(SourceProperty); }
            set { SetValue(SourceProperty, value); UpdateVideo(); }
        }

        public VideoViewer()
        {
            this.InitializeComponent();

            ((CustomMediaTransportControls)Constants.MainPageRef.viewer.TransportControls).SwitchedToFullSize += VideoViewer_SwitchedToFullSize;

            //Update progress bar 30 times per second
            timer.Interval = new TimeSpan(0, 0, 0, 0, 32);
            timer.Tick += Timer_Tick;
        }

        #region Transport Control Management

        private async void Timer_Tick(object sender, object e)
        {
            if (Window.Current.CoreWindow.GetKeyState(Windows.System.VirtualKey.LeftButton).HasFlag(CoreVirtualKeyStates.Down))
                return;
            await Dispatcher.RunAsync(CoreDispatcherPriority.High, () =>
            {
                viewerProgress.Value = videoPlayer.PlaybackSession.Position.TotalSeconds;
            });
        }

        private void ButtonPlay_Click(object sender, RoutedEventArgs e)
        {
            if(timelineController.State == MediaTimelineControllerState.Running)
            {
                timelineController.Pause();
                ButtonPlay.Icon = new SymbolIcon() { Symbol = Symbol.Play };
            }
            else
            {
                timelineController.Resume();
                ButtonPlay.Icon = new SymbolIcon() { Symbol = Symbol.Pause };
            }
        }

        private async void ButtonPiP_Click(object sender, RoutedEventArgs e)
        {
            ViewModePreferences compactOptions = ViewModePreferences.CreateDefault(ApplicationViewMode.CompactOverlay);
            compactOptions.CustomSize = new Size(500, 281);
            await ApplicationView.GetForCurrentView().TryEnterViewModeAsync(ApplicationViewMode.CompactOverlay, compactOptions);

            Constants.MainPageRef.viewer.Source = new Uri(videoStreams.Muxed[0].Url);
            Constants.MainPageRef.viewer.Position = timelineController.Position;
            Constants.MainPageRef.viewer.Visibility = Visibility.Visible;

            timelineController.Pause();

            var coreTitleBar = Windows.ApplicationModel.Core.CoreApplication.GetCurrentView().TitleBar;
            coreTitleBar.ExtendViewIntoTitleBar = true;
        }

        private void VideoViewer_SwitchedToFullSize(object sender, EventArgs e)
        {
            Constants.MainPageRef.viewer.Visibility = Visibility.Collapsed;

            //Reset title to bar to it's normal state
            var coreTitleBar = Windows.ApplicationModel.Core.CoreApplication.GetCurrentView().TitleBar;
            coreTitleBar.ExtendViewIntoTitleBar = false;

            //Set the position of this video page's viewer to the position of the compact view's one
            timelineController.Position = Constants.MainPageRef.viewer.Position;
            timelineController.Start();
        }

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

        private async void MediaViewerParent_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            FadeIn.Begin();
        }

        private void MediaViewerParent_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            FadeOut.Begin();
        }

        private async void viewer_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            OverViewer = true;
            await Task.Delay(2000);
            if (transportControls.Opacity == 1 && OverViewer)
                FadeOut.Begin();
        }

        private void viewer_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            OverViewer = false;
        }

        private void transportControls_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            if (transportControls.Opacity == 0)
                FadeIn.Begin();
        }

        private void viewerProgress_SliderOnComplete(object sender, PointerRoutedEventArgs e)
        {
            //Set new position to the one that was just selected
            timelineController.Position = new TimeSpan(0, 0, 0, 0, Convert.ToInt32(viewerProgress.Value * 1000));
        }

        private void QualityList_ItemClick(object sender, ItemClickEventArgs e)
        {
            videoPlayer.Source = MediaSource.CreateFromUri(new Uri(YoutubeItemMethodsStatic.GetVideoQuality((VideoQuality)e.ClickedItem, false)));
            ButtonSettings.Flyout.Hide();
        }

        #region Volume Button
        private void VolumeSlider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            audioPlayer.Volume = ((Slider)sender).Value/1000;
        }


        private void ButtonVolume_Click(object sender, RoutedEventArgs e)
        {
            ButtonVolume.Flyout.ShowAt(ButtonVolume);
        }

        #endregion


        #region Viewer Events
        private void viewer_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (timelineController.State == MediaTimelineControllerState.Running)
            {
                timelineController.Pause();
                ButtonPlay.Icon = new SymbolIcon() { Symbol = Symbol.Play };
            }
            else
            {
                timelineController.Resume();
                ButtonPlay.Icon = new SymbolIcon() { Symbol = Symbol.Pause };
            }
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
        }

        #endregion

        #region Download Video Event
        private async void DownloadVideo_Tapped(object sender, TappedRoutedEventArgs e)
        {
            var client = new YoutubeClient();
            var videoUrl = Constants.videoInfo.Muxed[0].Url;

            var savePicker = new Windows.Storage.Pickers.FileSavePicker();
            savePicker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.Downloads;
            savePicker.FileTypeChoices.Add("Video File", new List<string>() { ".mp4" });

            Windows.Storage.StorageFile file = await savePicker.PickSaveFileAsync();
            if (file != null)
            {
                // Prevent updates to the remote version of the file until
                // we finish making changes and call CompleteUpdatesAsync.
                Windows.Storage.CachedFileManager.DeferUpdates(file);
                // write to file
                BackgroundDownloader downloader = new BackgroundDownloader();
                DownloadOperation download = downloader.CreateDownload(new Uri(videoUrl), file);
                download.StartAsync();
            }
            else
            {
                //Log.Info("Download operation was cancelled.");
            }
        }
        #endregion
        #endregion

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

        private async void UpdateVideo()
        {
            if (!(await GetVideoData()))
                return;


            //We use this method so that we can synchronize the audio and video streams
            audioPlayer.Source = MediaSource.CreateFromUri(new Uri(Constants.videoInfo.Audio[0].Url));
            videoPlayer.Source = MediaSource.CreateFromUri(new Uri(Constants.videoInfo.Video[0].Url));

            audioPlayer.CommandManager.IsEnabled = false;
            videoPlayer.CommandManager.IsEnabled = false;

            audioPlayer.TimelineController = timelineController;
            videoPlayer.TimelineController = timelineController;

            viewer.SetMediaPlayer(videoPlayer);

            timelineController.Start();

            //Event that allows us to set the maximum progress bar value and start updating
            videoPlayer.MediaOpened += VideoPlayer_MediaOpened;

            //Update video qualities
            QualityList.ItemsSource = YoutubeItemMethodsStatic.GetVideoQualityList();
        }

        private async void VideoPlayer_MediaOpened(MediaPlayer sender, object args)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.High, () =>
            {
                viewerProgress.Maximum = videoPlayer.PlaybackSession.NaturalDuration.TotalSeconds;
                timer.Start();
            });
        }

        public void StopVideo()
        {
            //Stop updating the progress bar
            timer.Stop();

            //Remove the players from memory
            audioPlayer.Dispose();
            videoPlayer.Dispose();
        }
        #endregion
    }
}

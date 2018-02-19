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

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace YTApp.UserControls
{
    public sealed partial class VideoViewer : UserControl
    {
        private static readonly DependencyProperty SourceProperty = DependencyProperty.Register( "Source", typeof(string), typeof(VideoViewer), null );

        MediaStreamInfoSet videoStreams;

        MediaPlayer audioPlayer = new MediaPlayer();

        public string Source
        {
            get { return (string)GetValue(SourceProperty); }
            set { SetValue(SourceProperty, value); UpdateVideo(); }
        }

        public VideoViewer()
        {
            this.InitializeComponent();

            //Events for background audio playback
            viewer.CurrentStateChanged += Viewer_CurrentStateChanged;
            viewer.SeekCompleted += Viewer_SeekCompleted;
            viewer.VolumeChanged += Viewer_VolumeChanged;

            //Custom event that fires immediately after seeking
            var controls = (CustomMediaTransportControls)viewer.TransportControls;
            controls.SeekCompletedFast += Controls_SeekCompletedFast;

            //Checks to make sure the audio is synced
            DispatcherTimer checkAudioTimer = new DispatcherTimer();
            checkAudioTimer.Interval = new TimeSpan(0, 1, 0);
            checkAudioTimer.Tick += CheckAudioTimer_Tick;
            checkAudioTimer.Start();
        }

        private void CheckAudioTimer_Tick(object sender, object e)
        {
            if (Math.Round(audioPlayer.PlaybackSession.Position.TotalSeconds, 1) != Math.Round(viewer.Position.TotalSeconds, 1))
                audioPlayer.PlaybackSession.Position = viewer.Position;
        }

        #region Audio Events
        private void Controls_SeekCompletedFast(object sender, EventArgs e)
        {
            audioPlayer.Pause();
        }
        
        private void Viewer_VolumeChanged(object sender, RoutedEventArgs e)
        {
            audioPlayer.Volume = viewer.Volume;
        }

        private void Viewer_SeekCompleted(object sender, RoutedEventArgs e)
        {
            audioPlayer.PlaybackSession.Position = viewer.Position;
            audioPlayer.Play();
        }

        private async void Viewer_CurrentStateChanged(object sender, RoutedEventArgs e)
        {
            switch (viewer.CurrentState)
            {
                case MediaElementState.Playing:
                    await Task.Delay(200);
                    audioPlayer.PlaybackSession.Position = viewer.Position;
                    audioPlayer.Play();
                    break;
                default:
                    audioPlayer.PlaybackSession.Position = viewer.Position;
                    audioPlayer.Pause();
                    break;
            }
        }
        #endregion

        #region Video Source Management

        private async void UpdateVideo()
        {
            viewerAudio.SetMediaPlayer(null);

            var client = new YoutubeClient();
            string id = Source;

            //Convert it to a regular ID if it is a youtube link
            try { id = YoutubeClient.ParseVideoId(Source); }
            catch { }
            
            try
            {
                videoStreams = await client.GetVideoMediaStreamInfosAsync(id);
            }
            catch { return; }

            audioPlayer.Source = MediaSource.CreateFromUri(new Uri(videoStreams.Audio[0].Url));
            audioPlayer.PlaybackSession.PlaybackRate = 0.9983; // <-- Compensate for it being slightly too fast and causing desync
            viewerAudio.SetMediaPlayer(audioPlayer);
            

            viewer.Source = new Uri(videoStreams.Video[2].Url);
        }

        public void StopVideo()
        {
            audioPlayer.Source = null;
            viewer.Source = null;
        }
        #endregion
    }
}

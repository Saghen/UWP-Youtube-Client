using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.Media.Core;
using Windows.Media.Playback;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace YTApp.Classes
{
    public class MediaPlayerController
    {
        public MediaElement videoPlayer;
        public MediaPlayer audioPlayer = new MediaPlayer() { AutoPlay = false, AudioCategory = MediaPlayerAudioCategory.Media };

        bool videoPlayerOpened = false;
        bool audioPlayerOpened = false;

        public MediaPlayerController(MediaElement mediaElmnt)
        {
            videoPlayer = mediaElmnt;

            videoPlayer.CurrentStateChanged += VideoPlayer_CurrentStateChanged;

            audioPlayer.MediaOpened += AudioPlayer_MediaOpened;
            videoPlayer.MediaOpened += VideoPlayer_MediaOpened;

            SynchronizePlayers();
        }

        private void VideoPlayer_CurrentStateChanged(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            if (videoPlayer.CurrentState == MediaElementState.Playing && audioPlayer.PlaybackSession.PlaybackState == MediaPlaybackState.Paused)
                audioPlayer.Play();
            else if (videoPlayer.CurrentState != MediaElementState.Playing)
                audioPlayer.Pause();
        }

        private void AudioPlayer_MediaOpened(MediaPlayer sender, object args)
        {
            audioPlayerOpened = true;
        }

        private void VideoPlayer_MediaOpened(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            videoPlayerOpened = true;
        }

        public void Load(Uri videoLink, Uri audioLink)
        {
            audioPlayerOpened = false;
            videoPlayerOpened = false;
            videoPlayer.Source = videoLink;
            audioPlayer.Source = MediaSource.CreateFromUri(audioLink);
        }

        public async void Start()
        {
            while(!audioPlayerOpened || !videoPlayerOpened)
            {
                await Task.Delay(50);
            }

            videoPlayer.Play();
            audioPlayer.Play();
        }

        public async void Start(TimeSpan time)
        {
            while (!audioPlayerOpened || !videoPlayerOpened)
            {
                await Task.Delay(50);
            }

            videoPlayer.Position = time;
            audioPlayer.PlaybackSession.Position = time;

            Start();
        }

        public void Stop()
        {
            audioPlayer.Dispose();

            videoPlayer.Source = null;
            audioPlayer = new MediaPlayer();
        }

        public void Pause()
        {
            videoPlayer.Pause();
            audioPlayer.Pause();
        }

        public void SetPosition(TimeSpan time)
        {
            videoPlayer.Position = time;
            audioPlayer.PlaybackSession.Position = time;
        }

        private async void SynchronizePlayers()
        {
            while(true)
            {
                if (audioPlayer.PlaybackSession.Position.TotalSeconds > videoPlayer.Position.TotalSeconds)
                {
                    var offsetAmount = audioPlayer.PlaybackSession.Position.TotalSeconds - videoPlayer.Position.TotalSeconds;
                    if(offsetAmount > 0.07)
                        videoPlayer.PlaybackRate = 1 + offsetAmount / 2;
                }
                else if (audioPlayer.PlaybackSession.Position.TotalSeconds < videoPlayer.Position.TotalSeconds)
                {
                    var offsetAmount = videoPlayer.Position.TotalSeconds - audioPlayer.PlaybackSession.Position.TotalSeconds;
                    if (offsetAmount > 0.07)
                        videoPlayer.PlaybackRate = 1 - offsetAmount / 2;
                }
                else
                {
                    if (videoPlayer.PlaybackRate != 1)
                        videoPlayer.PlaybackRate = 1;
                }

                await Task.Delay(500);
            }
        }
    }
}

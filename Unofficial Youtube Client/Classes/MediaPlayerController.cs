using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.Media.Core;
using Windows.Media.Playback;

namespace YTApp.Classes
{
    public class MediaPlayerController
    {
        public event EventHandler StateChanged;
        public event EventHandler MediaEnded;

        public MediaPlayer videoPlayer = new MediaPlayer() { AutoPlay = false, AudioCategory = MediaPlayerAudioCategory.Media };
        public MediaPlayer audioPlayer = new MediaPlayer() { AutoPlay = false, AudioCategory = MediaPlayerAudioCategory.Media };

        bool videoPlayerOpened = false;
        bool audioPlayerOpened = false;

        public MediaPlayerController()
        {
            videoPlayer.MediaOpened += VideoPlayer_MediaOpened;
            audioPlayer.MediaOpened += AudioPlayer_MediaOpened;

            videoPlayer.CurrentStateChanged += VideoPlayer_CurrentStateChanged;

            Task.Run(() => SynchronizePlayers());
        }

        private void VideoPlayer_CurrentStateChanged(MediaPlayer sender, object args)
        {
            if (videoPlayer.PlaybackSession.PlaybackState == MediaPlaybackState.Buffering)
                audioPlayer.Pause();
            else if (videoPlayer.PlaybackSession.PlaybackState == MediaPlaybackState.Playing && audioPlayer.PlaybackSession.PlaybackState == MediaPlaybackState.Paused)
                audioPlayer.Play();
        }

        private void AudioPlayer_MediaOpened(MediaPlayer sender, object args)
        {
            audioPlayerOpened = true;
            if (videoPlayerOpened)
                Start();
        }

        private void VideoPlayer_MediaOpened(MediaPlayer sender, object args)
        {
            videoPlayerOpened = true;
            if (audioPlayerOpened)
            {
                Start();
            }
                
        }

        public void Load(Uri videoLink, Uri audioLink)
        {
            videoPlayer.Source = MediaSource.CreateFromUri(videoLink);
            audioPlayer.Source = MediaSource.CreateFromUri(audioLink);
        }

        public void Start()
        {
            videoPlayer.Play();
            audioPlayer.Play();
        }

        public void Start(TimeSpan time)
        {
            videoPlayer.PlaybackSession.Position = time;
            audioPlayer.PlaybackSession.Position = time;

            Start();
        }

        public void Stop()
        {
            videoPlayer.Dispose();
            audioPlayer.Dispose();

            videoPlayer = new MediaPlayer();
            audioPlayer = new MediaPlayer();
        }

        public void Pause()
        {
            videoPlayer.Pause();
            audioPlayer.Pause();
        }

        public void SetPosition(TimeSpan time)
        {
            videoPlayer.PlaybackSession.Position = time;
            audioPlayer.PlaybackSession.Position = time;
        }

        private async void SynchronizePlayers()
        {
            while(true)
            {
                if(audioPlayer.PlaybackSession.Position.TotalSeconds > videoPlayer.PlaybackSession.Position.TotalSeconds)
                {
                    var offsetAmount = audioPlayer.PlaybackSession.Position.TotalSeconds - videoPlayer.PlaybackSession.Position.TotalSeconds;
                    if(offsetAmount > 0.07)
                        videoPlayer.PlaybackSession.PlaybackRate = 1 + offsetAmount / 2;
                }
                else if (audioPlayer.PlaybackSession.Position.TotalSeconds < videoPlayer.PlaybackSession.Position.TotalSeconds)
                {
                    var offsetAmount = videoPlayer.PlaybackSession.Position.TotalSeconds - audioPlayer.PlaybackSession.Position.TotalSeconds;
                    if (offsetAmount > 0.07)
                        videoPlayer.PlaybackSession.PlaybackRate = 1 - offsetAmount / 2;
                }
                else
                {
                    if (videoPlayer.PlaybackSession.PlaybackRate != 1)
                        videoPlayer.PlaybackSession.PlaybackRate = 1;
                }

                await Task.Delay(500);
            }
        }
    }
}

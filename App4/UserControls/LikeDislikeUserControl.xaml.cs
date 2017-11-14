using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.YouTube.v3;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using Windows.ApplicationModel;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace YTApp.UserControls
{
    public sealed partial class LikeDislikeUserControl : UserControl
    {
        string VideoID;

        public LikeDislikeUserControl(string VideoId)
        {
            this.InitializeComponent();
            VideoID = VideoId;
            CheckIfAlreadyLikedOrDisliked();
        }

        public async void CheckIfAlreadyLikedOrDisliked()
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

            var videoRequest = service.Videos.GetRating(VideoID);
            var video = await videoRequest.ExecuteAsync();

            if (video.Items[0].Rating == "like")
            {
                LikeIcon.Fill = new SolidColorBrush(Color.FromArgb(255, 200, 200, 200));
            }
            else if (video.Items[0].Rating == "dislike")
            {
                DislikeIcon.Fill = new SolidColorBrush(Color.FromArgb(255, 200, 200, 200));
            }
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

            LikeIcon.Fill = new SolidColorBrush(Color.FromArgb(255, 136, 136, 136));
            DislikeIcon.Fill = new SolidColorBrush(Color.FromArgb(255, 200, 200, 200));
        }

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

            LikeIcon.Fill = new SolidColorBrush(Color.FromArgb(255, 200, 200, 200));
            DislikeIcon.Fill = new SolidColorBrush(Color.FromArgb(255, 136, 136, 136));
        }

        private void DislikeIcon_Entered(object sender, PointerRoutedEventArgs e)
        {
            DislikeIcon.Fill = new SolidColorBrush(Color.FromArgb(255, 160, 160, 160));
        }

        private void LikeIcon_Entered(object sender, PointerRoutedEventArgs e)
        {
            LikeIcon.Fill = new SolidColorBrush(Color.FromArgb(255, 160, 160, 160));
        }

        private void DislikeIcon_Exited(object sender, PointerRoutedEventArgs e)
        {
            DislikeIcon.Fill = new SolidColorBrush(Color.FromArgb(255, 136, 136, 136));
        }

        private void LikeIcon_Exited(object sender, PointerRoutedEventArgs e)
        {
            LikeIcon.Fill = new SolidColorBrush(Color.FromArgb(255, 136, 136, 136));
        }
    }
}

using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.YouTube.v3;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.UI.Xaml;

namespace YTApp.Classes
{
    static class YoutubeItemMethodsStatic
    {
        static public void StartVideo(string ID, MainPage mainPageRef)
        {
            //Get instance of our media viewer
            var viewer = mainPageRef.viewer;
            try
            {
                viewer.Visibility = Visibility.Visible;
                viewer.Source = new Uri("https://www.youtube.com/watch?v=" + ID);
                viewer.TransportControls.Focus(FocusState.Programmatic);
            }
            catch
            {
                //Implement code for when the video has music in it
            }

            //Set the video id for the flyout menu when right clicking on the video
            mainPageRef.VideoID = ID;
        }

        static async public YouTubeService GetService()
        {
            var credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(new ClientSecrets
            {
                ClientId = "957928808020-pa0lopl3crh565k6jd4djaj36rm1d9i5.apps.googleusercontent.com",
                ClientSecret = "oB9U6yWFndnBqLKIRSA0nYGm"
            }, new[] { YouTubeService.Scope.Youtube }, "user", CancellationToken.None);

            // Create the service.
            return new YouTubeService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = "Youtube Viewer",
            });
        }
    }
}

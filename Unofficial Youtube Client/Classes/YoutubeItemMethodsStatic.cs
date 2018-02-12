using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.YouTube.v3;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using VideoLibrary;
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
                var youTube = YouTube.Default;
                var video = youTube.GetVideo("https://www.youtube.com/watch?v=" + ID);
                viewer.Source = new Uri(video.GetUri());
                viewer.Visibility = Visibility.Visible;
                viewer.TransportControls.Focus(FocusState.Programmatic);
            }
            catch { }

            //Set the video id for the flyout menu when right clicking on the video
            mainPageRef.VideoID = ID;
        }

        static public string ViewCountShortner(long viewCount)
        {
            if (viewCount > 1000000)
            {
                return Convert.ToString(Math.Round(Convert.ToDouble(viewCount) / 1000000, 1)) + "M";
            }
            else if (viewCount > 1000)
            {
                return Convert.ToString(Math.Round(Convert.ToDouble(viewCount) / 1000, 1)) + "K";
            }
            else
            {
                return Convert.ToString(viewCount);
            }
        }

        static public string ViewCountShortner(ulong? viewCount)
        {
            if (viewCount > 1000000)
            {
                return Convert.ToString(Math.Round(Convert.ToDouble(viewCount) / 1000000, 1)) + "M";
            }
            else if (viewCount > 1000)
            {
                return Convert.ToString(Math.Round(Convert.ToDouble(viewCount) / 1000, 1)) + "K";
            }
            else
            {
                return Convert.ToString(viewCount);
            }
        }

        /*static async public YouTubeService GetService()
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
        }*/
    }
}

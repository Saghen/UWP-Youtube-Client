using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Flows;
using Google.Apis.Services;
using Google.Apis.Util.Store;
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
        static async public Task<YouTubeService> GetServiceAsync()
        {
            var credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(new ClientSecrets
            {
                ClientId = "957928808020-pa0lopl3crh565k6jd4djaj36rm1d9i5.apps.googleusercontent.com",
                ClientSecret = "oB9U6yWFndnBqLKIRSA0nYGm"
            }, new[] { YouTubeService.Scope.Youtube, Google.Apis.Oauth2.v2.Oauth2Service.Scope.UserinfoProfile }, "user", CancellationToken.None);

            // Create the service.
            return new YouTubeService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = "Youtube Viewer",
            });
        }

        static public async Task<bool> IsUserAuthenticated()
        {
            GoogleAuthorizationCodeFlow.Initializer initializer = new GoogleAuthorizationCodeFlow.Initializer();
            var secrets = new ClientSecrets
            {
                ClientSecret = Constants.ClientSecret,
                ClientId = Constants.ClientID
            };
            initializer.ClientSecrets = secrets;
            initializer.DataStore = new PasswordVaultDataStore();
            var test = new AuthorizationCodeFlow(initializer);
            var token = await test.LoadTokenAsync("user", CancellationToken.None);
            if (token == null)
            {
                return false;
            }
            else
            {
                Constants.Token = token;
                return true;
            }
        }

        static public string ViewCountShortner(long viewCount, int decimals = 1)
        {
            if (viewCount > 1000000000)
                return Convert.ToString(Math.Round(Convert.ToDouble(viewCount) / 1000000000, decimals)) + "B";
            else if (viewCount > 1000000)
                return Convert.ToString(Math.Round(Convert.ToDouble(viewCount) / 1000000, decimals)) + "M";
            else if (viewCount > 1000)
                return Convert.ToString(Math.Round(Convert.ToDouble(viewCount) / 1000, decimals)) + "K";
            else
                return Convert.ToString(viewCount);
        }

        static public string ViewCountShortner(ulong? viewCount, int decimals = 1)
        {
            if (viewCount > 1000000000)
                return Convert.ToString(Math.Round(Convert.ToDouble(viewCount) / 1000000000, decimals)) + "B";
            else if (viewCount > 1000000)
                return Convert.ToString(Math.Round(Convert.ToDouble(viewCount) / 1000000, decimals)) + "M";
            else if (viewCount > 1000)
                return Convert.ToString(Math.Round(Convert.ToDouble(viewCount) / 1000, decimals)) + "K";
            else
                return Convert.ToString(viewCount);
        }
    }
}

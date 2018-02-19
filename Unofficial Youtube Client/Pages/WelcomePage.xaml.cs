using MetroLog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using YTApp.Classes;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace YTApp.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class WelcomePage : Page
    {
        private ILogger Log = LogManagerFactory.DefaultLogManager.GetLogger<WelcomePage>();

        public WelcomePage()
        {
            this.InitializeComponent();
        }

        private void Button_Tapped(object sender, TappedRoutedEventArgs e)
        {
            RunAuthentication();
        }

        private async void RunAuthentication()
        {
            try
            {
                Google.Apis.Auth.OAuth2.UserCredential credential = await Google.Apis.Auth.OAuth2.GoogleWebAuthorizationBroker.AuthorizeAsync(new Google.Apis.Auth.OAuth2.ClientSecrets
                {
                    ClientId = "957928808020-pa0lopl3crh565k6jd4djaj36rm1d9i5.apps.googleusercontent.com",
                    ClientSecret = "oB9U6yWFndnBqLKIRSA0nYGm"
                }, new[] { Google.Apis.YouTube.v3.YouTubeService.Scope.Youtube, Google.Apis.Oauth2.v2.Oauth2Service.Scope.UserinfoProfile }, "user", System.Threading.CancellationToken.None);
            }
            catch (Exception ex)
            {
                Log.Error("Authentication did not complete successfully.");
                Log.Error(ex.Message);
            }
            
            if (await YoutubeItemMethodsStatic.IsUserAuthenticated())
            {
                btnLogin.Visibility = Visibility.Collapsed;
                btnContinue.Visibility = Visibility.Visible;
            }
        }
        private void Continue_Tapped(object sender, TappedRoutedEventArgs e)
        {
            Frame.Navigate(typeof(MainPage));
            Constants.MainPageRef.contentFrame.Navigate(typeof(HomePage));
            Constants.MainPageRef.LoadSubscriptions();
            Constants.MainPageRef.UpdateLoginDetails();
        }
    }
}

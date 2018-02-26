using MetroLog;
using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
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
            await YoutubeMethodsStatic.GetServiceAsync();
            
            if (await YoutubeMethodsStatic.IsUserAuthenticated())
            {
                btnLogin.Visibility = Visibility.Collapsed;
                btnContinue.Visibility = Visibility.Visible;
            }
        }
        private void Continue_Tapped(object sender, TappedRoutedEventArgs e)
        {
            Frame.Navigate(typeof(MainPage));
        }
    }
}

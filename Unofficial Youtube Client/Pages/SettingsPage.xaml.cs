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

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace YTApp.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class SettingsPage : Page
    {
        public SettingsPage()
        {
            InitializeComponent();

            if ((string)Windows.Storage.ApplicationData.Current.LocalSettings.Values["Theme"] == "Dark" || Windows.Storage.ApplicationData.Current.LocalSettings.Values["Theme"] == null)
                ThemeToggleSwitch.IsOn = true;

            //We set it here so that it doesn't fire when we set the initial value
            ThemeToggleSwitch.Toggled += ToggleSwitch_Toggled;
        }

        private void ToggleSwitch_Toggled(object sender, RoutedEventArgs e)
        {
            ThemeToggleSwitchRestartMessage.Visibility = Visibility.Visible;

            Windows.Storage.ApplicationDataContainer localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
            if (((ToggleSwitch)sender).IsOn)
                localSettings.Values["Theme"] = "Dark";
            else
                localSettings.Values["Theme"] = "Light";
        }
    }
}

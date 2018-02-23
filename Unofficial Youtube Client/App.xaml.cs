using MetroLog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.Data.Json;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using YTApp.Classes;

namespace YTApp
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    sealed partial class App : Application
    {


        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            Windows.Storage.ApplicationDataContainer localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;

            if (localSettings.Values["Theme"] == null || (string)localSettings.Values["Theme"] == "Dark")
                RequestedTheme = ApplicationTheme.Dark;
            else
                RequestedTheme = ApplicationTheme.Light;

            this.InitializeComponent();
            this.Suspending += OnSuspending;

            //Log everything from Info to Fatal levels to a file.
            LogManagerFactory.DefaultConfiguration.AddTarget(LogLevel.Info, LogLevel.Fatal, new MetroLog.Targets.StreamingFileTarget());

            //Log any crashes that occur and their stack trace.
            GlobalCrashHandler.Configure();
        }

        /// <summary>
        /// Invoked when the application is launched normally by the end user.  Other entry points
        /// will be used such as when the application is launched to open a specific file.
        /// </summary>
        /// <param name="e">Details about the launch request and process.</param>
        protected async override void OnLaunched(LaunchActivatedEventArgs e)
        {
            if (RequestedTheme == ApplicationTheme.Dark)
            {
                ((SolidColorBrush)Resources["AppBackgroundDark"]).Color = Color.FromArgb(255, 34, 34, 34);
                ((SolidColorBrush)Resources["AppBackground"]).Color = Color.FromArgb(255, 37, 37, 37);
                ((SolidColorBrush)Resources["AppBackgroundLighter"]).Color = Color.FromArgb(255, 42, 42, 42);
                ((SolidColorBrush)Resources["AppBackgroundLightest"]).Color = Color.FromArgb(255, 51, 51, 51);
                ((SolidColorBrush)Resources["AppText"]).Color = Color.FromArgb(255, 255, 255, 255);
                ((SolidColorBrush)Resources["AppTextSecondary"]).Color = Color.FromArgb(255, 170, 170, 170);
                ((SolidColorBrush)Resources["ButtonBackground"]).Color = Color.FromArgb(255, 102, 102, 102);
            }
            else
            {
                ((SolidColorBrush)Resources["AppBackgroundDark"]).Color = Color.FromArgb(255, 240, 240, 240);
                ((SolidColorBrush)Resources["AppBackground"]).Color = Color.FromArgb(255, 245, 245, 245);
                ((SolidColorBrush)Resources["AppBackgroundLighter"]).Color = Color.FromArgb(255, 250, 250, 250);
                ((SolidColorBrush)Resources["AppBackgroundLightest"]).Color = Color.FromArgb(255, 255, 255, 255);
                ((SolidColorBrush)Resources["AppText"]).Color = Color.FromArgb(255, 0, 0, 0);
                ((SolidColorBrush)Resources["AppTextSecondary"]).Color = Color.FromArgb(255, 60, 60, 60);
                ((SolidColorBrush)Resources["ButtonBackground"]).Color = Color.FromArgb(255, 200, 200, 200);
                ((SolidColorBrush)Resources["TextBoxBackground"]).Color = Color.FromArgb(255, 240, 240, 240);
                ((SolidColorBrush)Resources["BorderColor"]).Color = Color.FromArgb(255, 220, 220, 220);
            }

            Frame rootFrame = Window.Current.Content as Frame;

            //Reset Title Bar
            var coreTitleBar = Windows.ApplicationModel.Core.CoreApplication.GetCurrentView().TitleBar;
            coreTitleBar.ExtendViewIntoTitleBar = false;

            // Do not repeat app initialization when the Window already has content,
            // just ensure that the window is active
            if (rootFrame == null)
            {
                // Create a Frame to act as the navigation context and navigate to the first page
                rootFrame = new Frame();

                rootFrame.NavigationFailed += OnNavigationFailed;

                if (e.PreviousExecutionState == ApplicationExecutionState.Terminated)
                {
                    //TODO: Load state from previously suspended application
                }

                // Place the frame in the current Window
                Window.Current.Content = rootFrame;
            }

            if (e.PrelaunchActivated == false)
            {
                if (rootFrame.Content == null)
                {
                    // When the navigation stack isn't restored navigate to the first page,
                    // configuring the new page by passing required information as a navigation
                    // parameter

                    if (await Classes.YoutubeMethodsStatic.IsUserAuthenticated())
                        rootFrame.Navigate(typeof(MainPage), e.Arguments);
                    else
                        rootFrame.Navigate(typeof(Pages.WelcomePage), e.Arguments);

                }
                // Ensure the current window is active
                Window.Current.Activate();
            }
        }

        /// <summary>
        /// Invoked when Navigation to a certain page fails
        /// </summary>
        /// <param name="sender">The Frame which failed navigation</param>
        /// <param name="e">Details about the navigation failure</param>
        void OnNavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            throw new Exception("Failed to load Page " + e.SourcePageType.FullName);
        }

        /// <summary>
        /// Invoked when application execution is being suspended.  Application state is saved
        /// without knowing whether the application will be terminated or resumed with the contents
        /// of memory still intact.
        /// </summary>
        /// <param name="sender">The source of the suspend request.</param>
        /// <param name="e">Details about the suspend request.</param>
        private void OnSuspending(object sender, SuspendingEventArgs e)
        {
            var deferral = e.SuspendingOperation.GetDeferral();
            //TODO: Save application state and stop any background activity
            deferral.Complete();
        }
    }
}

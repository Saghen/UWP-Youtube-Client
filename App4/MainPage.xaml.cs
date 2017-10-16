﻿using System;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using VideoLibrary;
using Google.Apis.YouTube.v3;
using System.Collections.Generic;
using System.Threading.Tasks;
using Google.Apis.Services;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Flows;
using Windows.UI.Core;
using Newtonsoft.Json;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.ApplicationModel.DataTransfer;
using Windows.UI.Xaml.Media.Animation;
using Google.Apis.YouTube.v3.Data;
using System.ComponentModel;
using YTApp.Classes;
using Windows.UI.Xaml.Media.Imaging;
using YTApp.Pages;
using Windows.UI.Xaml.Navigation;
using System.Linq;
using Windows.Storage;
using System.Net.Http;
using Windows.Security.Cryptography;
using Windows.Security.Cryptography.Core;
using Windows.Storage.Streams;
using Windows.Data.Json;
using System.Text;
using Windows.ApplicationModel.Activation;
using System.Threading;

namespace YTApp
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private string YoutubeLink = "";
        private string YoutubeID = "";
        private bool FullSizedMediaElement = true;

        private List<SubscriptionDataType> subscriptionsList = new List<SubscriptionDataType>();

        List<SearchListResponse> youtubeVideos = new List<SearchListResponse>();

        public string OAuthToken;

        BackgroundWorker bg = new BackgroundWorker();

        public MainPage()
        {
            this.InitializeComponent();
            
        }

        private void DoWork()
        {
            string nextPageToken;
            var tempSubscriptions = GetSubscriptions(null);
            foreach (Subscription sub in tempSubscriptions.Items)
            {
                var subscription = new SubscriptionDataType();
                subscription.Id = sub.Snippet.ChannelId;
                subscription.Thumbnail = new BitmapImage(new Uri(sub.Snippet.Thumbnails.Medium.Url));
                subscription.Title = sub.Snippet.Title;
                subscriptionsList.Add(subscription);
            }
            if (tempSubscriptions.NextPageToken != null)
            {
                nextPageToken = tempSubscriptions.NextPageToken;
                while (nextPageToken != null)
                {
                    var tempSubs = GetSubscriptions(nextPageToken);
                    foreach (Subscription sub in tempSubs.Items)
                    {
                        var subscription = new SubscriptionDataType();
                        subscription.Id = sub.Snippet.ChannelId;
                        subscription.Thumbnail = new BitmapImage(new Uri(sub.Snippet.Thumbnails.Medium.Url));
                        subscription.Title = sub.Snippet.Title;
                        subscriptionsList.Add(subscription);
                    }
                    nextPageToken = tempSubs.NextPageToken;
                }
            }
            subscriptionsList.Sort((x, y) => string.Compare(x.Title, y.Title));
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            Window.Current.CoreWindow.KeyDown += Event_KeyDown;
            SearchBox.Focus(FocusState.Keyboard);
        }

        #region Search

        #region Events

        private async void Search_Click(object sender, RoutedEventArgs e)
        {
            contentFrame.Navigate(typeof(SearchPage), new Params() { mainPageRef = this });
        }

        private async void SearchBox_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter)
            {
                if (Uri.IsWellFormedUriString(SearchBox.Text, UriKind.Absolute))
                {
                    try
                    {
                        var youTube = YouTube.Default;
                        var video = youTube.GetVideo(SearchBox.Text);
                        StartVideo(video.Uri);
                    }
                    catch { }
                }
                contentFrame.Navigate(typeof(SearchPage), new Params() { mainPageRef = this });
            }
        }

        public class Params
        {
            public MainPage mainPageRef { get; set; }
        }

        #endregion

        #region API

        private SubscriptionListResponse GetSubscriptions(string NextPageToken)
        {
            var youtubeService = new YouTubeService(new BaseClientService.Initializer()
            {
                ApiKey = "AIzaSyCXOZJH2GUbdqwxZwsjTU93lFvgdnMOVD0",
                ApplicationName = this.GetType().ToString(),
            });

            var subscriptions = youtubeService.Subscriptions.List("snippet, contentDetails");
            subscriptions.ChannelId = "UCfYSGOxQeqO4AaM7eKryjeQ";
            subscriptions.PageToken = NextPageToken;
            subscriptions.OauthToken = App.OAuthCode;
            subscriptions.MaxResults = 50;

            return subscriptions.Execute();
        }
        #endregion

        #endregion

        #region Media Viewer

        #region Methods

        public void StartVideo(string URL)
        {
            viewer.Visibility = Visibility.Visible;
            viewer.Source = new Uri(URL);
            viewer.TransportControls.Focus(FocusState.Programmatic);
            var _displayRequest = new Windows.System.Display.DisplayRequest();
            _displayRequest.RequestActive();
        }

        public void StopVideo()
        {
            if (MediaElementContainer.Width != 640)
            {
                MediaElementContainer.Width = 640;
                MediaElementContainer.Height = 360;
            }
            else
            {
                MediaElementContainer.Width = Double.NaN;
                MediaElementContainer.Height = Double.NaN;
            }
        }

        private void Storyboard_Completed(object sender, object e)
        {
            MediaElementContainer.Height = Double.NaN;
            MediaElementContainer.Width = Double.NaN;
        }

        #endregion

        #region Events


        private void CloseMediaElement_Click(object sender, RoutedEventArgs e)
        {
            StopVideo();
        }

        private void viewer_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            viewer.IsFullWindow = !viewer.IsFullWindow;
            if (viewer.CurrentState == Windows.UI.Xaml.Media.MediaElementState.Playing)
            {
                viewer.Pause();
            }
            else
            {
                viewer.Play();
            }
        }

        private void Event_KeyDown(object sender, KeyEventArgs e)
        {
            if (viewer.IsFullWindow && e.VirtualKey == Windows.System.VirtualKey.Escape) { viewer.IsFullWindow = false; }
            if (viewer.CurrentState == Windows.UI.Xaml.Media.MediaElementState.Playing && e.VirtualKey == Windows.System.VirtualKey.Space && viewer.Visibility == Visibility.Visible)
            {
                viewer.Pause();
            }
            else if (e.VirtualKey == Windows.System.VirtualKey.Space && viewer.Visibility == Visibility.Visible)
            {
                viewer.Play();
            }
        }

        private void viewer_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (viewer.CurrentState == Windows.UI.Xaml.Media.MediaElementState.Playing)
            {
                viewer.Pause();
            }
            else
            {
                viewer.Play();
            }
        }

        private void viewer_RightTapped(object sender, RightTappedRoutedEventArgs e)
        {
            var tappedItem = (UIElement)e.OriginalSource;
            var attachedFlyout = (MenuFlyout)FlyoutBase.GetAttachedFlyout(viewer.TransportControls);

            attachedFlyout.ShowAt(tappedItem, e.GetPosition(tappedItem));
        }

        #endregion

        #endregion

        #region MediaElementButton Management
        private void viewer_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            CloseMediaElement.Visibility = Visibility.Visible;
            FadeIn.Begin();
        }

        private void viewer_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            FadeOut.Completed += MediaButtonCompleted;
            FadeOut.Begin();
        }

        private void MediaButtonCompleted(object sender, object e)
        {
            if (CloseMediaElement.Opacity == 0) { CloseMediaElement.Visibility = Visibility.Collapsed; }
        }
        #endregion

        private void Flyout_CopyLink(object sender, RoutedEventArgs e)
        {
            var dataPackage = new DataPackage();
            dataPackage.SetText("https://youtu.be/" + YoutubeID);
            Clipboard.SetContent(dataPackage);
        }

        private void Flyout_CopyLinkAtTime(object sender, RoutedEventArgs e)
        {
            var dataPackage = new DataPackage();
            dataPackage.SetText("https://youtu.be/" + YoutubeID + "?t=" + Convert.ToInt32(viewer.Position.TotalSeconds) + "s");
            Clipboard.SetContent(dataPackage);
        }


        private void HamburgerButton_Click(object sender, RoutedEventArgs e)
        {
            SideBarSplitView.IsPaneOpen = !SideBarSplitView.IsPaneOpen;
        }

        private void HamburgerButton_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            Window.Current.CoreWindow.PointerCursor = new CoreCursor(CoreCursorType.Hand, 1);
        }

        private void HamburgerButton_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            Window.Current.CoreWindow.PointerCursor = new CoreCursor(CoreCursorType.Arrow, 2);
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            Test();
        }

        private async Task Test()
        {
            UserCredential credential;
            credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
    new ClientSecrets
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

            var subscriptions = service.Subscriptions.List("snippet, contentDetails");
            subscriptions.MaxResults = 50;
            subscriptions.Mine = true;
            var whew = await subscriptions.ExecuteAsync();
        }
    }
}
using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.ApplicationModel.DataTransfer;
using Google.Apis.YouTube.v3;
using Google.Apis.Services;
using Google.Apis.Auth.OAuth2;
using Google.Apis.YouTube.v3.Data;
using VideoLibrary;
using YTApp.Classes;
using YTApp.Pages;
using YTApp.Classes.DataTypes;


namespace YTApp
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public string VideoID = "";

        public List<SubscriptionDataType> subscriptionsList = new List<SubscriptionDataType>();

        List<SearchListResponse> youtubeVideos = new List<SearchListResponse>();

        public string OAuthToken;

        BackgroundWorker bg = new BackgroundWorker();

        public MainPage()
        {
            this.InitializeComponent();
            SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = AppViewBackButtonVisibility.Visible;
            SystemNavigationManager.GetForCurrentView().BackRequested += MainPage_BackRequested;
            LoadSubscriptions();

            contentFrame.Navigate(typeof(HomePage), new NavigateParams() { mainPageRef = this, Refresh = true });
        }

        private void MainPage_BackRequested(object sender, BackRequestedEventArgs e)
        {
            if (contentFrame.CanGoBack && e.Handled == false)
            {
                e.Handled = true;
                contentFrame.GoBack();
            }
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            Window.Current.CoreWindow.KeyDown += Event_KeyDown;
            SearchBox.Focus(FocusState.Keyboard);
        }

        #region Menu

        private async void LoadSubscriptions()
        {
            UserCredential credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(new ClientSecrets
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

            string nextPageToken;
            var tempSubscriptions = GetSubscriptions(null, service);
            foreach (Subscription sub in tempSubscriptions.Items)
            {
                var subscription = new SubscriptionDataType();
                subscription.Id = sub.Snippet.ResourceId.ChannelId;
                subscription.Thumbnail = new BitmapImage(new Uri(sub.Snippet.Thumbnails.Medium.Url));
                subscription.Title = sub.Snippet.Title;
                subscription.NewVideosCount = Convert.ToString(sub.ContentDetails.NewItemCount);
                subscription.SubscriptionID = sub.Id;
                subscriptionsList.Add(subscription);
            }
            if (tempSubscriptions.NextPageToken != null)
            {
                nextPageToken = tempSubscriptions.NextPageToken;
                while (nextPageToken != null)
                {
                    var tempSubs = GetSubscriptions(nextPageToken, service);
                    foreach (Subscription sub in tempSubs.Items)
                    {
                        var subscription = new SubscriptionDataType();
                        subscription.Id = sub.Snippet.ResourceId.ChannelId;
                        subscription.Thumbnail = new BitmapImage(new Uri(sub.Snippet.Thumbnails.Medium.Url));
                        subscription.Title = sub.Snippet.Title;
                        subscription.NewVideosCount = Convert.ToString(sub.ContentDetails.NewItemCount);
                        subscriptionsList.Add(subscription);
                    }
                    nextPageToken = tempSubs.NextPageToken;
                }
            }
            subscriptionsList.Sort((x, y) => string.Compare(x.Title, y.Title));
        }

        private void SubscriptionsList_ItemClick(object sender, ItemClickEventArgs e)
        {
            var temp = (SubscriptionDataType)e.ClickedItem;
            contentFrame.Navigate(typeof(ChannelPage), new NavigateParams()
            {
                mainPageRef = this,
                ID = temp.Id
            });
        }

        private void PageMenuControls_ItemClick(object sender, ItemClickEventArgs e)
        {
            var item = (SplitViewItemDataType)e.ClickedItem;
            if(item.Text == "Home")
                contentFrame.Navigate(typeof(HomePage), new NavigateParams() { mainPageRef = this });
        }

        private void HamburgerButton_Click(object sender, RoutedEventArgs e)
        {
            SideBarSplitView.IsPaneOpen = !SideBarSplitView.IsPaneOpen;
        }

        #endregion

        #region Search

        #region Events

        private void Search_Click(object sender, RoutedEventArgs e)
        {
            contentFrame.Navigate(typeof(SearchPage), new NavigateParams() { mainPageRef = this });
        }

        private void SearchBox_KeyDown(object sender, KeyRoutedEventArgs e)
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
                contentFrame.Navigate(typeof(SearchPage), new NavigateParams() { mainPageRef = this });
            }
        }

        #endregion

        #region API

        private SubscriptionListResponse GetSubscriptions(string NextPageToken, YouTubeService service)
        {
            var subscriptions = service.Subscriptions.List("snippet, contentDetails");
            subscriptions.PageToken = NextPageToken;
            subscriptions.Mine = true;
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


        private void MinimizeMediaElement_Click(object sender, RoutedEventArgs e)
        {
            StopVideo();
        }

        private void CloseMediaElement_Click(object sender, RoutedEventArgs e)
        {
            viewer.Stop();
            viewer.Visibility = Visibility.Collapsed;
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
                e.Handled = true;
            }
            else if (e.VirtualKey == Windows.System.VirtualKey.Space && viewer.Visibility == Visibility.Visible)
            {
                viewer.Play();
                e.Handled = true;
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
            MinimizeMediaElement.Visibility = Visibility.Visible;
            FadeIn.Begin();
        }

        private void viewer_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            FadeOut.Completed += MediaButtonCompleted;
            FadeOut.Begin();
        }

        private void MediaButtonCompleted(object sender, object e)
        {
            if (MinimizeMediaElement.Opacity == 0) { MinimizeMediaElement.Visibility = Visibility.Collapsed; }
        }
        #endregion

        #region Flyout

        private void Flyout_CopyLink(object sender, RoutedEventArgs e)
        {
            var dataPackage = new DataPackage();
            dataPackage.SetText("https://youtu.be/" + VideoID);
            Clipboard.SetContent(dataPackage);
        }

        private void Flyout_CopyLinkAtTime(object sender, RoutedEventArgs e)
        {
            var dataPackage = new DataPackage();
            dataPackage.SetText("https://youtu.be/" + VideoID + "?t=" + Convert.ToInt32(viewer.Position.TotalSeconds) + "s");
            Clipboard.SetContent(dataPackage);
        }

        #endregion

        
    }
}

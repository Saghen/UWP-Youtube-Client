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
using Google.Apis.Auth.OAuth2.Flows;
using Google.Apis.YouTube.v3;
using Google.Apis.Services;
using Google.Apis.Auth.OAuth2;
using Google.Apis.YouTube.v3.Data;
using Google.Apis.Oauth2.v2;
using VideoLibrary;
using YTApp.Classes;
using YTApp.Pages;
using YTApp.Classes.DataTypes;
using System.Collections.ObjectModel;
using Google.Apis.Auth.OAuth2.Requests;
using Google.Apis.Util.Store;
using System.Net.Http;
using System.Net;
using System.Text;
using MetroLog;
using MetroLog.Targets;
using Newtonsoft.Json;

namespace YTApp
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public string VideoID = "";

        private ILogger Log = LogManagerFactory.DefaultLogManager.GetLogger<MainPage>();

        public ObservableCollection<SubscriptionDataType> subscriptionsList = new ObservableCollection<SubscriptionDataType>();
        List<SearchListResponse> youtubeVideos = new List<SearchListResponse>();

        public event EventHandler SwitchToFullSize;

        public MainPage()
        {
            this.InitializeComponent();

            //Reset Title Bar
            var coreTitleBar = Windows.ApplicationModel.Core.CoreApplication.GetCurrentView().TitleBar;
            coreTitleBar.ExtendViewIntoTitleBar = false;

            SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = AppViewBackButtonVisibility.Visible;
            SystemNavigationManager.GetForCurrentView().BackRequested += MainPage_BackRequested;

            Log.Info("Main page startup has started");
            Startup();
        }

        #region Main Events

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
            SearchBox.Focus(FocusState.Keyboard);
        }

        #endregion

        #region Startup
        private async void Startup()
        {
            Log.Info("Checking if the user is authenticated");
            //Check if user is authenticated and show startup page if not
            if (!(await YoutubeItemMethodsStatic.IsUserAuthenticated()))
            {
                Log.Info("The user is not authenticated");
                contentFrame.Navigate(typeof(WelcomePage), new NavigateParams() { MainPageRef = this });
            }
            else
            {
                Log.Info("The user is authenticated");
                Log.Info("Loading subscriptions");
                LoadSubscriptions();
                contentFrame.Navigate(typeof(HomePage), new NavigateParams() { MainPageRef = this });
                UpdateLoginDetails();
            }

            //Plays Youtube link in clipboard
            PlayClipboardYLink();
        }

        private async void PlayClipboardYLink()
        {
            if (Constants.Token == null)
                return;
            try
            {
                var dataPackageView = await Clipboard.GetContent().GetTextAsync();

                if (Uri.TryCreate(dataPackageView, UriKind.Absolute, out Uri uriResult)
                    && (uriResult.Scheme == "http" || uriResult.Scheme == "https"))
                {
                    if (Uri.IsWellFormedUriString("https://www.youtube.com/watch?v=", UriKind.Absolute))
                    {
                        Log.Info("Loading video found in clipboard");
                        StartVideo(dataPackageView.Remove(0, 32));
                    }
                    else if (Uri.IsWellFormedUriString("https://youtu.be/", UriKind.Absolute))
                    {
                        Log.Info("Loading video found in clipboard");
                        StartVideo(dataPackageView.Remove(0, 17));
                    }
                }
            }
            catch { Log.Error("Exception thrown while loading video from clipboard"); }
        }
        #endregion

        #region Menu

        #region Subscriptions

        public async void LoadSubscriptions()
        {
            //Get the service
            var service = await YoutubeItemMethodsStatic.GetServiceAsync();

            string nextPageToken;

            //Get the subscriptions
            var tempSubscriptions = GetSubscriptions(null, service);
            if (tempSubscriptions == null)
            {
                Log.Error("Get Subscriptions returned a null object. The method \"LoadSubscriptions\" was cancelled");
                return;
            }

            foreach (Subscription sub in tempSubscriptions.Items)
            {
                SubscriptionDataType subscription = new SubscriptionDataType();
                try
                {
                    subscription = new SubscriptionDataType
                    {
                        Id = sub.Snippet.ResourceId.ChannelId,
                        Thumbnail = new BitmapImage(new Uri(sub.Snippet.Thumbnails.Medium.Url)),
                        Title = sub.Snippet.Title,
                        NewVideosCount = Convert.ToString(sub.ContentDetails.NewItemCount),
                        SubscriptionID = sub.Id
                    };
                    subscriptionsList.Add(subscription);
                }
                catch (Exception ex)
                {
                    Log.Error(string.Format("Subscription failed to load. Object:", JsonConvert.SerializeObject(subscription)));
                    Log.Error(ex.Message);
                }
            }

            if (tempSubscriptions.NextPageToken != null)
            {
                nextPageToken = tempSubscriptions.NextPageToken;
                while (nextPageToken != null)
                {
                    var tempSubs = GetSubscriptions(nextPageToken, service);
                    foreach (Subscription sub in tempSubs.Items)
                    {
                        SubscriptionDataType subscription = new SubscriptionDataType();
                        try
                        {
                            subscription = new SubscriptionDataType
                            {
                                Id = sub.Snippet.ResourceId.ChannelId,
                                Thumbnail = new BitmapImage(new Uri(sub.Snippet.Thumbnails.Medium.Url)),
                                Title = sub.Snippet.Title,
                                NewVideosCount = Convert.ToString(sub.ContentDetails.NewItemCount),
                                SubscriptionID = sub.Id
                            };
                            subscriptionsList.Add(subscription);
                        }
                        catch (Exception ex)
                        {
                            Log.Error(string.Format("Subscription failed to load. Object:", JsonConvert.SerializeObject(subscription)));
                            Log.Error(ex.Message);
                        }
                    }
                    nextPageToken = tempSubs.NextPageToken;
                }
            }
        }

        private SubscriptionListResponse GetSubscriptions(string NextPageToken, YouTubeService service)
        {
            var subscriptions = service.Subscriptions.List("snippet, contentDetails");
            try
            {
                subscriptions.PageToken = NextPageToken;
                subscriptions.Mine = true;
                subscriptions.MaxResults = 50;
                subscriptions.Order = SubscriptionsResource.ListRequest.OrderEnum.Alphabetical;

                return subscriptions.Execute();
            }
            catch (Exception ex)
            {
                Log.Fatal(String.Format("GetSubscriptions failed to load with the service {0}", JsonConvert.SerializeObject(subscriptions)));
                Log.Fatal(ex.Message);
                return null;
            }
        }

        private void SubscriptionsList_ItemClick(object sender, ItemClickEventArgs e)
        {
            var temp = (SubscriptionDataType)e.ClickedItem;
            contentFrame.Navigate(typeof(ChannelPage), new NavigateParams()
            {
                MainPageRef = this,
                ID = temp.Id
            });
        }

        #endregion

        public async void UpdateLoginDetails()
        {
            UserCredential credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(new ClientSecrets
            {
                ClientId = "957928808020-pa0lopl3crh565k6jd4djaj36rm1d9i5.apps.googleusercontent.com",
                ClientSecret = "oB9U6yWFndnBqLKIRSA0nYGm"
            }, new[] { Oauth2Service.Scope.UserinfoProfile }, "user", CancellationToken.None);


            // Create the service.
            var service = new Oauth2Service(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = "Youtube Viewer",
            });

            var GetLoginInfo = service.Userinfo.Get();

            try
            {
                var LoginInfo = await GetLoginInfo.ExecuteAsync();

                txtLoginName.Text = LoginInfo.Name;

                var profileImg = new Windows.UI.Xaml.Media.ImageBrush
                {
                    ImageSource = new BitmapImage(new Uri(LoginInfo.Picture))
                };
                imgProfileIcon.Fill = profileImg;
            }
            catch (Exception)
            {
                txtLoginName.Text = "";
                imgProfileIcon.Fill = new Windows.UI.Xaml.Media.SolidColorBrush(Windows.UI.Color.FromArgb(0, 0, 0, 0));
            }
        }

        private void PageMenuControls_ItemClick(object sender, ItemClickEventArgs e)
        {
            var item = (SplitViewItemDataType)e.ClickedItem;
            if (item.Text == "Home" && Constants.Token != null)
                contentFrame.Navigate(typeof(HomePage), new NavigateParams() { MainPageRef = this });
        }

        private void PlaylistOptions_ItemClick(object sender, ItemClickEventArgs e)
        {
            if (Constants.Token != null)
            {
                var item = (SplitViewItemDataType)e.ClickedItem;
                if (item.Text == "Trending")
                    contentFrame.Navigate(typeof(TrendingPage), new NavigateParams() { MainPageRef = this });
                else if (item.Text == "History")
                    contentFrame.Navigate(typeof(HistoryPage), new NavigateParams() { MainPageRef = this});
            }
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
            if (Constants.Token != null)
                contentFrame.Navigate(typeof(SearchPage), new NavigateParams() { MainPageRef = this });
        }

        private void SearchBox_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter && Constants.Token != null)
            {
                if (Uri.IsWellFormedUriString(SearchBox.Text, UriKind.Absolute))
                {
                    PlayClipboardYLink();
                }
                contentFrame.Navigate(typeof(SearchPage), new NavigateParams() { MainPageRef = this });
            }
        }

        #endregion

        #endregion

        #region Media Viewer

        #region Methods

        public void StartVideo(string Id)
        {
            videoFrame.Visibility = Visibility.Visible;
            videoFrame.Navigate(typeof(VideoPage), new NavigateParams() { MainPageRef = this, ID = Id });
        }

        #endregion

        #region Events

        public void Viewer_SwitchToFullSize(object sender, EventArgs e)
        {
            SwitchToFullSize.Invoke(this, new EventArgs());
        }

        #endregion

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

        #region Login Region
        private async void BtnSignOut_Tapped(object sender, TappedRoutedEventArgs e)
        {
            //Check that the user is logged in
            if (Constants.Token == null)
                return;

            UserCredential credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(new ClientSecrets
            {
                ClientId = "957928808020-pa0lopl3crh565k6jd4djaj36rm1d9i5.apps.googleusercontent.com",
                ClientSecret = "oB9U6yWFndnBqLKIRSA0nYGm"
            }, new[] { YouTubeService.Scope.Youtube, Oauth2Service.Scope.UserinfoProfile }, "user", CancellationToken.None);

            await credential.RevokeTokenAsync(CancellationToken.None);

            //Clear Login details
            txtLoginName.Text = "";
            imgProfileIcon.Fill = new Windows.UI.Xaml.Media.SolidColorBrush(Windows.UI.Color.FromArgb(0, 0, 0, 0));

            //Clear Subscriptions
            SubscriptionsList.ItemsSource = null;

            contentFrame.Navigate(typeof(WelcomePage), new NavigateParams() { MainPageRef = this });
        }

        private void BtnLoginFlyout_Tapped(object sender, TappedRoutedEventArgs e)
        {
            FlyoutBase.ShowAttachedFlyout((FrameworkElement)sender);
        }

        private async void BtnMyChannel_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (Constants.Token == null)
                return;

            var service = await YoutubeItemMethodsStatic.GetServiceAsync();

            var getMyChannel = service.Channels.List("snippet");
            getMyChannel.Mine = true;
            var result = await getMyChannel.ExecuteAsync();

            contentFrame.Navigate(typeof(ChannelPage), new NavigateParams() { MainPageRef = this, ID = result.Items[0].Id });
        }
        #endregion
    }
}

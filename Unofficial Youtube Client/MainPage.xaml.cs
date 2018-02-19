using System;
using System.Collections.Generic;
using System.Threading;
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
using Google.Apis.Oauth2.v2;
using YTApp.Classes;
using YTApp.Pages;
using YTApp.Classes.DataTypes;
using System.Collections.ObjectModel;
using MetroLog;
using Newtonsoft.Json;
using YoutubeExplode;
using System.Threading.Tasks;
using Windows.UI.Xaml.Media.Animation;

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
            InitializeComponent();

            Constants.MainPageRef = this;

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
                Frame.Navigate(typeof(WelcomePage));
            }
            else
            {
                Log.Info("The user is authenticated");
                Log.Info("Loading subscriptions");
                LoadSubscriptions();
                contentFrame.Navigate(typeof(HomePage));
                UpdateLoginDetails();
            }

            //Plays Youtube link in clipboard
            PlayClipboardYLink();
        }

        private async void PlayClipboardYLink()
        {
            try
            {
                var clipboardText = await Clipboard.GetContent().GetTextAsync();
                var videoID = YoutubeClient.ParseVideoId(clipboardText);
                StartVideo(videoID);
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
            Constants.activeChannelID = temp.Id;
            contentFrame.Navigate(typeof(ChannelPage));
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
                contentFrame.Navigate(typeof(HomePage));
        }

        private void PlaylistOptions_ItemClick(object sender, ItemClickEventArgs e)
        {
            if (Constants.Token != null)
            {
                var item = (SplitViewItemDataType)e.ClickedItem;
                if (item.Text == "Trending")
                    contentFrame.Navigate(typeof(TrendingPage));
                else if (item.Text == "History")
                    contentFrame.Navigate(typeof(HistoryPage));
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
            contentFrame.Navigate(typeof(SearchPage));
        }

        private void SearchBox_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter && Constants.Token != null)
            {
                if (Uri.IsWellFormedUriString(SearchBox.Text, UriKind.Absolute))
                {
                    PlayClipboardYLink();
                }
                contentFrame.Navigate(typeof(SearchPage));
            }
        }

        #endregion

        #endregion

        #region Media Viewer

        #region Methods

        public void StartVideo(string Id)
        {
            videoFrame.Visibility = Visibility.Visible;
            Constants.activeVideoID = Id;
            videoFrame.Navigate(typeof(VideoPage));
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

            Frame.Navigate(typeof(WelcomePage));
        }

        private void BtnLoginFlyout_Tapped(object sender, TappedRoutedEventArgs e)
        {
            FlyoutBase.ShowAttachedFlyout((FrameworkElement)sender);
        }

        private async void BtnMyChannel_Tapped(object sender, TappedRoutedEventArgs e)
        {
            var service = await YoutubeItemMethodsStatic.GetServiceAsync();

            var getMyChannel = service.Channels.List("snippet");
            getMyChannel.Mine = true;
            var result = await getMyChannel.ExecuteAsync();

            Constants.activeChannelID = result.Items[0].Id;
            contentFrame.Navigate(typeof(ChannelPage));
        }
        #endregion

        #region Video Functions
        public async void DownloadVideo()
        {
            var client = new YoutubeClient();
            var videoUrl = (await client.GetVideoMediaStreamInfosAsync(Constants.activeVideoID)).Muxed[0].Url;

            Windows.Storage.ApplicationDataContainer localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;

            var picker = new Windows.Storage.Pickers.FileOpenPicker
            {
                SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.Unspecified,
                CommitButtonText = "Save that bich"
            };

            Windows.Storage.StorageFile file = await picker.PickSingleFileAsync();

            if (file != null)
            {
                try
                {
                    Windows.Networking.BackgroundTransfer.BackgroundDownloader downloader = new Windows.Networking.BackgroundTransfer.BackgroundDownloader();
                    Windows.Networking.BackgroundTransfer.DownloadOperation download = downloader.CreateDownload(new Uri(videoUrl), file);

                    HandleDownloadAsync(download);
                }
                catch (Exception ex)
                {
                    Log.Error("Download Error", ex);
                    Log.Error(videoUrl);
                }
            }
            else
            {
                Log.Info("Download operation was cancelled.");
            }
        }

        private async void HandleDownloadAsync(Windows.Networking.BackgroundTransfer.DownloadOperation download)
        {
            await Task.Factory.StartNew(() => download.StartAsync());

            while (download.Progress.BytesReceived / download.Progress.TotalBytesToReceive < 1)
            {
                DownloadProgress.Value = download.Progress.BytesReceived / download.Progress.TotalBytesToReceive;
                await Task.Delay(16);
            }
            DownloadProgress.Value = 1;
            await Task.Delay(200);
            DownloadProgress.Visibility = Visibility.Collapsed;
        }

        #endregion
    }
}

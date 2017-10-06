using System;
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

        public MainPage()
        {
            this.InitializeComponent();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            Window.Current.CoreWindow.KeyDown += Event_KeyDown;
        }

        #region Search

        #region Events

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                await Run();
            }
            catch { }

            GetSubscriptions();
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
                else { try { await Run(); } catch { } }
            }
        }

        #endregion
        #region API

        private async Task Run()
        {
            var youtubeService = new YouTubeService(new BaseClientService.Initializer()
            {
                ApiKey = "AIzaSyCXOZJH2GUbdqwxZwsjTU93lFvgdnMOVD0",
                ApplicationName = this.GetType().ToString()
            });

            var searchListRequest = youtubeService.Search.List("snippet");
            searchListRequest.Q = SearchBox.Text; // Replace with your search term.
            searchListRequest.MaxResults = 50;

            // Call the search.list method to retrieve results matching the specified query term.
            var searchListResponse = await searchListRequest.ExecuteAsync();

            List<string> videos = new List<string>();
            List<string> channels = new List<string>();
            List<string> playlists = new List<string>();

            Putallthethingshere.Children.Clear();

            // Add each result to the appropriate list, and then display the lists of
            // matching videos, channels, and playlists.
            foreach (var searchResult in searchListResponse.Items)
            {
                switch (searchResult.Id.Kind)
                {
                    case "youtube#video":
                        Putallthethingshere.Children.Add(new YoutubeItem(searchResult.Snippet.Thumbnails.Medium.Url, searchResult.Snippet.Title, searchResult.Snippet.ChannelTitle, searchResult.Id.VideoId));
                        break;

                    case "youtube#channel":
                        channels.Add(String.Format("{0} ({1})", searchResult.Snippet.Title, searchResult.Id.ChannelId));
                        break;
                }
            }

            foreach (YoutubeItem obj in Putallthethingshere.Children)
            {
                obj.ButtonClick += new EventHandler<YoutubeEventArgs>(YoutubeButtonClick);
            }

            Console.WriteLine("Done");
        }

        private void GetSubscriptions()
        {
            var youtubeService = new YouTubeService(new BaseClientService.Initializer()
            {
                ApiKey = "AIzaSyCXOZJH2GUbdqwxZwsjTU93lFvgdnMOVD0",
                ApplicationName = this.GetType().ToString()
            });

            var subscriptions = youtubeService.Subscriptions.List("snippet,contentDetails");
            subscriptions.OauthToken = "-Rn1-zDrvIFolkFBxm8zSwbB";
            subscriptions.ChannelId = "UCfYSGOxQeqO4AaM7eKryjeQ";

            var nice = subscriptions.Execute();
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
            if (FullSizedMediaElement == true)
            {
                var animationWidth = new DoubleAnimation();
                animationWidth.From = viewer.ActualWidth;
                animationWidth.To = 640;
                animationWidth.EnableDependentAnimation = true;
                animationWidth.Duration = TimeSpan.FromSeconds(0.5);
                var storyboard = new Storyboard();
                Storyboard.SetTargetProperty(animationWidth, "Width");
                Storyboard.SetTarget(animationWidth, MediaElementContainer);

                var animationHeight = new DoubleAnimation();
                animationHeight.From = viewer.ActualHeight;
                animationHeight.To = 360;
                animationHeight.EnableDependentAnimation = true;
                animationHeight.Duration = TimeSpan.FromSeconds(0.5);
                Storyboard.SetTargetProperty(animationHeight, "Height");
                Storyboard.SetTarget(animationHeight, MediaElementContainer);

                storyboard.Children.Add(animationWidth);
                storyboard.Children.Add(animationHeight);
                storyboard.Begin();

                FullSizedMediaElement = false;
            }
            else
            {
                var animationWidth = new DoubleAnimation();
                animationWidth.From = viewer.ActualWidth;
                animationWidth.To = ActualWidth;
                animationWidth.EnableDependentAnimation = true;
                animationWidth.Duration = TimeSpan.FromSeconds(0.5);
                var storyboard = new Storyboard();
                Storyboard.SetTargetProperty(animationWidth, "Width");
                Storyboard.SetTarget(animationWidth, MediaElementContainer);

                var animationHeight = new DoubleAnimation();
                animationHeight.From = viewer.ActualHeight;
                animationHeight.To = ActualHeight;
                animationHeight.EnableDependentAnimation = true;
                animationHeight.Duration = TimeSpan.FromSeconds(0.5);
                Storyboard.SetTargetProperty(animationHeight, "Height");
                Storyboard.SetTarget(animationHeight, MediaElementContainer);

                storyboard.Children.Add(animationWidth);
                storyboard.Children.Add(animationHeight);
                storyboard.Completed += Storyboard_Completed;
                storyboard.Begin();

                FullSizedMediaElement = true;
            }


            ScrollView.Visibility = Visibility.Visible;
            //viewer.Source = new Uri("about:blank");
        }

        private void Storyboard_Completed(object sender, object e)
        {
            MediaElementContainer.Height = Double.NaN;
            MediaElementContainer.Width = Double.NaN;
        }

        #endregion
        #region Events

        protected void YoutubeButtonClick(object sender, YoutubeEventArgs e)
        {
            try
            {
                var youTube = YouTube.Default;
                var video = youTube.GetVideo(e.URL);
                StartVideo(video.Uri);

                YoutubeLink = e.URL;
                YoutubeID = e.ID;
            }
            catch { }
        }


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
    }
}

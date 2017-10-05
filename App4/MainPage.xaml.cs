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

namespace App4
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public string YoutubeLink = "";

        public MainPage()
        {
            this.InitializeComponent();
        }

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

        private async void GetSubscriptions()
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

        public void StartVideo(string URL)
        {
            viewer.Visibility = Visibility.Visible;
            ScrollView.Visibility = Visibility.Collapsed;
            viewer.Source = new Uri(URL);
        }

        public void StopVideo()
        {
            viewer.Visibility = Visibility.Collapsed;
            ScrollView.Visibility = Visibility.Visible;
            viewer.Source = new Uri("about:Blank");
        }

        protected void YoutubeButtonClick(object sender, YoutubeEventArgs e)
        {
            try
            {
                var youTube = YouTube.Default;
                var video = youTube.GetVideo(e.URL);
                StartVideo(video.Uri);
            }
            catch { }
        }

        private void viewer_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            CloseMediaElement.Visibility = Visibility.Visible;
        }

        private void viewer_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            CloseMediaElement.Visibility = Visibility.Collapsed;
        }

        private void CloseMediaElement_Click(object sender, RoutedEventArgs e)
        {
            StopVideo();
        }
    }
}

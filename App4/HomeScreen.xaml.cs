using Google.Apis.Services;
using Google.Apis.YouTube.v3;
using Google.Apis.YouTube.v3.Data;
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

namespace YTApp
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class HomeScreen : Page
    {
        public HomeScreen()
        {
            InitializeComponent();
            GetSubscriptionVideos(GetSubscriptions());
        }

        private SubscriptionListResponse GetSubscriptions()
        {
            var youtubeService = new YouTubeService(new BaseClientService.Initializer()
            {
                ApiKey = "AIzaSyCXOZJH2GUbdqwxZwsjTU93lFvgdnMOVD0",
                ApplicationName = this.GetType().ToString()
            });

            var subscriptions = youtubeService.Subscriptions.List("snippet,contentDetails");
            subscriptions.ChannelId = "UCfYSGOxQeqO4AaM7eKryjeQ";
            subscriptions.MaxResults = 50;

            return subscriptions.Execute();
        }

        private void GetSubscriptionVideos(SubscriptionListResponse subscriptionList)
        {
            List<ChannelListResponse> youtubeVideos = new List<ChannelListResponse>();
            foreach (Subscription sub in subscriptionList.Items)
            {
                var youtubeService = new YouTubeService(new BaseClientService.Initializer()
                {
                    ApiKey = "AIzaSyCXOZJH2GUbdqwxZwsjTU93lFvgdnMOVD0",
                    ApplicationName = this.GetType().ToString()
                });
                var video = youtubeService.Channels.List("snippet");
                video.Id = sub.Snippet.ChannelId;
                youtubeVideos.Add(video.Execute());
            }
            Console.WriteLine("breakpoint");
        }
    }
}

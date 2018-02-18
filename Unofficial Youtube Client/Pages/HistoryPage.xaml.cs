using Google.Apis.Auth.OAuth2;
using Google.Apis.Oauth2.v2;
using Google.Apis.Services;
using Google.Apis.YouTube.v3;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using YTApp.Classes;
using YTApp.Classes.DataTypes;
using YTApp.Classes.EventsArgs;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace YTApp.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class HistoryPage : Page
    {
        MainPage MainPageReference;

        public HistoryPage()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            NavigateParams result = (NavigateParams)e.Parameter;
            base.OnNavigatedTo(e);
            MainPageReference = result.MainPageRef;
            UpdateVideos();
        }

        public async void UpdateVideos()
        {
            List<YoutubeItemDataType> tempList = new List<YoutubeItemDataType>();

            YoutubeItemMethods methods = new YoutubeItemMethods();

            UserCredential credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(new ClientSecrets
            {
                ClientId = "957928808020-pa0lopl3crh565k6jd4djaj36rm1d9i5.apps.googleusercontent.com",
                ClientSecret = "oB9U6yWFndnBqLKIRSA0nYGm"
            }, new[] { YouTubeService.Scope.Youtube }, "user", CancellationToken.None);

            var youtubeService = new YouTubeService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = "Youtube Viewer",
            });

            //Gets our channel which has a playlist called Watch History
            var getChannel = youtubeService.Channels.List("contentDetails");
            getChannel.Mine = true;

            var myChannel = await getChannel.ExecuteAsync();

            //Get the playlist items from the watch history playlist that we got from our channel
            var getWatchHistoryItems = youtubeService.PlaylistItems.List("snippet");
            getWatchHistoryItems.PlaylistId = myChannel.Items[0].ContentDetails.RelatedPlaylists.WatchHistory;
            getWatchHistoryItems.MaxResults = 50;

            var watchHistory = await getWatchHistoryItems.ExecuteAsync();


            // Call the search.list method to retrieve results matching the specified query term.
            /*var searchListResponse = await searchListRequest.ExecuteAsync();

            foreach (var video in searchListResponse.Items)
            {
                tempList.Add(methods.VideoToYoutubeItem(video));
            }
            methods.FillInViews(tempList, youtubeService);

            nextPageToken = searchListResponse.NextPageToken;

            foreach (var item in tempList)
            {
                VideosList.Add(item);
            }*/
        }
    }
}

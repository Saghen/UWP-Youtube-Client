using Google.Apis.Services;
using Google.Apis.YouTube.v3;
using Google.Apis.YouTube.v3.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using VideoLibrary;
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

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace YTApp.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class SearchPage : Page
    {
        public MainPage MainPageReference;

        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            NavigateParams result = (NavigateParams)e.Parameter;
            base.OnNavigatedTo(e);
            MainPageReference = result.mainPageRef;
            await Run();
            try { await Run(); } catch { }
        }

        public SearchPage()
        {
            this.InitializeComponent();
        }

        private void YoutubeItemsGridView_ItemClick(object sender, ItemClickEventArgs e)
        {
            if (e.ClickedItem.GetType() == typeof(YoutubeItemDataType))
            {
                var item = (YoutubeItemDataType)e.ClickedItem;
                YoutubeItemMethodsStatic.StartVideo(item.Id, MainPageReference);
            }
            else if (e.ClickedItem.GetType() == typeof(YoutubeChannelDataType))
            {
                var item = (YoutubeChannelDataType)e.ClickedItem;
                MainPageReference.contentFrame.Navigate(typeof(ChannelPage), new NavigateParams() { mainPageRef = MainPageReference, ID = item.Id });
            }
        }

        #region Search
        private async Task Run()
        {
            var youtubeService = new YouTubeService(new BaseClientService.Initializer()
            {
                ApiKey = "AIzaSyCXOZJH2GUbdqwxZwsjTU93lFvgdnMOVD0",
                ApplicationName = this.GetType().ToString()
            });

            var searchListRequest = youtubeService.Search.List("snippet");
            searchListRequest.Q = MainPageReference.SearchBox.Text;
            searchListRequest.MaxResults = 50;

            // Call the search.list method to retrieve results matching the specified query term.
            var searchListResponse = await searchListRequest.ExecuteAsync();

            string VideoIDs = "";
            foreach (var searchResult in searchListResponse.Items) { VideoIDs += searchResult.Id.VideoId + ","; }
            var getViewsRequest = youtubeService.Videos.List("statistics");
            getViewsRequest.Id = VideoIDs.Remove(VideoIDs.Length - 1);

            var videoListResponse = await getViewsRequest.ExecuteAsync();
            List<string> VideoIDsSplit = VideoIDs.Split(new String[] { "," }, StringSplitOptions.RemoveEmptyEntries).ToList();

            YoutubeItemsGridView.Items.Clear();

            // Add each result to the ListView
            foreach (var searchResult in searchListResponse.Items)
            {
                if (searchResult.Id.Kind == "youtube#video")
                {
                    var methods = new YoutubeItemMethods();
                    var data = methods.VideoToYoutubeItem(searchResult);
                    YoutubeItemsGridView.Items.Add(data);
                }
                else if (searchResult.Id.Kind == "youtube#channel")
                {
                    var methods = new YoutubeItemMethods();
                    var data = methods.ChannelToYoutubeChannel(searchResult, youtubeService);
                    YoutubeItemsGridView.Items.Add(data);
                }     
            }
        }

        private string ViewCountShortner(ulong? viewCount)
        {
            if (viewCount > 1000000)
            {
                return Convert.ToString(Math.Round(Convert.ToDouble(viewCount / 1000000), 1)) + "M";
            }
            else if (viewCount > 1000)
            {
                return Convert.ToString(Math.Round(Convert.ToDouble(viewCount / 1000), 1)) + "K";
            }
            else
            {
                return Convert.ToString(viewCount);
            }
        }

        private string TimeSinceDate(DateTime? date)
        {
            try
            {
                TimeSpan ts = DateTime.Now.Subtract(Convert.ToDateTime(date));
                if (ts.TotalDays > 365)
                    return String.Format("{0} years ago", (int)ts.TotalDays / 365);
                else if (ts.TotalDays > 30)
                    return String.Format("{0} months ago", (int)ts.TotalDays / 30);
                else if (ts.TotalDays > 1)
                    return String.Format("{0} days ago", (int)ts.TotalDays);
                else if (ts.TotalHours > 1)
                    return String.Format("{0} hours ago", (int)ts.TotalHours);
                else if (ts.TotalMinutes > 1)
                    return String.Format("{0} minutes ago", (int)ts.TotalMinutes);
                else
                    return String.Format("{0} seconds ago", (int)ts.TotalSeconds);
            }
            catch { return "unkown date"; }
        }
        #endregion

    }
}

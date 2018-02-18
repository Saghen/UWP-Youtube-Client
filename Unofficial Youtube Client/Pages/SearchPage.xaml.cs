using Google.Apis.Services;
using Google.Apis.YouTube.v3;
using Google.Apis.YouTube.v3.Data;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

        private string nextPageToken = "";
        private string searchQuery = "";
        private bool addingVideos = false;

        ObservableCollection<object> SearchResultsList = new ObservableCollection<object>();

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            NavigateParams result = (NavigateParams)e.Parameter;
            base.OnNavigatedTo(e);
            MainPageReference = result.MainPageRef;

            searchQuery = MainPageReference.SearchBox.Text;

            Search();
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
                MainPageReference.StartVideo(item.Id);
            }
            else if (e.ClickedItem.GetType() == typeof(YoutubeChannelDataType))
            {
                var item = (YoutubeChannelDataType)e.ClickedItem;
                this.Frame.Navigate(typeof(ChannelPage), new NavigateParams() { MainPageRef = MainPageReference, ID = item.Id });
            }
        }

        #region Search
        private async void Search()
        {
            SearchResultsList.Clear();

            var youtubeService = await YoutubeItemMethodsStatic.GetServiceAsync();

            var searchListRequest = youtubeService.Search.List("snippet");
            searchListRequest.Q = MainPageReference.SearchBox.Text;
            searchListRequest.MaxResults = 25;

            SearchListResponse searchListResponse = new SearchListResponse();

            // Call the search.list method to retrieve results matching the specified query term.
            searchListResponse = await searchListRequest.ExecuteAsync();

            nextPageToken = searchListResponse.NextPageToken;

            ObservableCollection<YoutubeItemDataType> tempList = new ObservableCollection<YoutubeItemDataType>();

            var methods = new YoutubeItemMethods();

            foreach (var searchResult in searchListResponse.Items)
            {
                if (searchResult.Id.Kind == "youtube#video")
                {
                    var data = methods.VideoToYoutubeItem(searchResult);
                    tempList.Add(data);
                }
                else if (searchResult.Id.Kind == "youtube#channel")
                {
                    var data = methods.ChannelToYoutubeChannel(searchResult, youtubeService);
                    SearchResultsList.Add(data);
                }
            }

            methods.FillInViews(tempList, youtubeService);

            foreach (var item in tempList)
                SearchResultsList.Add(item);
        }

        private async void SearchAddMore()
        {
            var youtubeService = await YoutubeItemMethodsStatic.GetServiceAsync();

            if (addingVideos == true)
                return;

            addingVideos = true;

            var searchListRequest = youtubeService.Search.List("snippet");
            searchListRequest.Q = MainPageReference.SearchBox.Text;
            searchListRequest.PageToken = nextPageToken;
            searchListRequest.MaxResults = 25;

            // Call the search.list method to retrieve results matching the specified query term.
            var searchListResponse = await searchListRequest.ExecuteAsync();

            nextPageToken = searchListResponse.NextPageToken;

            ObservableCollection<YoutubeItemDataType> tempList = new ObservableCollection<YoutubeItemDataType>();

            var methods = new YoutubeItemMethods();

            foreach (var searchResult in searchListResponse.Items)
            {
                if (searchResult.Id.Kind == "youtube#video")
                {
                    var data = methods.VideoToYoutubeItem(searchResult);
                    tempList.Add(data);
                    SearchResultsList.Add(data);
                }
                else if (searchResult.Id.Kind == "youtube#channel")
                {
                    var data = methods.ChannelToYoutubeChannel(searchResult, youtubeService);
                    SearchResultsList.Add(data);
                }
            }

            methods.FillInViews(tempList, youtubeService);

            foreach (var item in tempList)
                SearchResultsList.Add(item);

            addingVideos = false;
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

        private void ScrollView_ViewChanged(object sender, ScrollViewerViewChangedEventArgs e)
        {
            var verticalOffset = ScrollView.VerticalOffset;
            var maxVerticalOffset = ScrollView.ScrollableHeight - 1000; //sv.ExtentHeight - sv.ViewportHeight;

            if (maxVerticalOffset < 0 ||
                verticalOffset >= maxVerticalOffset)
            {
                SearchAddMore();
            }
        }
    }
}

using Google.Apis.YouTube.v3.Data;
using System;
using System.Collections.ObjectModel;
using Windows.UI.Xaml.Controls;
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
        private string nextPageToken = "";
        private bool addingVideos = false;

        ObservableCollection<object> SearchResultsList = new ObservableCollection<object>();

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
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
                var listView = (ListView)sender;
                listView.PrepareConnectedAnimation("videoThumb", item, "ImageControl");
                Constants.MainPageRef.StartVideo(item.Id);
            }
            else if (e.ClickedItem.GetType() == typeof(YoutubeChannelDataType))
            {
                var item = (YoutubeChannelDataType)e.ClickedItem;
                Constants.activeChannelID = item.Id;
                this.Frame.Navigate(typeof(ChannelPage));
            }
        }

        private async void Search()
        {
            SearchResultsList.Clear();

            var youtubeService = await YoutubeMethodsStatic.GetServiceAsync();

            var searchListRequest = youtubeService.Search.List("snippet");
            searchListRequest.Q = Constants.MainPageRef.SearchBox.Text;
            searchListRequest.MaxResults = 25;

            SearchListResponse searchListResponse = new SearchListResponse();

            // Call the search.list method to retrieve results matching the specified query term.
            searchListResponse = await searchListRequest.ExecuteAsync();

            nextPageToken = searchListResponse.NextPageToken;

            ObservableCollection<YoutubeItemDataType> tempList = new ObservableCollection<YoutubeItemDataType>();

            var methods = new YoutubeMethods();

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
            var youtubeService = await YoutubeMethodsStatic.GetServiceAsync();

            if (addingVideos == true)
                return;

            addingVideos = true;

            var searchListRequest = youtubeService.Search.List("snippet");
            searchListRequest.Q = Constants.MainPageRef.SearchBox.Text;
            searchListRequest.PageToken = nextPageToken;
            searchListRequest.MaxResults = 25;

            // Call the search.list method to retrieve results matching the specified query term.
            var searchListResponse = await searchListRequest.ExecuteAsync();

            nextPageToken = searchListResponse.NextPageToken;

            ObservableCollection<YoutubeItemDataType> tempList = new ObservableCollection<YoutubeItemDataType>();

            var methods = new YoutubeMethods();

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

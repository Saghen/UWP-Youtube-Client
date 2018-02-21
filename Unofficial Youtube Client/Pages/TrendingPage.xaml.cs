using Google.Apis.YouTube.v3;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using YTApp.Classes;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace YTApp.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class TrendingPage : Page
    {
        System.Collections.ObjectModel.ObservableCollection<YoutubeItemDataType> videosList = new System.Collections.ObjectModel.ObservableCollection<YoutubeItemDataType>();

        public TrendingPage()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            UpdateVideos();
        }

        public async void UpdateVideos()
        {
            YoutubeMethods methods = new YoutubeMethods();

            var service = await YoutubeMethodsStatic.GetServiceAsync();

            var recommendations = service.Videos.List("snippet, contentDetails");
            recommendations.Chart = VideosResource.ListRequest.ChartEnum.MostPopular;
            recommendations.MaxResults = 25;
            var result = await recommendations.ExecuteAsync();

            foreach(var video in result.Items)
            {
                videosList.Add(methods.VideoToYoutubeItem(video));
            }

            methods.FillInViews(videosList, service);
        }

        private void YoutubeItemsGridView_ItemClick(object sender, ItemClickEventArgs e)
        {
            var item = (YoutubeItemDataType)e.ClickedItem;
            var listView = (ListView)sender;
            listView.PrepareConnectedAnimation("videoThumb", item, "ImageControl");
            Constants.MainPageRef.StartVideo(item.Id);
        }
    }
}

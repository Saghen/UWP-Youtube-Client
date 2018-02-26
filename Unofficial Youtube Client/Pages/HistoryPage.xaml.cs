using System;
using System.Collections.ObjectModel;
using Windows.UI.Xaml.Controls;
using YTApp.Classes;
using YTApp.Classes.DataTypes;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace YTApp.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class HistoryPage : Page
    {
        ObservableCollection<YoutubeItemDataType> videosList = new ObservableCollection<YoutubeItemDataType>(Constants.syncedData.history);

        public HistoryPage()
        {
            this.InitializeComponent();
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

using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.YouTube.v3;
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
using YTApp.Classes;
using YTApp.Classes.DataTypes;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace YTApp.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class TrendingPage : Page
    {
        MainPage MainPageReference;

        System.Collections.ObjectModel.ObservableCollection<YoutubeItemDataType> videosList = new System.Collections.ObjectModel.ObservableCollection<YoutubeItemDataType>();

        public TrendingPage()
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
            YoutubeItemMethods methods = new YoutubeItemMethods();

            var service = await YoutubeItemMethodsStatic.GetServiceAsync();

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
            MainPageReference.StartVideo(item.Id);
        }
    }
}

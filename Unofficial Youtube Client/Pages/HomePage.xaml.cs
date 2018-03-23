using Google.Apis.Services;
using Google.Apis.YouTube.v3;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using YTApp.Classes;
using YTApp.Classes.DataTypes;
using MetroLog;
using Newtonsoft.Json;
using Windows.UI.Xaml.Media.Animation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace YTApp.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class HomePage : Page
    {
        private ILogger Log = LogManagerFactory.DefaultLogManager.GetLogger<MainPage>();

        private ObservableCollection<PlaylistDataType> YTItems = new ObservableCollection<PlaylistDataType>();

        public HomePage()
        {
            //Use custom page transition
            TransitionCollection collection = new TransitionCollection();
            NavigationThemeTransition theme = new NavigationThemeTransition();
            var info = new EntranceNavigationTransitionInfo();
            theme.DefaultNavigationTransitionInfo = info;
            collection.Add(theme);
            this.Transitions = collection;

            //Initialize page
            this.InitializeComponent();

            //Keep the page in memory so we don't have to reload it everytime
            NavigationCacheMode = NavigationCacheMode.Enabled;

            UpdateHomeItems();
        }

        private void YoutubeItemsGridView_ItemClick(object sender, ItemClickEventArgs e)
        {
            var item = (YoutubeItemDataType)e.ClickedItem;
            var gridView = (GridView)sender;
            gridView.PrepareConnectedAnimation("videoThumb", item, "ImageControl");
            Constants.MainPageRef.StartVideo(item.Id);
        }

        private async void UpdateHomeItems()
        {
            #region Subscriptions

            Log.Info("Updating the videos on the home page");

            PlaylistDataType YTItemsListTemp = new PlaylistDataType() { Title = "Today" };
            PlaylistDataType YTItemsListTempYesterday = new PlaylistDataType() { Title = "Yesterday" };
            PlaylistDataType YTItemsListTempTwoDays = new PlaylistDataType() { Title = "Two Days Ago" };
            PlaylistDataType YTItemsListTempThreeDays = new PlaylistDataType() { Title = "Three Days Ago" };
            PlaylistDataType YTItemsListTempFourDays = new PlaylistDataType() { Title = "Four Days Ago" };
            PlaylistDataType YTItemsListTempFiveDays = new PlaylistDataType() { Title = "Five Days Ago" };

            System.Collections.Concurrent.BlockingCollection<Google.Apis.YouTube.v3.Data.SearchResult> searchResponseList = new System.Collections.Concurrent.BlockingCollection<Google.Apis.YouTube.v3.Data.SearchResult>();

            var service = await YoutubeMethodsStatic.GetServiceAsync();

            await Task.Run(() =>
            {
                Parallel.ForEach(Constants.MainPageRef.subscriptionsList, subscription =>
                {
                    try
                    {
                        var tempService = service.Search.List("snippet");
                        tempService.ChannelId = subscription.Id;
                        tempService.Order = SearchResource.ListRequest.OrderEnum.Date;
                        tempService.MaxResults = 8;
                        var response = tempService.Execute();
                        foreach (var video in response.Items)
                        {
                            searchResponseList.Add(video);
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.Error("A subscription's videos failed to load.");
                        subscription.Thumbnail = null;
                        Log.Error(JsonConvert.SerializeObject(subscription));
                        Log.Error(ex.Message);
                    }
                });
            });

            var orderedSearchResponseList = searchResponseList.OrderByDescending(x => x.Snippet.PublishedAt).ToList();

            Log.Info("Ordering videos by date and placing them in the correct list");
            foreach (var video in orderedSearchResponseList)
            {
                var methods = new YoutubeMethods();
                if (video != null && video.Id.Kind == "youtube#video" && video.Id.VideoId != null && video.Snippet.LiveBroadcastContent != "live")
                {
                    try
                    {
                        DateTime now = DateTime.Now;
                        var ytubeItem = methods.VideoToYoutubeItem(video);
                        if (ytubeItem.Failed != true)
                        {
                            if (video.Snippet.PublishedAt > now.AddHours(-24))
                            {
                                YTItemsListTemp.Items.Add(ytubeItem);
                            }
                            else if (video.Snippet.PublishedAt > now.AddHours(-48))
                            {
                                YTItemsListTempYesterday.Items.Add(ytubeItem);
                            }
                            else if (video.Snippet.PublishedAt > now.AddHours(-72))
                            {
                                YTItemsListTempTwoDays.Items.Add(ytubeItem);
                            }
                            else if (video.Snippet.PublishedAt > now.AddHours(-96))
                            {
                                YTItemsListTempThreeDays.Items.Add(ytubeItem);
                            }
                            else if (video.Snippet.PublishedAt > now.AddHours(-120))
                            {
                                YTItemsListTempFourDays.Items.Add(ytubeItem);
                            }
                            else if (video.Snippet.PublishedAt > now.AddHours(-144) && video.Snippet.PublishedAt <= now)
                            {
                                YTItemsListTempFiveDays.Items.Add(ytubeItem);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.Error(String.Format("A video failed to load into the home page. Json: {0}", JsonConvert.SerializeObject(video)));
                        Log.Error(ex.Message);
                    }
                }
            }

            YTItems.Add(YTItemsListTemp);
            YTItems.Add(YTItemsListTempYesterday);
            YTItems.Add(YTItemsListTempTwoDays);
            YTItems.Add(YTItemsListTempThreeDays);
            YTItems.Add(YTItemsListTempFourDays);
            YTItems.Add(YTItemsListTempFiveDays);
            #endregion

            LoadingRing.IsActive = false;

            Parallel.ForEach(YTItems, playlist =>
            {
                var methodsLocal = new YoutubeMethods();
                methodsLocal.FillInViews(playlist.Items, service);
            });
        }
    }
}

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

        public bool isLoaded = false;

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
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            //Check if we need to update 
            if (isLoaded == false)
            {
                UpdateHomeItems();
            }
            isLoaded = true;
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

            var service = await YoutubeItemMethodsStatic.GetServiceAsync();

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
                var methods = new YoutubeItemMethods();
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
                var methodsLocal = new YoutubeItemMethods();
                methodsLocal.FillInViews(playlist.Items, service);
            });
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

        private async Task Run()
        {
            var youtubeService = new YouTubeService(new BaseClientService.Initializer()
            {
                ApiKey = "AIzaSyCXOZJH2GUbdqwxZwsjTU93lFvgdnMOVD0",
                ApplicationName = this.GetType().ToString()
            });

            var RecommendedVideosRequest = youtubeService.Activities.List("snippet");
            RecommendedVideosRequest.Mine = true;
            RecommendedVideosRequest.OauthToken = App.OAuthCode;

            var RecommendedVideosResults = await RecommendedVideosRequest.ExecuteAsync();

            string VideoIDs = "";
            //foreach (var Result in RecommendedVideosResults.Items) { VideoIDs += Result.Id. + ","; }
            var getViewsRequest = youtubeService.Videos.List("statistics");
            getViewsRequest.Id = VideoIDs.Remove(VideoIDs.Length - 1);

            var videoListResponse = await getViewsRequest.ExecuteAsync();
            List<string> VideoIDsSplit = VideoIDs.Split(new String[] { "," }, StringSplitOptions.RemoveEmptyEntries).ToList();

            foreach (var result in RecommendedVideosResults.Items)
            {
            }
        }
    }
}

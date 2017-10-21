using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.YouTube.v3;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
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
using static YTApp.MainPage;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace YTApp.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ChannelPage : Page
    {
        public MainPage MainPageReference;
        public string ChannelID;

        public ChannelPage()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            Params result = (Params)e.Parameter;
            base.OnNavigatedTo(e);
            MainPageReference = result.mainPageRef;
            ChannelID = result.ID;
            UpdateChannel();
        }

        public async void UpdateChannel()
        {
            var methods = new YoutubeItemMethods();
            List<YoutubeItemDataType> YoutubeItemsTemp = new List<YoutubeItemDataType>();

            UserCredential credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(new ClientSecrets
            {
                ClientId = "957928808020-pa0lopl3crh565k6jd4djaj36rm1d9i5.apps.googleusercontent.com",
                ClientSecret = "oB9U6yWFndnBqLKIRSA0nYGm"
            }, new[] { YouTubeService.Scope.Youtube }, "user", System.Threading.CancellationToken.None);

            // Create the service.
            var service = new YouTubeService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = "Youtube Viewer",
            });

            #region Uploads
            var GetChannelVideosUploads = service.Search.List("snippet");
            GetChannelVideosUploads.ChannelId = ChannelID;
            GetChannelVideosUploads.Order = SearchResource.ListRequest.OrderEnum.Date;
            GetChannelVideosUploads.MaxResults = 10;
            var ChannelVideosResultUploads = GetChannelVideosUploads.Execute();
            foreach (var video in ChannelVideosResultUploads.Items)
            {
                YoutubeItemsTemp.Add(methods.VideoToYoutubeItem(video));
            }
            methods.FillInViews(YoutubeItemsTemp, service);
            var PlaylistUserControlUploads = new ChannelPlaylistGridView(YoutubeItemsTemp);
            //HomeGridView.Children.Add(PlaylistUserControlUploads);
            #endregion

            #region Popular Uploads
            YoutubeItemsTemp.Clear();
            var GetChannelVideosPopular = service.Search.List("snippet");
            GetChannelVideosPopular.ChannelId = ChannelID;
            GetChannelVideosPopular.Order = SearchResource.ListRequest.OrderEnum.ViewCount;
            GetChannelVideosPopular.MaxResults = 10;
            var ChannelVideosResultPopular = GetChannelVideosPopular.Execute();
            foreach (var video in ChannelVideosResultPopular.Items)
            {
                YoutubeItemsTemp.Add(methods.VideoToYoutubeItem(video));
            }
            methods.FillInViews(YoutubeItemsTemp, service);
            var PlaylistUserControlPopular = new ChannelPlaylistGridView(YoutubeItemsTemp);
            //HomeGridView.Children.Add(PlaylistUserControlPopular);
            #endregion

            #region Playlists
            //Get the playlists for the channel
            var GetChannelPlaylists = service.Playlists.List("snippet");
            GetChannelPlaylists.ChannelId = ChannelID;
            GetChannelPlaylists.MaxResults = 3;
            var ChannelPlaylistsResult = GetChannelPlaylists.Execute();

            //Go through each playlist and get all its items
            foreach(var playlist in ChannelPlaylistsResult.Items)
            {
                YoutubeItemsTemp.Clear();
                var GetPlaylistVideos = service.PlaylistItems.List("snippet");
                GetPlaylistVideos.Id = playlist.Id;
                GetPlaylistVideos.MaxResults = 10;
                var PlaylistVideosResult = GetPlaylistVideos.Execute();
                if(PlaylistVideosResult.Items.Count == 0) { break; }
                foreach (var video in PlaylistVideosResult.Items)
                {
                    YoutubeItemsTemp.Add(methods.VideoToYoutubeItem(video));
                }
                methods.FillInViews(YoutubeItemsTemp, service);
                var PlaylistUserControlPlaylist = new ChannelPlaylistGridView(YoutubeItemsTemp);
                //HomeGridView.Children.Add(PlaylistUserControlPlaylist);
            }
            #endregion
        }
    }
}

using Google.Apis.Drive.v3;
using Google.Apis.Drive.v3.Data;
using Google.Apis.YouTube.v3;
using Google.Apis.YouTube.v3.Data;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using YoutubeExplode.Models.MediaStreams;

namespace YTApp.Classes
{
    static class Constants
    {
        static public readonly string ClientID = "856342177720-ae8rhd1r35gocq5dc7l3509tp123koe1.apps.googleusercontent.com";
        static public readonly string ClientSecret = "GA1G-NL20O5JTXtZZbbKrd4u";
        static public readonly string ApiKey = "AIzaSyAclS0aOrMI8W2uo5Gb2VMcH9OtX2cmgPg";
        static public Google.Apis.Auth.OAuth2.Responses.TokenResponse Token;

        static public MainPage MainPageRef;

        static private Video activeVideo = new Video();
        static public Video ActiveVideo
        {
            get => activeVideo;
            set
            {
                activeVideo = value;

                //Find if the video has already been watched and remove it if it has
                List<DataTypes.YoutubeItemDataType> list;
                if (syncedData.history.Count > 200)
                    list = syncedData.history.GetRange(0, 200);
                else
                    list = syncedData.history;

                var item = list.Find(x => x.Id == activeVideo.Id);
                syncedData.history.Insert(0, new YoutubeMethods().VideoToYoutubeItem(activeVideo));
                if (item != null)
                {
                    list.Remove(item);
                }

                //Store the new data to the roaming data store
                StoreAppData();
            }
        }

        static public string activeVideoID;

        static public Google.Apis.YouTube.v3.Data.Channel activeChannel = new Google.Apis.YouTube.v3.Data.Channel();
        static public string activeChannelID;

        static public MediaStreamInfoSet videoInfo;

        static public DataTypes.SyncedApplicationDataType syncedData = new DataTypes.SyncedApplicationDataType() { DarkTheme = true, history = new List<DataTypes.YoutubeItemDataType>() };

        static async public void StoreAppData()
        {
            StorageFolder roamingFolder = ApplicationData.Current.RoamingFolder;
            var file = await roamingFolder.CreateFileAsync("data.json", CreationCollisionOption.ReplaceExisting);

            var what = JsonConvert.SerializeObject(syncedData);

            await FileIO.WriteTextAsync(file, JsonConvert.SerializeObject(syncedData));
        }
    }
}

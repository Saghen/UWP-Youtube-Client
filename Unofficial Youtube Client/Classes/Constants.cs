using Google.Apis.Drive.v2;
using Google.Apis.Drive.v2.Data;
using Google.Apis.YouTube.v3;
using Google.Apis.YouTube.v3.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YoutubeExplode.Models.MediaStreams;

namespace YTApp.Classes
{
    static class Constants
    {
        static public readonly string ClientID = "856342177720-ae8rhd1r35gocq5dc7l3509tp123koe1.apps.googleusercontent.com";
        static public readonly string ClientSecret = "GA1G-NL20O5JTXtZZbbKrd4u ";
        static public Google.Apis.Auth.OAuth2.Responses.TokenResponse Token;

        static public MainPage MainPageRef;

        static private Video activeVideo = new Video();
        static public string activeVideoID;

        static public Google.Apis.YouTube.v3.Data.Channel activeChannel = new Google.Apis.YouTube.v3.Data.Channel();
        static public string activeChannelID;

        static public MediaStreamInfoSet videoInfo;

        static public DataTypes.SyncedApplicationDataType syncedData = new DataTypes.SyncedApplicationDataType() { DarkTheme = true, history = new List<YoutubeItemDataType>() };

        public static Video ActiveVideo { get => activeVideo; set { activeVideo = value; SaveVideoToHistory(); } }

        static async public void SaveVideoToHistory()
        {
            var methods = new YoutubeMethods();
            //Save the video in history
            syncedData.history.Insert(0, methods.VideoToYoutubeItem(activeVideo));

            //Check if history is too long
            while (syncedData.history.Count > 500)
                syncedData.history.RemoveAt(syncedData.history.Count - 1);

            var credential = await Google.Apis.Auth.OAuth2.GoogleWebAuthorizationBroker.AuthorizeAsync(new Google.Apis.Auth.OAuth2.ClientSecrets
            {
                ClientId = "957928808020-pa0lopl3crh565k6jd4djaj36rm1d9i5.apps.googleusercontent.com",
                ClientSecret = "oB9U6yWFndnBqLKIRSA0nYGm"
            }, new[] { DriveService.Scope.DriveAppdata }, "user", System.Threading.CancellationToken.None);

            var service = new DriveService(new Google.Apis.Services.BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = "Unofficial Youtube Client",
            });

            var fileMetadata = new Google.Apis.Drive.v2.Data.File()
            {
                Title = "config.json",
                AppDataContents = true
            };

            var request = service.Files.Insert(fileMetadata, GenerateStreamFromString(Newtonsoft.Json.JsonConvert.SerializeObject(syncedData)), "");
            await request.UploadAsync();
        }

        public static Stream GenerateStreamFromString(string s)
        {
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            writer.Write(s);
            writer.Flush();
            stream.Position = 0;
            return stream;
        }
    }
}

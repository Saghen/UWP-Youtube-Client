using Google.Apis.YouTube.v3;
using Google.Apis.YouTube.v3.Data;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YTApp.Classes
{
    class YoutubeItemMethods
    {
        public YoutubeItemDataType VideoToYoutubeItem(SearchResult video)
        {
            var VideoToAdd = new YoutubeItemDataType();
            VideoToAdd.Author = video.Snippet.ChannelTitle;
            VideoToAdd.Description = video.Snippet.Description;
            VideoToAdd.Thumbnail = video.Snippet.Thumbnails.Medium.Url;
            VideoToAdd.Title = video.Snippet.Title;
            VideoToAdd.Id = video.Id.VideoId;
            VideoToAdd.Ylink = "https://www.youtube.com/watch?v=" + video.Id.VideoId;
            VideoToAdd.ViewsAndDate = " Views • " + TimeSinceDate(video.Snippet.PublishedAt);
            VideoToAdd.DateSubmitted = video.Snippet.PublishedAt.Value;
            if (VideoToAdd == null)
            { Console.WriteLine("Hi"); }
            return VideoToAdd;
        }

        public void FillInViews(ObservableCollection<YoutubeItemDataType> collection, YouTubeService service)
        {
            if (collection.Count <= 0) return;

            string VideoIDs = "";
            foreach (var video in collection) { VideoIDs += video.Id + ","; }
            var getViewsRequest = service.Videos.List("statistics");
            getViewsRequest.Id = VideoIDs.Remove(VideoIDs.Length - 1);

            var videoListResponse = getViewsRequest.Execute();

            for (int i = 0; i < collection.Count; i++)
            {
                collection[i].ViewsAndDate = ViewCountShortner(videoListResponse.Items[i].Statistics.ViewCount) + collection[i].ViewsAndDate;
            }
        }

        public string ViewCountShortner(ulong? viewCount)
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

        public string TimeSinceDate(DateTime? date)
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
    }
}

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
            return VideoToAdd;
        }

        public YoutubeItemDataType VideoToYoutubeItem(PlaylistItem video)
        {
            var VideoToAdd = new YoutubeItemDataType();
            VideoToAdd.Author = video.Snippet.ChannelTitle;
            VideoToAdd.Description = video.Snippet.Description;
            try { VideoToAdd.Thumbnail = video.Snippet.Thumbnails.Medium.Url; }
            catch { VideoToAdd.Thumbnail = video.Snippet.Thumbnails.High.Url; }
            VideoToAdd.Title = video.Snippet.Title;
            VideoToAdd.Id = video.Snippet.ResourceId.VideoId;
            VideoToAdd.Ylink = "https://www.youtube.com/watch?v=" + video.Snippet.ResourceId.VideoId;
            VideoToAdd.ViewsAndDate = " Views • " + TimeSinceDate(video.Snippet.PublishedAt);
            VideoToAdd.DateSubmitted = video.Snippet.PublishedAt.Value;
            return VideoToAdd;
        }

        public YoutubeItemDataType VideoToYoutubeItem(Activity video)
        {
            var VideoToAdd = new YoutubeItemDataType();
            VideoToAdd.Author = video.Snippet.ChannelTitle;
            VideoToAdd.Description = video.Snippet.Description;
            try { VideoToAdd.Thumbnail = video.Snippet.Thumbnails.Medium.Url; }
            catch { VideoToAdd.Thumbnail = video.Snippet.Thumbnails.High.Url; }
            VideoToAdd.Title = video.Snippet.Title;
            VideoToAdd.Id = video.ContentDetails.Upload.VideoId;
            VideoToAdd.Ylink = "https://www.youtube.com/watch?v=" + video.ContentDetails.Upload.VideoId;
            VideoToAdd.ViewsAndDate = " Views • " + TimeSinceDate(video.Snippet.PublishedAt);
            VideoToAdd.DateSubmitted = video.Snippet.PublishedAt.Value;
            return VideoToAdd;
        }


        public YoutubeChannelDataType ChannelToYoutubeChannel(SearchResult video, YouTubeService service)
        {
            var getChannelInfo = service.Channels.List("snippet, statistics");
            getChannelInfo.Id = video.Snippet.ChannelId;
            var channelInfo = getChannelInfo.Execute();
            

            var VideoToAdd = new YoutubeChannelDataType();
            VideoToAdd.Description = video.Snippet.Description;
            VideoToAdd.Thumbnail = channelInfo.Items[0].Snippet.Thumbnails.Medium.Url;
            VideoToAdd.Title = video.Snippet.Title;
            VideoToAdd.Id = video.Id.ChannelId;
            VideoToAdd.Ylink = "https://www.youtube.com/watch?v=" + video.Id.VideoId;
            VideoToAdd.SubscribersAndVideos = string.Format("{0:#,###0.#}", channelInfo.Items[0].Statistics.SubscriberCount) + " Subscribers • Videos " + Convert.ToString(channelInfo.Items[0].Statistics.VideoCount);
            return VideoToAdd;
        }

        public void FillInViews(ObservableCollection<YoutubeItemDataType> collection, YouTubeService service)
        {
            if (collection.Count <= 0) return;
            int j = 0;

            string VideoIDs = "";
            foreach (var video in collection)
            {
                if (video == null)
                {
                    collection.RemoveAt(j); break;
                }
                VideoIDs += video.Id + ",";
                j++;
            }
            var getViewsRequest = service.Videos.List("statistics");
            getViewsRequest.Id = VideoIDs.Remove(VideoIDs.Length - 1);

            var videoListResponse = getViewsRequest.Execute();

            for (int i = 0; i < collection.Count; i++)
            {
                collection[i].ViewsAndDate = YoutubeItemMethodsStatic.ViewCountShortner(videoListResponse.Items[i].Statistics.ViewCount) + collection[i].ViewsAndDate;
            }
        }

        public void FillInViews(List<YoutubeItemDataType> collection, YouTubeService service)
        {
            if (collection.Count <= 0) return;

            string VideoIDs = "";
            int j = 0;
            foreach (var video in collection)
            {
                if (video == null)
                {
                    collection.RemoveAt(j); break;
                }
                VideoIDs += video.Id + ",";
                j++;
            }
            var getViewsRequest = service.Videos.List("statistics");
            getViewsRequest.Id = VideoIDs.Remove(VideoIDs.Length - 1);

            var videoListResponse = getViewsRequest.Execute();

            for (int i = 0; i < collection.Count; i++)
            {
                try
                {
                    collection[i].ViewsAndDate = YoutubeItemMethodsStatic.ViewCountShortner(videoListResponse.Items[i].Statistics.ViewCount) + collection[i].ViewsAndDate;
                }
                catch { collection[i].ViewsAndDate = "Unknown" + collection[i].ViewsAndDate; }
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

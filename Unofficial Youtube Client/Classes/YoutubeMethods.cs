using Google.Apis.YouTube.v3;
using Google.Apis.YouTube.v3.Data;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using YTApp.Classes.DataTypes;

namespace YTApp.Classes
{
    class YoutubeMethods
    {
        #region Video To Youtube Item
        public YoutubeItemDataType VideoToYoutubeItem(SearchResult video)
        {
            var VideoToAdd = new YoutubeItemDataType();
            if (video == null) { VideoToAdd.Failed = true; return VideoToAdd; }
            VideoToAdd.Author = video.Snippet.ChannelTitle;
            VideoToAdd.Description = video.Snippet.Description;
            try { VideoToAdd.Thumbnail = video.Snippet.Thumbnails.Medium.Url; }
            catch { VideoToAdd.Thumbnail = video.Snippet.Thumbnails.High.Url; }
            VideoToAdd.Title = video.Snippet.Title;
            VideoToAdd.Id = video.Id.VideoId;
            VideoToAdd.Ylink = "https://www.youtube.com/watch?v=" + video.Id.VideoId;
            VideoToAdd.ViewsAndDate = " Views • " + TimeSinceDate(video.Snippet.PublishedAt);
            VideoToAdd.DateSubmitted = video.Snippet.PublishedAt.Value;
            VideoToAdd.ChanneId = video.Snippet.ChannelId;
            VideoToAdd.WatchTime = GetWatchedTime(video.Id.VideoId);
            return VideoToAdd;
        }

        public YoutubeItemDataType VideoToYoutubeItem(PlaylistItem video)
        {
            var VideoToAdd = new YoutubeItemDataType();
            if (video == null) { VideoToAdd.Failed = true; return VideoToAdd; }
            VideoToAdd.Author = video.Snippet.ChannelTitle;
            VideoToAdd.Description = video.Snippet.Description;
            try { VideoToAdd.Thumbnail = video.Snippet.Thumbnails.Medium.Url; }
            catch { VideoToAdd.Thumbnail = video.Snippet.Thumbnails.High.Url; }
            VideoToAdd.Title = video.Snippet.Title;
            VideoToAdd.Id = video.Snippet.ResourceId.VideoId;
            VideoToAdd.Ylink = "https://www.youtube.com/watch?v=" + video.Snippet.ResourceId.VideoId;
            VideoToAdd.ViewsAndDate = " Views • " + TimeSinceDate(video.Snippet.PublishedAt);
            VideoToAdd.DateSubmitted = video.Snippet.PublishedAt.Value;
            VideoToAdd.ChanneId = video.Snippet.ChannelId;
            VideoToAdd.WatchTime = GetWatchedTime(video.Id);
            return VideoToAdd;
        }

        public YoutubeItemDataType VideoToYoutubeItem(Activity video)
        {
            var VideoToAdd = new YoutubeItemDataType();
            if (video == null) { VideoToAdd.Failed = true; return VideoToAdd; }
            VideoToAdd.Author = video.Snippet.ChannelTitle;
            VideoToAdd.Description = video.Snippet.Description;
            try { VideoToAdd.Thumbnail = video.Snippet.Thumbnails.Medium.Url; }
            catch { VideoToAdd.Thumbnail = video.Snippet.Thumbnails.High.Url; }
            VideoToAdd.Title = video.Snippet.Title;
            VideoToAdd.Id = video.ContentDetails.Upload.VideoId;
            VideoToAdd.Ylink = "https://www.youtube.com/watch?v=" + video.ContentDetails.Upload.VideoId;
            VideoToAdd.ViewsAndDate = " Views • " + TimeSinceDate(video.Snippet.PublishedAt);
            VideoToAdd.DateSubmitted = video.Snippet.PublishedAt.Value;
            VideoToAdd.ChanneId = video.Snippet.ChannelId;
            VideoToAdd.WatchTime = GetWatchedTime(video.Id);
            return VideoToAdd;
        }

        public YoutubeItemDataType VideoToYoutubeItem(Video video)
        {
            var VideoToAdd = new YoutubeItemDataType();
            if (video == null) { VideoToAdd.Failed = true; return VideoToAdd; }
            VideoToAdd.Author = video.Snippet.ChannelTitle;
            VideoToAdd.Description = video.Snippet.Description;
            try { VideoToAdd.Thumbnail = video.Snippet.Thumbnails.Medium.Url; }
            catch { VideoToAdd.Thumbnail = video.Snippet.Thumbnails.High.Url; }
            VideoToAdd.Title = video.Snippet.Title;
            VideoToAdd.Id = video.Id;
            VideoToAdd.Ylink = "https://www.youtube.com/watch?v=" + video.Id;
            VideoToAdd.ViewsAndDate = " Views • " + TimeSinceDate(video.Snippet.PublishedAt);
            VideoToAdd.DateSubmitted = video.Snippet.PublishedAt.Value;
            VideoToAdd.ChanneId = video.Snippet.ChannelId;
            VideoToAdd.WatchTime = GetWatchedTime(video.Id);
            return VideoToAdd;
        }
        #endregion

        #region Channel To Channel Item
        public YoutubeChannelDataType ChannelToYoutubeChannel(SearchResult video, YouTubeService service)
        {
            var getChannelInfo = service.Channels.List("snippet, statistics");
            getChannelInfo.Id = video.Snippet.ChannelId;
            var channelInfo = getChannelInfo.Execute();

            var VideoToAdd = new YoutubeChannelDataType
            {
                Description = video.Snippet.Description,
                Thumbnail = channelInfo.Items[0].Snippet.Thumbnails.Medium.Url,
                Title = video.Snippet.Title,
                Id = video.Id.ChannelId,
                Ylink = "https://www.youtube.com/watch?v=" + video.Id.VideoId,
                SubscribersAndVideos = string.Format("{0:#,###0.#}", channelInfo.Items[0].Statistics.SubscriberCount) + " Subscribers • Videos " + Convert.ToString(channelInfo.Items[0].Statistics.VideoCount)
            };
            return VideoToAdd;
        }

        public YoutubeChannelDataType ChannelToYoutubeChannel(Channel channel)
        {
            var ChannelToAdd = new YoutubeChannelDataType
            {
                Description = channel.Snippet.Description,
                Thumbnail = channel.Snippet.Thumbnails.Medium.Url,
                Title = channel.Snippet.Title,
                Id = channel.Id,
                Subscribers = string.Format("{0:#,###0.#}", channel.Statistics.SubscriberCount) + " Subscribers",
                Videos = Convert.ToString(channel.Statistics.VideoCount) + " Videos"
            };
            return ChannelToAdd;
        }
        #endregion

        #region Comment to Comment Datatype

        public CommentDataType CommentToDataType(CommentThread commentData)
        {
            var comment = new CommentDataType();
            comment.Author = commentData.Snippet.TopLevelComment.Snippet.AuthorDisplayName;
            comment.AuthorThumbnail = commentData.Snippet.TopLevelComment.Snippet.AuthorProfileImageUrl;
            comment.Content = commentData.Snippet.TopLevelComment.Snippet.TextDisplay;
            comment.DatePosted = TimeSinceDate(commentData.Snippet.TopLevelComment.Snippet.PublishedAt);
            comment.Id = commentData.Snippet.TopLevelComment.Id;
            comment.IsReplies = commentData.Replies != null;
            comment.LikeCount = commentData.Snippet.TopLevelComment.Snippet.LikeCount;

            if (commentData.Replies != null)
                comment.ReplyCount = commentData.Replies.Comments.Count;
            else
                comment.ReplyCount = 0;

            if (commentData.Replies != null)
                foreach (var item in commentData.Replies.Comments)
                    comment.Replies.Add(CommentToDataType(item));

            return comment;
        }

        public CommentDataType CommentToDataType(Comment commentData)
        {
            var comment = new CommentDataType();
            comment.Author = commentData.Snippet.AuthorDisplayName;
            comment.AuthorThumbnail = commentData.Snippet.AuthorProfileImageUrl;
            comment.Content = commentData.Snippet.TextDisplay;
            comment.DatePosted = TimeSinceDate(commentData.Snippet.PublishedAt);
            comment.Id = commentData.Id;
            comment.IsReplies = false;
            comment.LikeCount = commentData.Snippet.LikeCount;

            return comment;
        }

        #endregion

        #region Fill in Views
        public Task FillInViewsAsync(ObservableCollection<YoutubeItemDataType> collection, YouTubeService service)
        {
            if (collection.Count <= 0) return null;

            for(int i = 0; i <= (collection.Count - 1)/50; i++)
            {
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

                for (int k = 0; k < collection.Count; k++)
                {
                    try { collection[k + i * 50].ViewsAndDate = YoutubeMethodsStatic.ViewCountShortner(videoListResponse.Items[k].Statistics.ViewCount) + collection[k + i * 50].ViewsAndDate; }
                    catch { collection[k + i * 50].ViewsAndDate = "Unknown" + collection[k + i * 50].ViewsAndDate; }
                }
            }
            return null;
        }

        public Task FillInViewsAsync(List<YoutubeItemDataType> collection, YouTubeService service)
        {
            if (collection.Count <= 0) return null;

            for (int i = 0; i <= (collection.Count - 1) / 50; i++)
            {
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

                for (int k = 0; k < collection.Count; k++)
                {
                    try { collection[k + i * 50].ViewsAndDate = YoutubeMethodsStatic.ViewCountShortner(videoListResponse.Items[k].Statistics.ViewCount) + collection[k + i * 50].ViewsAndDate; }
                    catch { collection[k + i * 50].ViewsAndDate = "Unknown" + collection[k + i * 50].ViewsAndDate; }
                }
            }

            return null;
        }

        public void FillInViews(ObservableCollection<YoutubeItemDataType> collection, YouTubeService service)
        {
            if (collection.Count <= 0) return;

            for (int i = 0; i <= (collection.Count - 1) / 50; i++)
            {
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

                for (int k = 0; k < collection.Count; k++)
                {
                    try { collection[k + i * 50].ViewsAndDate = YoutubeMethodsStatic.ViewCountShortner(videoListResponse.Items[k].Statistics.ViewCount) + collection[k + i * 50].ViewsAndDate; }
                    catch { collection[k + i * 50].ViewsAndDate = "Unknown" + collection[k + i * 50].ViewsAndDate; }
                }
            }
        }

        public void FillInViews(List<YoutubeItemDataType> collection, YouTubeService service)
        {
            if (collection.Count <= 0) return;

            for (int i = 0; i <= (collection.Count - 1) / 50; i++)
            {
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

                for (int k = 0; k < collection.Count; k++)
                {
                    try { collection[k + i * 50].ViewsAndDate = YoutubeMethodsStatic.ViewCountShortner(videoListResponse.Items[k].Statistics.ViewCount) + collection[k + i * 50].ViewsAndDate; }
                    catch { collection[k + i * 50].ViewsAndDate = "Unknown" + collection[k + i * 50].ViewsAndDate; }
                }
            }
        }

        #endregion

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

        public double GetWatchedTime(string VideoID)
        {
            List<YoutubeItemDataType> list;
            if (Constants.syncedData.history.Count > 200)
                list = Constants.syncedData.history.GetRange(0, 200);
            else
                list = Constants.syncedData.history;

            var value = list.Find(x => x.Id == VideoID);
            if (value != null)
            {
                return value.WatchTime;
            }
            return 0;
        }
    }
}

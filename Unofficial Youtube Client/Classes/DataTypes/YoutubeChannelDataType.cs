using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YTApp.Classes
{
    public class YoutubeChannelDataType
    {
        private string _thumbnail;
        private string _title;
        private string _description;
        private string _subscribersAndVideos;
        private string _subscribers;
        private string _videos;
        private string _ylink;
        private string _id;

        public string Thumbnail { get => _thumbnail; set => _thumbnail = value; }
        public string Title { get => _title; set => _title = value; }
        public string Description { get => _description; set => _description = value; }
        public string SubscribersAndVideos { get => _subscribersAndVideos; set => _subscribersAndVideos = value; }
        public string Subscribers { get => _subscribers; set => _subscribers = value; }
        public string Videos { get => _videos; set => _videos = value; }
        public string Ylink { get => _ylink; set => _ylink = value; }
        public string Id { get => _id; set => _id = value; }
    }
}

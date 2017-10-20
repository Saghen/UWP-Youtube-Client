using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Media.Imaging;

namespace YTApp.Classes
{
    public class SubscriptionDataType
    {
        private string id;
        private string title;
        private BitmapImage thumbnail;
        private string newVideosCount;

        public string Id { get => id; set => id = value; }
        public string Title { get => title; set => title = value; }
        public BitmapImage Thumbnail { get => thumbnail; set => thumbnail = value; }
        public string NewVideosCount { get => newVideosCount; set { if (value == "0") { newVideosCount = ""; } else { newVideosCount = value; } } }
    }
}

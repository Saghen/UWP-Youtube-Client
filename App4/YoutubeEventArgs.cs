using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;

namespace YTApp
{
    public class YoutubeEventArgs : RoutedEventArgs
    {
        public string URL { get; set; }
        public string ID { get; set; }
        public YoutubeEventArgs(string Url, string Id)
        {
            URL = Url;
            ID = Id;
        }
    }
}

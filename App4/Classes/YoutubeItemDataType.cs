using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YTApp.Classes
{
    public class YoutubeItemDataType
    {
        private string _thumbnail;
        private string _title;
        private string _viewsAndDate;
        private string _author;
        private string _description;
        private string _length;
        private string _ylink;

        public string Thumbnail { get => _thumbnail; set => _thumbnail = value; }
        public string Title { get => _title; set => _title = value; }
        public string ViewsAndDate { get => _viewsAndDate; set => _viewsAndDate = value; }
        public string Author { get => _author; set => _author = value; }
        public string Description { get => _description; set => _description = value; }
        public string Length { get => _length; set => _length = value; }
        public string Ylink { get => _ylink; set => _ylink = value; }
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YTApp.Classes
{
    public class YoutubeItemDataType : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged = delegate { };

        private string _thumbnail;
        private string _title;
        private string _viewsAndDate;
        private string _author;
        private string _description;
        private string _length;
        private string _ylink;
        private string _id;
        private string _channeId;
        private DateTime _dateSubmitted;
        private bool failed = false;

        public string Thumbnail { get => _thumbnail; set => _thumbnail = value; }
        public string Title { get => _title; set => _title = value; }
        public string ViewsAndDate { get { return _viewsAndDate; } set { _viewsAndDate = value; NotifyPropertyChanged(); } }
        public string Author { get => _author; set => _author = value; }
        public string Description { get => _description; set => _description = value; }
        public string Length { get => _length; set => _length = value; }
        public string Ylink { get => _ylink; set => _ylink = value; }
        public DateTime DateSubmitted { get => _dateSubmitted; set => _dateSubmitted = value; }
        public string Id { get => _id; set => _id = value; }
        public bool Failed { get => failed; set => failed = value; }
        public string ChanneId { get => _channeId; set => _channeId = value; }

        private void NotifyPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}

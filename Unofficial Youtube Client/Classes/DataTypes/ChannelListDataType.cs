using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YTApp.Classes.DataTypes
{
    class ChannelListDataType : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged = delegate { };

        ObservableCollection<YoutubeChannelDataType> _items = new ObservableCollection<YoutubeChannelDataType>();
        double _x = 0;
        string _title = "";

        public ObservableCollection<YoutubeChannelDataType> Items
        {
            get { return _items; }
            set
            {
                _items = value;
                NotifyPropertyChanged();
            }
        }

        public string Title
        {
            get { return _title; }
            set
            {
                _title = value;
                NotifyPropertyChanged();
            }
        }

        public double X
        {
            get { return _x; }
            set
            {
                _x = value;
                NotifyPropertyChanged();
            }
        }

        private void NotifyPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}

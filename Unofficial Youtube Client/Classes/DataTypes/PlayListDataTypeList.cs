using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YTApp.Classes.DataTypes
{
    class PlayListDataTypeList : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged = delegate { };

        private List<PlaylistDataType> _list = new List<PlaylistDataType>();

        public List<PlaylistDataType> List
        {
            get { return _list; }
            set
            {
                _list = value;
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

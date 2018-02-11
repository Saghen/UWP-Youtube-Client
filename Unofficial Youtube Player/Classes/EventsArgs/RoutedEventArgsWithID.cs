using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;

namespace YTApp.Classes.EventsArgs
{
    public class RoutedEventArgsWithID : RoutedEventArgs
    {
        public string ID;
        public RoutedEventArgsWithID(string Id)
        {
            ID = Id;
        }
    }
}

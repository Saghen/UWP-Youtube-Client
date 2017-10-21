using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using YTApp.Classes;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace YTApp
{
    public sealed partial class ChannelPlaylistGridView : UserControl
    {
        public ChannelPlaylistGridView(List<YoutubeItemDataType> list, string header)
        {
            this.InitializeComponent();
            VideoItemGridView.Header = header;
            this.Height = Double.NaN;
            VideoItemGridView.ItemsSource = new ObservableCollection<YoutubeItemDataType>(list);
        }
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel.Core;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Core;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;
using YTApp;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace YTApp
{
    public sealed partial class YoutubeItem : UserControl
    {
        string YLink = "";
        string YId = "";

        public YoutubeItem(string ImageLink, string Title, string Channel, string Link)
        {
            this.InitializeComponent();
            ImageControl.Source = new BitmapImage(new Uri(ImageLink));
            TitleControl.Text = Title;
            AuthorControl.Text = Channel;
            YLink = "https://www.youtube.com/watch?v=" + Link;
            YId = Link;
        }

        public event EventHandler<YoutubeEventArgs> ButtonClick;

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (this.ButtonClick != null)
                this.ButtonClick(this, new YoutubeEventArgs(YLink, YId));
        }
    }
}

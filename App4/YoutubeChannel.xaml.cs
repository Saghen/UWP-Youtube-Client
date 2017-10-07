using System;
using System.Collections.Generic;
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
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;
using YTApp;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace YTApp
{
    public sealed partial class YoutubeChannel : UserControl
    {
        string channelID;

        public YoutubeChannel(string ID, string Title, string ImageURL)
        {
            this.InitializeComponent();
            var imageBrush = new ImageBrush();
            imageBrush.ImageSource = new BitmapImage(new Uri(ImageURL));
            ChannelImage.Fill = imageBrush;
            TitleBlock.Text = Title;
            channelID = ID;
        }

        public event EventHandler<YoutubeEventArgs> ButtonClick;

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (this.ButtonClick != null)
                this.ButtonClick(this, new YoutubeEventArgs("", channelID));
        }
    }
}

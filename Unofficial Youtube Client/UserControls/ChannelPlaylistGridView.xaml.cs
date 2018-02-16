using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
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
using YTApp.Classes;
using YTApp.Classes.DataTypes;
using YTApp.Classes.EventsArgs;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace YTApp
{
    public sealed partial class ChannelPlaylistGridView : UserControl
    {
        public event EventHandler<RoutedEventArgsWithID> ItemClicked;
        public TransformModel transform = new TransformModel();

        private static readonly DependencyProperty ItemsSourceProperty = DependencyProperty.Register("ItemsSource", typeof(List<YoutubeItemDataType>), typeof(ChannelPlaylistGridView), new PropertyMetadata(null, null));

        public List<YoutubeItemDataType> ItemsSource
        {
            get { return (List<YoutubeItemDataType>)GetValue(ItemsSourceProperty); }
            set { SetValue(ItemsSourceProperty, value); }
        }

        public ChannelPlaylistGridView()
        {
            this.InitializeComponent();
            VideoItemHeader.Header = "Test";

            if (ItemsSource == null)
                return;
            foreach (var item in ItemsSource)
            {
                var parent = new GridViewItem()
                {
                    Height = 250,
                    Width = 250
                };

                parent.Tapped += VideoItemGridView_ItemClick;
                parent.Tag = item.Id;

                StackPanel stkPanel = new StackPanel()
                {
                    Margin = new Thickness(10)
                };

                var img = new Image()
                {
                    Margin = new Thickness(10),
                    HorizontalAlignment = HorizontalAlignment.Center,
                    Source = new BitmapImage(new Uri(item.Thumbnail))
                };

                var title = new TextBlock()
                {
                    Text = item.Title,
                    FontSize = 15,
                    TextWrapping = TextWrapping.WrapWholeWords,
                    TextTrimming = TextTrimming.CharacterEllipsis,
                    HorizontalAlignment = HorizontalAlignment.Center
                };

                var author = new TextBlock()
                {
                    Text = item.Author,
                    FontSize = 12,
                    HorizontalAlignment = HorizontalAlignment.Center
                };

                var viewsAndDate = new TextBlock()
                {
                    Text = item.ViewsAndDate,
                    FontSize = 12,
                    HorizontalAlignment = HorizontalAlignment.Center
                };

                stkPanel.Children.Add(img);
                stkPanel.Children.Add(title);
                stkPanel.Children.Add(author);
                stkPanel.Children.Add(viewsAndDate);

                parent.Content = stkPanel;

                VideoItems.Children.Add(parent);
            }
        }

        private void VideoItemGridView_ItemClick(object sender, TappedRoutedEventArgs e)
        {
            var item = (GridViewItem)sender;
            if (ItemClicked != null)
                ItemClicked(this, new RoutedEventArgsWithID((string)item.Tag));
        }

        public class TransformModel : INotifyPropertyChanged
        {
            public event PropertyChangedEventHandler PropertyChanged = delegate { };

            private double _xTransform = 0;

            public double XTransform
            {
                get { return _xTransform; }
                set
                {
                    _xTransform = value;
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

        private void MoveRight_Click(object sender, RoutedEventArgs e)
        {
            if (-(VideoItems.Children.Count * 250 - this.ActualWidth - 500) < transform.XTransform)
                transform.XTransform += -500;
            else if (-(VideoItems.Children.Count * 250 - this.ActualWidth - 250) < transform.XTransform)
                transform.XTransform = -(VideoItems.Children.Count * 250 - this.ActualWidth - 230);
            else if (-(VideoItems.Children.Count * 250 - this.ActualWidth) < transform.XTransform)
                transform.XTransform += -50;
        }

        private void MoveLeft_Click(object sender, RoutedEventArgs e)
        {
            if (transform.XTransform <= -500)
                transform.XTransform += 500;
            else if (transform.XTransform < 0)
                transform.XTransform = 0;
        }
    }
}

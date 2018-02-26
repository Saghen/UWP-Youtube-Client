using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace YTApp.Classes
{
    public class SearchTemplateSelector : DataTemplateSelector
    {
        public DataTemplate VideoTemplate { get; set; }
        public DataTemplate ChannelTemplate { get; set; }

        protected override DataTemplate SelectTemplateCore(object item, DependencyObject container)
        {
            if (item != null)
            {
                if (item is DataTypes.YoutubeItemDataType)
                {
                    return VideoTemplate;
                }
                return ChannelTemplate;
            }

            return null;
        }
    }
}

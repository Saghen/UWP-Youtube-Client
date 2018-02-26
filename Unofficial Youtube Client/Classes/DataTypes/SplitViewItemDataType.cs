using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YTApp.Classes.DataTypes
{
    class SplitViewItemDataType
    {
        private string _icon;
        private string _text;
        private string _thumbnail;

        public string Icon { get => _icon; set => _icon = value; }
        public string Text { get => _text; set => _text = value; }
        public string Thumbnail { get => _thumbnail; set => _thumbnail = value; }
    }
}

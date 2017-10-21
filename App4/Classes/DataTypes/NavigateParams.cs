using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YTApp.Classes.DataTypes
{
    class NavigateParams
    {
        public MainPage mainPageRef { get; set; }
        public string ID { get; set; }
        public bool Refresh { get; set; }
    }
}

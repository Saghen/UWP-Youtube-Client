using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;

namespace YTApp.Controls
{
    class GridViewWithXProperty : Windows.UI.Xaml.Controls.GridView
    {
        private static readonly DependencyProperty _xProperty = DependencyProperty.Register("XValue", typeof(double), typeof(GridViewWithXProperty), new PropertyMetadata(null, null));

        public double XValue
        {
            get { return (double)GetValue(_xProperty); }
            set { SetValue(_xProperty, value); }
        }
    }
}

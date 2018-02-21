using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace YTApp
{
    class CustomMediaTransportControls : MediaTransportControls
    {
        public event EventHandler SwitchedToFullSize;

        public CustomMediaTransportControls()
        {
            this.DefaultStyleKey = typeof(CustomMediaTransportControls);
        }

        protected override void OnApplyTemplate()
        {
            // Find the custom button and create an event handler for its Click event.
            var compactButton = GetTemplateChild("CompactWindow") as Button;
            compactButton.Click += CompactButton_Click;
            base.OnApplyTemplate();
        }

        private void CompactButton_Click(object sender, RoutedEventArgs e)
        {
            SwitchedToFullSize.Invoke(this, new EventArgs());
        }
    }
}

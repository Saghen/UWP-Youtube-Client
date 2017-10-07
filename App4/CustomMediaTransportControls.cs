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

        bool CompactView;

        private async void CompactButton_Click(object sender, RoutedEventArgs e)
        {
            // Raise an event on the custom control when 'like' is clicked.
            if (CompactView == true)
            {
                await ApplicationView.GetForCurrentView().TryEnterViewModeAsync(ApplicationViewMode.Default);
                CompactView = false;
                IsCompact = true;
            }
            else
            {
                await ApplicationView.GetForCurrentView().TryEnterViewModeAsync(ApplicationViewMode.CompactOverlay);
                CompactView = true;
                IsCompact = false;
            }
        }
    }
}

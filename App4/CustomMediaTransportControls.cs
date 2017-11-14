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
        public event EventHandler SwitchedToCompact;
        public event EventHandler SwitchedToFullSize;

        public readonly DependencyProperty IsCompactProperty = DependencyProperty.Register("IsCompact", typeof(bool), typeof(CustomMediaTransportControls), new PropertyMetadata(false));

        public bool IsCompact
        {
            get { return (bool)GetValue(IsCompactProperty); }
            set { SetValue(IsCompactProperty, value); }
        }

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
            if (IsCompact == true)
            {
                await ApplicationView.GetForCurrentView().TryEnterViewModeAsync(ApplicationViewMode.Default);
                SwitchedToFullSize.Invoke(this, new EventArgs());
            }
            else
            {
                ViewModePreferences compactOptions = ViewModePreferences.CreateDefault(ApplicationViewMode.CompactOverlay);
                compactOptions.CustomSize = new Windows.Foundation.Size(500, 281);
                await ApplicationView.GetForCurrentView().TryEnterViewModeAsync(ApplicationViewMode.CompactOverlay, compactOptions);
                SwitchedToCompact.Invoke(this, new EventArgs());
            }
        }
    }
}

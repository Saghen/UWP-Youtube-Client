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
        public event EventHandler SeekCompletedFast;

        public CustomMediaTransportControls()
        {
            this.DefaultStyleKey = typeof(CustomMediaTransportControls);
        }

        protected override void OnApplyTemplate()
        {
            // Find the custom button and create an event handler for its Click event.
            var compactButton = GetTemplateChild("CompactWindow") as Button;
            compactButton.Click += CompactButton_Click;

            // Captures the position of the slider being changed
            var slider = GetTemplateChild("MediaTransportControls_Timeline_Grid") as Grid;
            slider.PointerReleased += Slider_PointerReleased;
            slider.PointerCaptureLost += Slider_PointerCaptureLost;
            slider.PointerCanceled += Slider_PointerCanceled;
            base.OnApplyTemplate();
        }

        private void Slider_PointerCanceled(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            SeekCompletedFast.Invoke(this, new EventArgs());
        }

        private void Slider_PointerCaptureLost(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            SeekCompletedFast.Invoke(this, new EventArgs());
        }

        private void Slider_PointerReleased(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            SeekCompletedFast.Invoke(this, new EventArgs());
        }

        private void CompactButton_Click(object sender, RoutedEventArgs e)
        {
            // Raise an event on the custom control when 'like' is clicked.
            if (IsCompact == true)
            {
                SwitchedToFullSize.Invoke(this, new EventArgs());
            }
            else
            {
                SwitchedToCompact.Invoke(this, new EventArgs());
            }
        }
    }
}

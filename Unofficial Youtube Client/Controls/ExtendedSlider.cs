using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;

namespace YTApp.Controls
{
    public class ExtendedSlider : Slider
    {
        public event EventHandler SliderOnDrag;
        public event EventHandler SliderOnComplete;

        protected override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            var thumb = base.GetTemplateChild("HorizontalThumb") as Thumb;
            if (thumb != null)
            {
                thumb.PointerPressed += Slider_PointerPressed;
                thumb.PointerReleased += Thumb_PointerReleased;
                thumb.PointerCanceled += Thumb_PointerReleased;
                thumb.PointerCaptureLost += Thumb_PointerReleased;
                thumb.DragStarted += Thumb_DragStarted;
                thumb.DragDelta += Thumb_DragDelta;
            }
        }

        private void Thumb_DragDelta(object sender, DragDeltaEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void Thumb_DragStarted(object sender, DragStartedEventArgs e)
        {
            if (SliderOnDrag != null)
            {
                SliderOnDrag.Invoke(this, new EventArgs());
            }
        }

        private void Slider_PointerPressed(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            if (SliderOnDrag != null)
            {
                SliderOnDrag.Invoke(this, new EventArgs());
            }
        }

        private void Thumb_PointerReleased(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            if (SliderOnComplete != null)
            {
                SliderOnComplete.Invoke(this, new EventArgs());
            }
        }
    }
}

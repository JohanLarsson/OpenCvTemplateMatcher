namespace OpenCvTemplateMatcher
{
    using System.Linq;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Input;
    using System.Windows.Media;

    public class ZoomBorder : Border
    {
        private UIElement child = null;
        private Point origin;
        private Point start;

        public override UIElement Child
        {
            get => base.Child;
            set
            {
                if (value != null && !ReferenceEquals(value, this.Child))
                {
                    this.Initialize(value);
                }

                base.Child = value;
            }
        }

        public void Reset()
        {
            if (this.child != null)
            {
                // reset zoom
                var st = this.GetScaleTransform(this.child);
                st.SetCurrentValue(ScaleTransform.ScaleXProperty, 1.0);
                st.SetCurrentValue(ScaleTransform.ScaleYProperty, 1.0);

                // reset pan
                var tt = this.GetTranslateTransform(this.child);
                tt.SetCurrentValue(TranslateTransform.XProperty, 0.0);
                tt.SetCurrentValue(TranslateTransform.YProperty, 0.0);
            }
        }

        private void Initialize(UIElement element)
        {
            this.child = element;
            if (this.child != null)
            {
                var group = new TransformGroup();
                var st = new ScaleTransform();
                group.Children.Add(st);
                var tt = new TranslateTransform();
                group.Children.Add(tt);
                this.child.SetCurrentValue(RenderTransformProperty, group);
                this.child.SetCurrentValue(RenderTransformOriginProperty, new Point(0.0, 0.0));
                this.MouseWheel += this.Child_MouseWheel;
                this.MouseLeftButtonDown += this.Child_MouseLeftButtonDown;
                this.MouseLeftButtonUp += this.Child_MouseLeftButtonUp;
                this.MouseMove += this.Child_MouseMove;
                this.PreviewMouseRightButtonDown += this.Child_PreviewMouseRightButtonDown;
            }
        }

        private TranslateTransform GetTranslateTransform(UIElement element)
        {
            return (TranslateTransform)((TransformGroup)element.RenderTransform)
                .Children.First(tr => tr is TranslateTransform);
        }

        private ScaleTransform GetScaleTransform(UIElement element)
        {
            return (ScaleTransform)((TransformGroup)element.RenderTransform)
                .Children.First(tr => tr is ScaleTransform);
        }

        private void Child_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (this.child != null)
            {
                var st = this.GetScaleTransform(this.child);
                var tt = this.GetTranslateTransform(this.child);

                if (e.Delta == 0)
                {
                    return;
                }

                var relative = e.GetPosition(this.child);

                var absX = (relative.X * st.ScaleX) + tt.X;
                var absY = (relative.Y * st.ScaleY) + tt.Y;

                var zoom = e.Delta > 0 ? 0.9 : 1.0 / 0.9;

                st.ScaleX *= zoom;
                st.ScaleY *= zoom;

                tt.SetCurrentValue(TranslateTransform.XProperty, absX - (relative.X * st.ScaleX));
                tt.SetCurrentValue(TranslateTransform.YProperty, absY - (relative.Y * st.ScaleY));
            }
        }

        private void Child_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (this.child != null)
            {
                var tt = this.GetTranslateTransform(this.child);
                this.start = e.GetPosition(this);
                this.origin = new Point(tt.X, tt.Y);
                this.SetCurrentValue(CursorProperty, Cursors.Hand);
                this.child.CaptureMouse();
            }
        }

        private void Child_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (this.child != null)
            {
                this.child.ReleaseMouseCapture();
                this.SetCurrentValue(CursorProperty, Cursors.Arrow);
            }
        }

        private void Child_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.Reset();
        }

        private void Child_MouseMove(object sender, MouseEventArgs e)
        {
            if (this.child != null)
            {
                if (this.child.IsMouseCaptured)
                {
                    var tt = this.GetTranslateTransform(this.child);
                    var v = this.start - e.GetPosition(this);
                    tt.SetCurrentValue(TranslateTransform.XProperty, this.origin.X - v.X);
                    tt.SetCurrentValue(TranslateTransform.YProperty, this.origin.Y - v.Y);
                }
            }
        }
    }
}
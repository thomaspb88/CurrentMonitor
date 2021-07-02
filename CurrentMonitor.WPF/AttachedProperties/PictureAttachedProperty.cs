using System.Windows;
using System.Windows.Controls;

namespace CurrentMonitor.WPF.AttachedProperties
{
    public class PictureAttachedProperty : Button
    {
        public static Canvas GetMyProperty(DependencyObject obj)
        {
            return (Canvas)obj.GetValue(MyPropertyProperty);
        }

        public static void SetMyProperty(DependencyObject obj, Canvas value)
        {
            obj.SetValue(MyPropertyProperty, value);
        }

        // Using a DependencyProperty as the backing store for MyProperty.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MyPropertyProperty =
            DependencyProperty.RegisterAttached("MyProperty", typeof(Canvas), typeof(PictureAttachedProperty), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));


    }
}

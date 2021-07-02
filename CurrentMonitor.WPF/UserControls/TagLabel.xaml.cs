using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace CurrentMonitor.WPF.UserControls
{
    /// <summary>
    /// Interaction logic for TagLabel.xaml
    /// </summary>
    public partial class TagLabel : UserControl
    {
        public TagLabel()
        {
            InitializeComponent();
            this.DataContext = this;
        }


        public string LabelText
        {
            get { return (string)GetValue(LabelTextProperty); }
            set { SetValue(LabelTextProperty, value); }
        }

        // Using a DependencyProperty as the backing store for LabelText.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty LabelTextProperty =
            DependencyProperty.Register("LabelText", typeof(string), typeof(TagLabel), new PropertyMetadata(""));

        public string Value
        {
            get { return (string)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Value.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register("Value", typeof(string), typeof(TagLabel), new UIPropertyMetadata(string.Empty));

        public int OuterBorderThickness
        {
            get { return (int)GetValue(OuterBorderThicknessProperty); }
            set { SetValue(OuterBorderThicknessProperty, value); }
        }

        // Using a DependencyProperty as the backing store for OuterBorderThickness.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty OuterBorderThicknessProperty =
            DependencyProperty.Register("OuterBorderThickness", typeof(int), typeof(TagLabel), new PropertyMetadata(1));



        public int CornerRadius
        {
            get { return (int)GetValue(CornerRadiusProperty); }
            set { SetValue(CornerRadiusProperty, value); }
        }

        // Using a DependencyProperty as the backing store for CornerRadius.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CornerRadiusProperty =
            DependencyProperty.Register("CornerRadius", typeof(int), typeof(TagLabel), new PropertyMetadata(0));

        public Brush BackgroundColourBrush
        {
            get { return (Brush)GetValue(BackgroundColourBrushProperty); }
            protected set { SetValue(BackgroundColourBrushPropertyKey, value); }
        }

        private static readonly DependencyPropertyKey BackgroundColourBrushPropertyKey
                = DependencyProperty.RegisterReadOnly("BackgroundColourBrush", typeof(Brush), typeof(TagLabel),
                        new FrameworkPropertyMetadata(Brushes.White, FrameworkPropertyMetadataOptions.None));

        public static readonly DependencyProperty BackgroundColourBrushProperty = BackgroundColourBrushPropertyKey.DependencyProperty;

        public Color BackgroundColour
        {
            get { return (Color)GetValue(BackgroundColourProperty); }
            set { SetValue(BackgroundColourProperty, value); }
        }

        // Using a DependencyProperty as the backing store for BackgroundColour.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty BackgroundColourProperty =
            DependencyProperty.Register("BackgroundColour", typeof(Color), typeof(TagLabel), new PropertyMetadata(Colors.White, OnBackgroundColorChanged));

        private static void OnBackgroundColorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            (d as TagLabel).BackgroundColourBrush = new SolidColorBrush((Color)e.NewValue);
        }


        public Brush OuterBorderColourBrush
        {
            get { return (Brush)GetValue(OuterBorderColourBrushProperty); }
            protected set { SetValue(OuterBorderColourBrushPropertyKey, value); }
        }

        private static readonly DependencyPropertyKey OuterBorderColourBrushPropertyKey
                = DependencyProperty.RegisterReadOnly("OuterBorderColourBrush", typeof(Brush), typeof(TagLabel),
                        new FrameworkPropertyMetadata(Brushes.White, FrameworkPropertyMetadataOptions.None));

        public static readonly DependencyProperty OuterBorderColourBrushProperty = OuterBorderColourBrushPropertyKey.DependencyProperty;

        public Color OuterBorderColour
        {
            get { return (Color)GetValue(OuterBorderColourProperty); }
            set { SetValue(OuterBorderColourProperty, value); }
        }

        // Using a DependencyProperty as the backing store for BackgroundColour.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty OuterBorderColourProperty =
            DependencyProperty.Register("OuterBorderColour", typeof(Color), typeof(TagLabel), new PropertyMetadata(Colors.White, OnOuterBorderColourChanged));

        private static void OnOuterBorderColourChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            (d as TagLabel).OuterBorderColourBrush = new SolidColorBrush((Color)e.NewValue);
        }
    }
}

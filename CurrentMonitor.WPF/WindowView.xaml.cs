using CurrentMonitor.WPF.ViewModel;
using System.Windows;

namespace CurrentMonitor.WPF
{
    /// <summary>
    /// Interaction logic for WindowView.xaml
    /// </summary>
    public partial class WindowView : Window
    {
        public WindowView(MainViewModel mainViewModel) : base()
        {
            InitializeComponent();
            mainViewModel.Load();
            this.DataContext = new WindowViewModel(this, mainViewModel);
        }
    }
}

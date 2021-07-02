using Prism.Commands;
using Prism.Mvvm;
using System.Windows;
using System.Windows.Input;

namespace CurrentMonitor.WPF.ViewModel
{
    public class WindowViewModel : BindableBase
    {
        internal IViewModel _currentViewModel;
        internal Window _window;
        private int _paddingSize = 10;
        public bool IsWindowMaximised => _window.WindowState == WindowState.Maximized;

        public WindowViewModel(Window window, MainViewModel mainViewModel)
        {
            _currentViewModel = mainViewModel;
            _window = window;
            _window.MaxHeight = SystemParameters.MaximizedPrimaryScreenHeight;
            _window.StateChanged += (s, e) =>
            {
                RaisePropertyChanged(nameof(ResizeBorderThickness));
                RaisePropertyChanged(nameof(PaddingSize));
                RaisePropertyChanged(nameof(OuterMarginThickness));

                RaisePropertyChanged(nameof(IsWindowMaximised));
            };

            MinimiseCommand = new DelegateCommand(() => _window.WindowState = WindowState.Minimized);
            MaximiseCommand = new DelegateCommand(() => _window.WindowState ^= WindowState.Maximized);
            CloseCommand = new DelegateCommand(() => _window.Close());
        }

        public ICommand CloseCommand { get; set; }

        public IViewModel CurrentViewModel
        {
            get { return _currentViewModel; }
            set
            {
                _currentViewModel = value;
                RaisePropertyChanged();
            }
        }

        public ICommand MaximiseCommand { get; set; }
        public ICommand MinimiseCommand { get; set; }

        public Thickness OuterMarginThickness { get { return new Thickness(PaddingSize); } }

        public int PaddingSize
        {
            get { return _window.WindowState == WindowState.Normal ? 0 : _paddingSize; }
            set { _paddingSize = value; }
        }

        public int ResizeBorder { get; set; } = 6;
        public Thickness ResizeBorderThickness { get { return new Thickness(ResizeBorder + PaddingSize); } }

        public int TitleHeight { get; set; } = 42;
        public GridLength TitleHeightGridLength { get { return new GridLength(TitleHeight + ResizeBorder); } }
    }
}
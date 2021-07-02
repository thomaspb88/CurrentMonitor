using Prism.Mvvm;

namespace CurrentMonitor.WPF.DesignTime
{
    public class MockWindowViewModel : BindableBase
    {
        private BindableBase _currentViewModel;

        public int ResizeBorder { get; set; } = 6;

        public int OuterMarginSizeThickness { get; set; } = 0;

        public MockWindowViewModel()
        {
            var newVM = new MockMainViewModel();
            CurrentViewModel = newVM;
        }

        public BindableBase CurrentViewModel
        {
            get { return _currentViewModel; }
            set
            {
                _currentViewModel = value;
                RaisePropertyChanged();
            }
        }

        public int TitleHeight { get; set; } = 42;
    }
}


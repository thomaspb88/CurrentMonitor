using Prism.Mvvm;
using System.Windows.Input;

namespace CurrentMonitor.WPF.DesignTime
{
    public class MockMainViewModel : BindableBase
    {

        public MockMainViewModel()
        {
            CurrentViewModel = new MockTestConsoleViewModel();
        }

        private BindableBase currentViewModel;

        public BindableBase CurrentViewModel
        {
            get { return currentViewModel; }
            set
            {
                currentViewModel = value;
                RaisePropertyChanged();
            }
        }

        public ICommand NavigateToCommand { get; private set; }
    }


}

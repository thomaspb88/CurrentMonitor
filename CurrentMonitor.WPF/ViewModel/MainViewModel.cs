using CurrentMonitor.WPF.Events;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using System.Windows.Input;

namespace CurrentMonitor.WPF.ViewModel
{
    public enum Status
    {
        Stopped,
        Paused,
        Running
    }

    public class MainViewModel : BindableBase, IViewModel
    {
        private readonly IEventAggregator _eventAggregator;
        private NavigationViewModelFactory _navigationViewModelFactory;
        private readonly TestConsoleViewModel _testConsoleViewModel;
        private readonly SettingsViewModel _settingsViewModel;

        public MainViewModel(IEventAggregator eventAggregator, NavigationViewModelFactory navigationViewModelFactory)
        {
            _eventAggregator = eventAggregator;
            _navigationViewModelFactory = navigationViewModelFactory;
            NavigateToCommand = new DelegateCommand<string>(OnNavigateTo, CanExecuteNavigateTo);

            _eventAggregator.GetEvent<NavigationRequestEvent>().Subscribe(OnViewModelContextChanged);
            _eventAggregator.GetEvent<SettingsChangedEvent>().Subscribe(OnSettingChanged);
        }

        public void Load()
        {
            CurrentViewModel = _navigationViewModelFactory.Create("TestConsoleViewModel");
        }

        private void OnSettingChanged(bool obj)
        {
            CurrentViewModel.Load();
        }

        private void OnViewModelContextChanged(string viewModelName)
        {
            OnNavigateTo(viewModelName);
        }

        private bool CanExecuteNavigateTo(string viewModelName)
        {
            return !currentViewModel.IsBusy;
        }

        private void OnNavigateTo(string viewModelName)
        {
            if(CurrentViewModel.GetType().Name != viewModelName)
            {
                MenuPaneDisplayed = false;
                CurrentViewModel = _navigationViewModelFactory.Create(viewModelName);
            }
        }

        private IViewModel currentViewModel;

        public IViewModel CurrentViewModel
        {
            get { return currentViewModel; }
            set
            {
                currentViewModel = value;
                RaisePropertyChanged();
            }
        }

        public ICommand NavigateToCommand { get; }

        private bool menuPaneDisplayed;

        public bool MenuPaneDisplayed
        {
            get { return menuPaneDisplayed; }
            set
            {
                menuPaneDisplayed = value;
                RaisePropertyChanged();
            }
        }

        public bool IsBusy => false;
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CurrentMonitor.WPF.ViewModel
{
    public class NavigationViewModelFactory
    {
        private SettingsViewModel _settingsViewModel;
        private TestConsoleViewModel _testConsoleViewModel;

        public NavigationViewModelFactory(SettingsViewModel settingsViewModel, TestConsoleViewModel testConsoleViewModel)
        {
            _settingsViewModel = settingsViewModel;
            _testConsoleViewModel = testConsoleViewModel;
        }

        public IViewModel Create(string name)
        {
            switch (name)
            {
                case "SettingsViewModel":
                    _settingsViewModel.Load();
                    return _settingsViewModel;
                case "TestConsoleViewModel":
                default:
                    _testConsoleViewModel.Load();
                    return _testConsoleViewModel;
                    break;
            }
        }

    }
}

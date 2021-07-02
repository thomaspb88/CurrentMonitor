using CurrentMonitor.Common.Events;
using CurrentMonitor.Devices;
using CurrentMonitor.WPF.Events;
using CurrentMonitor.WPF.Shared;
using Newtonsoft.Json;
using Prism.Commands;
using Prism.Events;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Windows.Input;

namespace CurrentMonitor.WPF.ViewModel
{
    public class SettingsViewModel : ValidationViewModelBase, IViewModel
    {
        private readonly IDAQDevice _device;
        private readonly IDeviceChannelViewModelFactory.Create _deviceChannelViewModelFactory;
        private readonly IEventAggregator _eventAggregator;
        private readonly ISettings _settings;
        public SettingsViewModel(IEventAggregator eventAggregator, ISettings settings, IDAQDevice device, IDeviceChannelViewModelFactory.Create deviceViewModelFactory)
        {
            _deviceChannelViewModelFactory = deviceViewModelFactory;
            _eventAggregator = eventAggregator;
            _eventAggregator.GetEvent<DeviceConnectionChangedEvent>().Subscribe((s) => this.Load(), ThreadOption.UIThread);
            _settings = settings;
            _device = device;
            SaveSettingsCommand = new DelegateCommand(OnSaveSettings, CanExecuteSaveSettings);
            this.PropertyChanged += SettingsViewModel_PropertyChanged;
            DeviceChannels = new ObservableCollection<IDeviceChannelViewModel>();
        }
        private string _selectedDeviceChannels;

        [Range(0, 9, ErrorMessage = "Must be between 0 and 9")]
        public int ChannelCount
        {
            get { return GetValue<int>(); }
            set { SetValue(value); }
        }

        [Range(0, 10, ErrorMessage = "Must be between 10 and 0")]
        public double ChartYAxisMax
        {
            get { return GetValue<double>(); }
            set { SetValue(value); }
        }

        [Range(-10, 0, ErrorMessage = "Must be between -10 and 0")]
        public double ChartYAxisMin
        {
            get { return GetValue<double>(); }
            set { SetValue(value); }
        }

        public ObservableCollection<IDeviceChannelViewModel> DeviceChannels { get; set; }

        public bool IsBusy => false;
        public bool IsVoltageDisplayed
        {
            get { return GetValue<bool>(); }
            set { SetValue(value); }
        }

        [Range(-10, 0, ErrorMessage = "Must be between -10 and 0")]
        public double LowerThreshold
        {
            get { return GetValue<double>(); }
            set { SetValue(value); }
        }

        [Range(0, 10000, ErrorMessage = "Must be between 0 and 10,000")]
        public int NumberOfSamplesToDisplay
        {
            get { return GetValue<int>(); }
            set { SetValue(value); }
        }

        [Range(0, 10000, ErrorMessage = "Must be between 0 and 10,000")]
        public int SampleHz
        {
            get { return GetValue<int>(); }
            set { SetValue(value); }
        }

        [Range(1, 10000, ErrorMessage = "Must be between 0 and 10,000")]
        public int SampleToRead
        {
            get { return GetValue<int>(); }
            set { SetValue(value); }
        }

        public ICommand SaveSettingsCommand { get; set; }

        [Range(0, 10, ErrorMessage = "Must be between 0 and 10")]
        public double UpperThreshold
        {
            get { return GetValue<double>(); }
            set { SetValue(value); }
        }

        public void Load()
        {
            DeviceChannels.Clear();

            UpperThreshold = (double)_settings[nameof(UpperThreshold)];
            LowerThreshold = (double)_settings[nameof(LowerThreshold)];
            ChartYAxisMax = (double)_settings[nameof(ChartYAxisMax)];
            ChartYAxisMin = (double)_settings[nameof(ChartYAxisMin)];
            IsVoltageDisplayed = (bool)_settings[nameof(IsVoltageDisplayed)];
            ChannelCount = (int)_settings[nameof(ChannelCount)];
            SampleToRead = (int)_settings[nameof(SampleToRead)];
            NumberOfSamplesToDisplay = (int)_settings[nameof(NumberOfSamplesToDisplay)];
            SampleHz = (int)_settings[nameof(SampleHz)];
            _selectedDeviceChannels = (string)_settings["DeviceChannelList"];
            IntialiseDeviceChannels(_selectedDeviceChannels);
        }

        private void IntialiseDeviceChannels(string tempString)
        {
            var selectedChannels = JsonConvert.DeserializeObject<string[]>(tempString);
            var channels = _device.GetAllAnalogueInChannels();

            var hasChannelsCurrentlySelected = selectedChannels != null && selectedChannels.Any(c => channels.Any(x => x == c));

            if (!hasChannelsCurrentlySelected) _selectedDeviceChannels = null;
 
            foreach(var channel in channels)
            {
                var deviceViewModel = _deviceChannelViewModelFactory();
                deviceViewModel.Load(channel, (selectedChannels?.Contains(channel) == true));
                DeviceChannels.Add(deviceViewModel);
            }
        }

        private bool CanExecuteSaveSettings()
        {
            return !HasErrors;
        }

        private void OnSaveSettings()
        {
            _settings[nameof(UpperThreshold)] = UpperThreshold;
            _settings[nameof(LowerThreshold)] = LowerThreshold;
            _settings[nameof(ChartYAxisMax)] = ChartYAxisMax;
            _settings[nameof(ChartYAxisMin)] = ChartYAxisMin;
            _settings[nameof(IsVoltageDisplayed)] = IsVoltageDisplayed;
            _settings[nameof(ChannelCount)] = ChannelCount;
            _settings[nameof(SampleToRead)] = SampleToRead;
            _settings[nameof(NumberOfSamplesToDisplay)] = NumberOfSamplesToDisplay;
            _settings[nameof(SampleHz)] = SampleHz;
            var array = DeviceChannels.Where(d => d.IsSelected).Select(d => d.Name).ToArray();

            _settings["DeviceChannelList"] = JsonConvert.SerializeObject(array);
            _settings.Save();

            _eventAggregator.GetEvent<SettingsChangedEvent>()
                .Publish(true);
            _eventAggregator.GetEvent<NavigationRequestEvent>().Publish("");
        }

        private void SettingsViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            ((DelegateCommand)SaveSettingsCommand).RaiseCanExecuteChanged();
        }
    }
}
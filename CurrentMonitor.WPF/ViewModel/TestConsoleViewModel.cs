using CurrentMonitor.Common.Events;
using CurrentMonitor.DataAccess;
using CurrentMonitor.WPF.Events;
using CurrentMonitor.WPF.Shared;
using Newtonsoft.Json;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

namespace CurrentMonitor.WPF.ViewModel
{
    public class TestConsoleViewModel : BindableBase, IViewModel
    {
        private readonly IDAQDataProvider _currentDataProvider;
        private readonly IEventAggregator _eventAggregator;
        private readonly MonitorViewModelFactory.Create _monitorViewModelFactory;
        private readonly ISettings _settings;
        private readonly Stopwatch stopwatch;
        private readonly DispatcherTimer timer;
        private IEnumerable<string> _deviceChannelList;
        private int _sampleHz;
        private int _sampleToRead;
        private TimeSpan _time;
        private CancellationTokenSource cancellationTokenSource;
        private TimeSpan dur;
        private int hours;
        private string incomingData;
        private bool isBusy;
        private bool isDeviceConnected;
        private int minutes;
        private ObservableCollection<MonitorViewModel> monitors;
        private int seconds;
        private ObservableCollection<string> testEventsList;
        private TimeSpan timeOffset;
        private string timeRemaining;
        private string volt;

        private float[] voltages;

        public TestConsoleViewModel(IDAQDataProvider currentDataProvider,
                                                    MonitorViewModelFactory.Create monitorViewModelFactory,
            IEventAggregator eventAggregator, ISettings settings)
        {
            _currentDataProvider = currentDataProvider;
            _settings = settings;
            _eventAggregator = eventAggregator;
            _eventAggregator.GetEvent<DeviceConnectionChangedEvent>().Subscribe(OnConnectionChanged, ThreadOption.UIThread);
            _eventAggregator.GetEvent<FaultDetectedEvent>().Subscribe(PrintMessage, ThreadOption.UIThread);
            _monitorViewModelFactory = monitorViewModelFactory;

            IsDeviceConnected = _currentDataProvider.IsConnected;

            timer = new DispatcherTimer
            {
                Interval = new TimeSpan(0, 0, 0, 0, 200)
            };
            timer.Tick += Timer_Tick;

            stopwatch = new Stopwatch();

            StartMonitoringCommand = new DelegateCommand(OnStartMonitoring, CanExecuteStartMonitoring);
            PauseMonitoringCommand = new DelegateCommand(OnPauseMonitoring, CanExecutePauseMonitoring);
            StopMonitoringCommand = new DelegateCommand(OnStopMonitoring, CanExecuteStopMonitoring);

            Monitors = new ObservableCollection<MonitorViewModel>();
            TestEventsList = new ObservableCollection<string>();
        }

        public TimeSpan Duration
        {
            get { return dur; }
            set
            {
                dur = value;
                RaisePropertyChanged();
            }
        }

        public int Hours
        {
            get { return hours; }
            set
            {
                hours = value;
                RaisePropertyChanged();
            }
        }

        public string IncomingData
        {
            get { return incomingData; }
            set
            {
                incomingData = value;
                RaisePropertyChanged();
            }
        }

        public bool IsBusy
        {
            get { return isBusy; }
            set
            {
                isBusy = value;
                RaisePropertyChanged();
            }
        }

        public bool IsDeviceConnected
        {
            get { return isDeviceConnected; }
            set
            {
                isDeviceConnected = value;
                RaisePropertyChanged();
            }
        }

        public int Minutes
        {
            get { return minutes; }
            set
            {
                minutes = value;
                RaisePropertyChanged();
            }
        }

        public ObservableCollection<MonitorViewModel> Monitors
        {
            get { return monitors; }
            set
            {
                monitors = value;
                RaisePropertyChanged();
            }
        }

        public ICommand PauseMonitoringCommand { get; }

        public ObservableCollection<string> PhysicalChannels { get; set; }

        public int Seconds
        {
            get { return seconds; }
            set
            {
                seconds = value;
                RaisePropertyChanged();
            }
        }

        public ICommand StartMonitoringCommand { get; }

        public ICommand StopMonitoringCommand { get; }

        public ObservableCollection<string> TestEventsList
        {
            get { return testEventsList; }
            set
            {
                testEventsList = value;
                RaisePropertyChanged();
            }
        }

        public TimeSpan Time
        {
            get { return _time; }
            set
            {
                _time = value;
                RaisePropertyChanged();
            }
        }

        public TimeSpan TimeOffset
        {
            get { return timeOffset; }
            set
            {
                timeOffset = value;
                RaisePropertyChanged();
            }
        }

        public string TimeRemaining
        {
            get { return timeRemaining; }
            set
            {
                timeRemaining = value;
                RaisePropertyChanged();
            }
        }

        public string Volt
        {
            get { return volt; }
            set
            {
                volt = value;
                RaisePropertyChanged();
            }
        }

        public float[] Voltages
        {
            get { return voltages; }
            set
            {
                voltages = value;
                RaisePropertyChanged();
            }
        }

        public void Load()
        {
            _sampleHz = (int)_settings["SampleHz"];
            _sampleToRead = (int)_settings["SampleToRead"];
            _deviceChannelList = JsonConvert.DeserializeObject<string[]>((string)_settings["DeviceChannelList"]);
            Monitors.Clear();

            cancellationTokenSource = new CancellationTokenSource();

            if (_deviceChannelList == null) return;
            for (var i = 0; i < _deviceChannelList.ToList().Count; i++)
            {
                var vm = _monitorViewModelFactory();
                vm.SetUp($"Phase {i + 1}", cancellationTokenSource.Token);
                Monitors.Add(vm);
            }
        }

        private bool CanExecutePauseMonitoring()
        {
            return _currentDataProvider.Status == DataAccessState.Running;
        }

        private bool CanExecuteStartMonitoring()
        {
            return _currentDataProvider.Status != DataAccessState.Running && _currentDataProvider.IsConnected;
        }

        private bool CanExecuteStopMonitoring()
        {
            return _currentDataProvider.Status != DataAccessState.Stopped;
        }

        private void Consume(CancellationToken cancellationToken)
        {
            //while (_currentDataProvider.Status == DataAccessState.Running)
            //{
                foreach (var data in _currentDataProvider._dataQueue.GetConsumingEnumerable())
                {
                    var arraySize = data.GetLength(1);
                    Parallel.For(0, data.GetLength(0), i =>
                    {
                        double[] dataArray = new double[arraySize];
                        Buffer.BlockCopy(data, i * 8 * arraySize, dataArray, 0, arraySize * 8);
                        Monitors[i].PushData(Filter(dataArray, 4), Time);
                    });
                }
            //}
        }

        private void OnConnectionChanged(DeviceConnectionChangedEventArgs obj)
        {
            ((DelegateCommand)StartMonitoringCommand).RaiseCanExecuteChanged();

            IsDeviceConnected = _currentDataProvider.IsConnected;
        }

        private void OnPauseMonitoring()
        {
            _currentDataProvider.PauseScan();

            _currentDataProvider.Status = DataAccessState.Paused;
            IsBusy = true;

            ((DelegateCommand)PauseMonitoringCommand).RaiseCanExecuteChanged();
            ((DelegateCommand)StopMonitoringCommand).RaiseCanExecuteChanged();
            ((DelegateCommand)StartMonitoringCommand).RaiseCanExecuteChanged();

            cancellationTokenSource.Cancel();
            stopwatch.Stop();
        }

        private void OnStartMonitoring()
        {
            if (_currentDataProvider.Status == DataAccessState.Running) return;
            if (!_currentDataProvider.IsConnected) MessageBox.Show("cDAQ-9174 not plugged in");                     
            if (_currentDataProvider.Status == DataAccessState.Stopped)
            {
                cancellationTokenSource = new CancellationTokenSource();
                _currentDataProvider.SetUp(_deviceChannelList, _sampleToRead, _sampleHz);
                _currentDataProvider.BeginScan(cancellationTokenSource.Token);
            }

            if (_currentDataProvider.Status == DataAccessState.Paused)
            {
                cancellationTokenSource = new CancellationTokenSource();
                _currentDataProvider.BeginScan(cancellationTokenSource.Token);
            }

            IsBusy = true;
            
            _currentDataProvider.Status = DataAccessState.Running;
            ((DelegateCommand)PauseMonitoringCommand).RaiseCanExecuteChanged();
            ((DelegateCommand)StopMonitoringCommand).RaiseCanExecuteChanged();
            ((DelegateCommand)StartMonitoringCommand).RaiseCanExecuteChanged();

            foreach (var graph in Monitors)
            {
                graph.OnResetCommand();
            }

            Task.Run(() => Consume(cancellationTokenSource.Token));

            timer.Start();
            stopwatch.Start();
        }

        private void OnStopMonitoring()
        {
            if (_currentDataProvider.Status == DataAccessState.Stopped) return;
            cancellationTokenSource.Cancel();
            _currentDataProvider.EndScan();
            _currentDataProvider.Status = DataAccessState.Stopped;
            
            ((DelegateCommand)PauseMonitoringCommand).RaiseCanExecuteChanged();
            ((DelegateCommand)StopMonitoringCommand).RaiseCanExecuteChanged();
            ((DelegateCommand)StartMonitoringCommand).RaiseCanExecuteChanged();
            
            foreach (var monitor in Monitors)
            {
                monitor.ClearChart();
            }

            stopwatch.Stop();
            stopwatch.Reset();

            if (_currentDataProvider.Status == DataAccessState.Paused) return;
            TestEventsList.Clear();

            IsBusy = false;
        }

        private void PrintMessage(FaultDetectedEventArgs args)
        {
            testEventsList.Add($"Fault detected on {args.Description} @ {args.EventTime}");
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            //Time = stopwatch.Elapsed - TimeOffset;
            var timeRemaining = (Duration - Time).TotalSeconds;
            var timeRemainingRoundedUp = (TimeSpan.FromSeconds(Math.Ceiling(timeRemaining)));
            //TimeRemaining = timeRemainingRoundedUp.ToString();
            Dispatcher.CurrentDispatcher.BeginInvoke(new Action(() =>
            {
                Time = stopwatch.Elapsed - TimeOffset;
                TimeRemaining = timeRemainingRoundedUp.ToString();
            }), DispatcherPriority.Background);
        }

        #region Helper Classes

        private double[] Filter(double[] source, int step)
        {
            var largestValue = source.Max();
            var index = source.ToList().IndexOf(largestValue);
            var stepShift = step - (index % step);
            var bufferSize = (source.Length) / step;

            var newArray = new double[bufferSize];
            int j = 0;
            for (var i = stepShift; i < source.Count(); i += step)
            {
                newArray[j] = source[i];
                j += 1;
            }
            return newArray;
        }

        #endregion
    }
}
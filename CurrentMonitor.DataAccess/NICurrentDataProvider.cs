using CurrentMonitor.Common.Events;
using CurrentMonitor.Devices;
using NationalInstruments.DAQmx;
using Prism.Events;
using System;
using System.Collections.Generic;
using System.Threading;
using niTask = NationalInstruments.DAQmx.Task;
using Task = System.Threading.Tasks.Task;

namespace CurrentMonitor.DataAccess
{
    public class NICurrentDataProvider : BaseDataProvider
    {
        private readonly IDAQDevice _device;
        private readonly IEventAggregator _eventAggregator;
        private niTask _myTask;
        private niTask _runningTask;
        private double[,] data;
        private AnalogMultiChannelReader myAnalogReader;

        public NICurrentDataProvider(IEventAggregator eventAggregator, IDAQDevice device)
        {
            _eventAggregator = eventAggregator;
            _eventAggregator.GetEvent<DeviceConnectionChangedEvent>().Subscribe(OnConnectionChanged);
            _device = device;
            _device.IntialiseDevice();
            IsConnected = _device.IsConnected;
        }

        public override Queue<double[,]> _dataQueue { get; set; }
        public override bool IsConnected { get; set; }

        public override void BeginScan(CancellationToken cancellationToken)
        {
            if (CurrentStatus == DataAccessState.Paused)
            {
                _runningTask.Control(TaskAction.Start);
                return;
            }

            _runningTask = _myTask;
            _cancellationToken = cancellationToken;

            Task.Run(() => myAnalogReader.BeginReadMultiSample(Convert.ToInt32(_sampleReads), new AsyncCallback(AnalogInCallback), _myTask));

            CurrentStatus = DataAccessState.Running;
        }

        public override void EndScan()
        {
            if (_runningTask != null)
            {
                _runningTask.Control(TaskAction.Stop);
                _runningTask = null;
                _myTask.Dispose();
                CurrentStatus = DataAccessState.Stopped;
            }
        }

        public override void PauseScan()
        {
            if (_runningTask != null)
            {
                _runningTask.Control(TaskAction.Stop);
                CurrentStatus = DataAccessState.Paused;
            }
        }

        public override void SetUp(int numberOfChannels, int sampleReads, int sampleRate)
        {
            _numberOfChannels = numberOfChannels;
            _sampleReads = sampleReads;
            _sampleRate = sampleRate;
            _dataQueue = new Queue<double[,]>();

            _myTask = new niTask();
            myAnalogReader = new AnalogMultiChannelReader(_myTask.Stream)
            {
                SynchronizeCallbacks = true
            };

            SetUpDaq();
        }

        public override void SetUp(IEnumerable<string> channelNames, int sampleReads, int sampleRate)
        {
            _sampleReads = sampleReads;
            _sampleRate = sampleRate;
            _dataQueue = new Queue<double[,]>();

            _myTask = new niTask();
            myAnalogReader = new AnalogMultiChannelReader(_myTask.Stream)
            {
                SynchronizeCallbacks = true
            };

            SetUpDaq();
        }

        private void AnalogInCallback(IAsyncResult ar)
        {
            try
            {
                if (_runningTask != null
                    && _runningTask == ar.AsyncState
                    && !_cancellationToken.IsCancellationRequested)
                {
                    data = myAnalogReader.EndReadMultiSample(ar);

                    _dataQueue.Enqueue(data);

                    myAnalogReader.BeginReadMultiSample(Convert.ToInt32(_sampleReads),
                        new AsyncCallback(AnalogInCallback), _myTask);
                }
            }
            catch (DaqException)
            {
                throw;
            }
        }

        private void CreateVerifyTask(string[] physicalChannelsAvailable)
        {
            for (var i = 0; i < _numberOfChannels; i++)
            {
                _myTask.AIChannels.CreateVoltageChannel(physicalChannelsAvailable[i], "",
                       (AITerminalConfiguration)(-1), Convert.ToDouble(-10),
                        Convert.ToDouble(10), AIVoltageUnits.Volts);
            }

            _myTask.Timing.ConfigureSampleClock("", Convert.ToDouble(_sampleRate),
                SampleClockActiveEdge.Rising, SampleQuantityMode.ContinuousSamples, _sampleReads * 10);

            _myTask.Control(TaskAction.Verify);
        }

        private void OnConnectionChanged(DeviceConnectionChangedEventArgs args)
        {
            IsConnected = args.IsConnected;
        }

        private void SetUpDaq()
        {
            try
            {
                var physicalChannelsAvailable = DaqSystem.Local.GetPhysicalChannels(PhysicalChannelTypes.AI, PhysicalChannelAccess.External);
                CreateVerifyTask(physicalChannelsAvailable);
            }
            catch (Exception)
            {
                _runningTask = null;
                _myTask.Dispose();
                _myTask = null;
                throw;
            }
        }
    }
}
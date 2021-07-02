using System;
using System.Collections.Generic;
using System.Linq;
using CurrentMonitor.Common.Events;
using niTask = NationalInstruments.DAQmx.Task;
using Task = System.Threading.Tasks.Task;
using NationalInstruments.DAQmx;
using Prism.Events;
using UsbDeviceUtility;
using System.Threading;
using System.Collections.Concurrent;

namespace CurrentMonitor.Devices
{
    public enum DaqState
    {
        Stopped,
        Running,
        Paused
    }

    public class NIDaqC9174 : IDAQDevice
    {
        private readonly IEventAggregator _eventAggregator;
        private readonly IDeviceMonitoringService _deviceMonitoringService;

        private readonly Dictionary<niTask, AnalogMultiChannelReader> _idleAnalogueTasks = new Dictionary<niTask, AnalogMultiChannelReader>();
        private readonly Dictionary<niTask, AnalogMultiChannelReader> _runningAnalogueTasks = new Dictionary<niTask, AnalogMultiChannelReader>();
        private CancellationToken _cancellationToken;

        private int _sampleReads;

        public BlockingCollection<double[,]> _dataQueue { get; private set; } = new BlockingCollection<double[,]>();
        private double[,] data;

        private DaqState daqState;
        public bool IsConnected { get; set; }

        private readonly Dictionary<string, PhysicalChannelTypes> _availableChannels = new Dictionary<string, PhysicalChannelTypes>();
        private readonly Dictionary<string, PhysicalChannelTypes> _allocatedChannels = new Dictionary<string, PhysicalChannelTypes>();

        public NIDaqC9174(IEventAggregator eventAggregator, IDeviceMonitoringService deviceMonitoringService)
        {
            _eventAggregator = eventAggregator;
            _deviceMonitoringService = deviceMonitoringService;
            _deviceMonitoringService.ConfigureProvider("Win32_USBControllerDevice");
            _deviceMonitoringService.Watch(USBProperty.Name, "cDAQ-9174");
            _deviceMonitoringService.DeviceConnectionChanged += _deviceMonitoringService_DeviceConnectionChanged;
            IsConnected = deviceMonitoringService.IsConnected();
            if (IsConnected) IntialiseChannelList();
        }

        private void IntialiseChannelList()
        {
            foreach (PhysicalChannelTypes channelType in Enum.GetValues(typeof(PhysicalChannelTypes)))
            {
                var channels = DaqSystem.Local.GetPhysicalChannels(channelType, PhysicalChannelAccess.External);
                foreach (var channel in channels)
                {
                    if (_availableChannels.ContainsKey(channel)) continue;
                    _availableChannels.Add(channel, channelType);
                };
            };
        }

        public void IntialiseDevice() { }

        private void _deviceMonitoringService_DeviceConnectionChanged(object sender, USBDeviceConnectionChangedEventArgs e)
        {
            IsConnected = USBState.Connected == e.USBState;
            _eventAggregator.GetEvent<DeviceConnectionChangedEvent>().Publish(new DeviceConnectionChangedEventArgs()
            {
                IsConnected = IsConnected,
                Name = e.USBDevice.Name,
                DeviceID = e.USBDevice.DeviceID
            });

            _availableChannels.Clear();
            IntialiseChannelList();
        }

        public IEnumerable<string> GetAllAnalogueInChannels()
        {
            return _availableChannels.Where(c => c.Value == PhysicalChannelTypes.AI).Select(c => c.Key).ToList();
        }

        public IEnumerable<string> GetAllAnalogueOutChannels()
        {
            return _availableChannels.Where(c => c.Value == PhysicalChannelTypes.AO).Select(c => c.Key).ToList();
        }

        private void VerifyVoltageAnalogueInTask(niTask task)
        {
            task.Control(TaskAction.Verify);
        }

        public void CreateVoltageAnalogueInTask(IEnumerable<string> physicalChannels, int sampleReads, int sampleRate)
        {
            if (physicalChannels == null) throw new ArgumentNullException("Physical Channels cannot be null");
            if (sampleRate < 1) throw new ArgumentException("Sample rate must be greater than 1");
            if (sampleReads < 1) throw new ArgumentException("Sample rate must be greater than 1");
            if (physicalChannels.Any(c => _allocatedChannels.ContainsKey(c))) throw new ArgumentException("Channel has already allocated. A physical channel cannot be allocated more than once.");
            _sampleReads = sampleReads;

            var task = new niTask();

            try
            {
                foreach (var channel in physicalChannels)
                {
                    if (_allocatedChannels.ContainsKey(channel)) return;
                    _allocatedChannels[channel] = PhysicalChannelTypes.AI;
                    task.AIChannels.CreateVoltageChannel(channel, "", (AITerminalConfiguration)(-1), Convert.ToDouble(-10), Convert.ToDouble(10), AIVoltageUnits.Volts);
                }

                task.Timing.ConfigureSampleClock("", Convert.ToDouble(sampleRate), SampleClockActiveEdge.Rising, SampleQuantityMode.ContinuousSamples, sampleReads * 10);
                VerifyVoltageAnalogueInTask(task);

                var analogMultiChannelReader = new AnalogMultiChannelReader(task.Stream)
                {
                    SynchronizeCallbacks = true
                };
                _idleAnalogueTasks[task] = analogMultiChannelReader;
            }
            catch (Exception)
            {
                task.Dispose();
                _idleAnalogueTasks.Remove(task);
                throw;
            }
        }

        public void BeginTasks(CancellationToken cancellationToken)
        {
            _dataQueue = new BlockingCollection<double[,]>();
            if (_idleAnalogueTasks.Count < 1) throw new Exception("Unable to begin scan as no tasks have been created");
            _cancellationToken = cancellationToken;
            if (daqState == DaqState.Paused)
            {
                foreach (var task in _idleAnalogueTasks.ToList())
                {
                    task.Key.Control(TaskAction.Start);
                }
            }
            if (daqState == DaqState.Running) return;
            
            foreach (var task in _idleAnalogueTasks.ToList())
            {
                _runningAnalogueTasks.Add(task.Key, task.Value);
                _idleAnalogueTasks.Remove(task.Key);
                Task.Run(() =>
                {
                    _runningAnalogueTasks[task.Key].BeginReadMultiSample(Convert.ToInt32(task.Key.Timing.SamplesPerChannel), new AsyncCallback(AnalogInCallback), task.Key);
                });
            }
            daqState = DaqState.Running;
        }

        private void AnalogInCallback(IAsyncResult ar)
        {
            foreach (var task in _runningAnalogueTasks)
            {
                try
                {
                    if (task.Key == ar.AsyncState
                        && !_cancellationToken.IsCancellationRequested)
                    {
                        data = task.Value.EndReadMultiSample(ar);
                        _dataQueue.Add(data);

                        task.Value.BeginReadMultiSample(Convert.ToInt32(_sampleReads),
                            new AsyncCallback(AnalogInCallback), task.Key);
                    }
                }
                catch (DaqException)
                {
                    throw;
                }
            }
        }

        public void EndTasks()
        {
            foreach (var task in _runningAnalogueTasks.ToList())
            {
                if (task.Key != null)
                {
                    task.Key.Control(TaskAction.Stop);
                    _runningAnalogueTasks.Remove(task.Key);
                    task.Key.Dispose();
                }
            }

            foreach (var task in _idleAnalogueTasks.ToList())
            {
                if (task.Key != null)
                {
                    task.Key.Control(TaskAction.Stop);
                    _idleAnalogueTasks.Remove(task.Key);
                    task.Key.Dispose();
                }
            }
            _allocatedChannels.Clear();
            _dataQueue.CompleteAdding();
            daqState = DaqState.Stopped;

        }

        public void PauseTasks()
        {
            foreach (var task in _runningAnalogueTasks.ToList())
            {
                if (task.Key != null)
                {
                    task.Key.Control(TaskAction.Stop);
                    _idleAnalogueTasks.Add(task.Key, task.Value);
                    _runningAnalogueTasks.Remove(task.Key);
                }
            }
            daqState = DaqState.Paused;
        }
    }
}

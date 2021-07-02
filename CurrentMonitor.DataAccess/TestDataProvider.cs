using CurrentMonitor.Common.Events;
using CurrentMonitor.Devices;
using Prism.Events;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace CurrentMonitor.DataAccess
{
    public class TestDataProvider : ITestDataProvider
    {
        private IEventAggregator _eventAggregator;
        private IDAQDevice _device;

        public bool IsConnected { get; set; }
        public BlockingCollection<double[,]> _dataQueue { get; set; } = new BlockingCollection<double[,]>();
        public DataAccessState Status { get ; set ; }

        private int _sampleReads;
        private int _sampleRate;
        private IEnumerable<string> _channelNames;

        public TestDataProvider(IEventAggregator eventAggregator, IDAQDevice device)
        {
            _eventAggregator = eventAggregator;
            _eventAggregator.GetEvent<DeviceConnectionChangedEvent>().Subscribe(OnConnectionChanged);
            _device = device;
            _device.IntialiseDevice();
            IsConnected = _device.IsConnected;
        }

        private void OnConnectionChanged(DeviceConnectionChangedEventArgs args)
        {
            IsConnected = args.IsConnected;
        }

        public void SetUp(IEnumerable<string> channelNames, int sampleReads, int sampleRate)
        {
            _sampleReads = sampleReads;
            _sampleRate = sampleRate;
            _channelNames = channelNames;
            _device.CreateVoltageAnalogueInTask(_channelNames, _sampleReads, _sampleRate);
        }

        public void BeginScan(CancellationToken cancellationToken)
        {
            _dataQueue = new BlockingCollection<double[,]>();
            Status = DataAccessState.Running;
            _device.BeginTasks(cancellationToken);
            Task.Run(() => Consumer(cancellationToken),cancellationToken);
        }

        public void EndScan()
        {
            Status = DataAccessState.Stopped;
            _dataQueue.CompleteAdding();
            _device.EndTasks();
        }

        public void PauseScan()
        {
            Status = DataAccessState.Paused;
            _device.PauseTasks();
        }

        private void Consumer(CancellationToken cancellationToken)
        {
            foreach (var data in _device._dataQueue.GetConsumingEnumerable())
            {
                _dataQueue.Add(data);
            }
        }
    }
}

using CurrentMonitor.Common.Events;
using CurrentMonitor.Devices;
using Prism.Events;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CurrentMonitor.DataAccess
{
    public class DAQDataProvider : BaseDataStreamProvider, IDAQDataProvider
    {
        private int _sampleReads;
        private int _sampleRate;
        private IEnumerable<string> _channelNames;
        private IDAQDevice _device;
        public BlockingCollection<double[,]> _dataQueue { get; set; } = new BlockingCollection<double[,]>();

        public DAQDataProvider(IEventAggregator eventAggregator, IDAQDevice device) : base(eventAggregator)
        {
            _device = device;
            _eventAggregator.GetEvent<DeviceConnectionChangedEvent>().Subscribe(OnConnectionChanged);
            IsConnected = _device.IsConnected;
        }

        public void OnConnectionChanged(DeviceConnectionChangedEventArgs args)
        {
            IsConnected = args.IsConnected;
        }

        public virtual void SetUp(IEnumerable<string> channelNames, int sampleReads, int sampleRate)
        {
            _sampleReads = sampleReads;
            _sampleRate = sampleRate;
            _channelNames = channelNames;
            _device.CreateVoltageAnalogueInTask(_channelNames, _sampleReads, _sampleRate);
        }

        public override void BeginScan(CancellationToken cancellationToken)
        {
            _dataQueue = new BlockingCollection<double[,]>();
            Status = DataAccessState.Running;
            _device.BeginTasks(cancellationToken);
            Task.Run(() => Consumer(cancellationToken), cancellationToken);
        }

        public override void EndScan()
        {
            Status = DataAccessState.Stopped;
            _device.EndTasks();
            _dataQueue.CompleteAdding();
        }

        public override void PauseScan()
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

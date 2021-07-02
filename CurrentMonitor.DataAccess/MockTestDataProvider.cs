using CurrentMonitor.Common.Events;
using CurrentMonitor.Devices;
using Prism.Events;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CurrentMonitor.DataAccess
{
    public class MockTestDataProvider : BaseDataStreamProvider, IDAQDataProvider
    {
        private CancellationToken _cancellationToken;
        private IEnumerable<string> _channelNames;
        private int _sampleReads;
        private int _sampleRate;
        public BlockingCollection<double[,]> _dataQueue { get; set; } = new BlockingCollection<double[,]>();

        public MockTestDataProvider(IEventAggregator eventAggregator) : base(eventAggregator)
        {
            IsConnected = true;
        }

        public override void BeginScan(CancellationToken cancellationToken)
        {
            _cancellationToken = cancellationToken;
            if (Status == DataAccessState.Running) return;
            if (Status != DataAccessState.Running)
            {
                _cancellationToken = cancellationToken;
                Task.Run(() => Produce(), cancellationToken);
                Status = DataAccessState.Running;
            }           
        }

        private void Produce()
        {
            while (!_cancellationToken.IsCancellationRequested)
            {
                double[,] data = new double[_channelNames.Count(), _sampleReads];
                Parallel.For(0, _channelNames.Count(), (i, state) =>
                {
                    for (int j = 0; j < _sampleReads; j++)
                    {
                        if (_cancellationToken.IsCancellationRequested) state.Break();

                        var rand = new Random();
                        double randValue = rand.NextDouble() * 0.5;
                        data[i, j] = randValue;
                    }

                });
                _dataQueue.Add(data);
            }
        }

        public override void EndScan()
        {
            Status = DataAccessState.Stopped;
        }

        public override void PauseScan()
        {
            Status = DataAccessState.Paused;
        }

        public void SetUp(IEnumerable<string> channelNames, int sampleReads, int sampleRate)
        {
            _channelNames = channelNames;
            _sampleReads = sampleReads;
            _sampleRate = sampleRate;
        }

        public void OnConnectionChanged(DeviceConnectionChangedEventArgs args)
        {
            throw new NotImplementedException();
        }
    }
}

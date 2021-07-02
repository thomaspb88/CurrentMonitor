using CurrentMonitor.Common.Events;
using CurrentMonitor.Devices;
using Prism.Events;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace CurrentMonitor.DataAccess
{
    public abstract class BaseDataStreamProvider
    {
        public IEventAggregator _eventAggregator;

        public DataAccessState Status { get; set; }


        public bool IsConnected { get; set; }

        protected BaseDataStreamProvider(IEventAggregator eventAggregator)
        {
            _eventAggregator = eventAggregator;
        }

        public abstract void EndScan();
        public abstract void BeginScan(CancellationToken cancellationToken);
        public abstract void PauseScan();
    }
}
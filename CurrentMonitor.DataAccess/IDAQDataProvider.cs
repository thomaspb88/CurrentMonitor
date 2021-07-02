using CurrentMonitor.Common.Events;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;

namespace CurrentMonitor.DataAccess
{
    public interface IDAQDataProvider
    {
        BlockingCollection<double[,]> _dataQueue { get; set; }
        DataAccessState Status { get; set; }
        bool IsConnected { get; set; }
        void OnConnectionChanged(DeviceConnectionChangedEventArgs args);
        void BeginScan(CancellationToken cancellationToken);
        void EndScan();
        void PauseScan();
        void SetUp(IEnumerable<string> channelNames, int sampleReads, int sampleRate);
    }
}
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;

namespace CurrentMonitor.DataAccess
{
    public interface ITestDataProvider
    {
        BlockingCollection<double[,]> _dataQueue { get; set; }

        void BeginScan(CancellationToken cancellationToken);

        void EndScan();

        void PauseScan();

        void SetUp(IEnumerable<string> channelNames, int sampleReads, int sampleRate);

        bool IsConnected { get; set; }

        DataAccessState Status { get; set; }
    }
}
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;

namespace CurrentMonitor.Devices
{
    public interface IDAQDevice
    {
        BlockingCollection<double[,]> _dataQueue { get; }
        bool IsConnected { get; set; }

        void BeginTasks(CancellationToken cancellationToken);
        void CreateVoltageAnalogueInTask(IEnumerable<string> physicalChannels, int sampleReads, int sampleRate);
        void EndTasks();
        IEnumerable<string> GetAllAnalogueInChannels();
        IEnumerable<string> GetAllAnalogueOutChannels();
        void IntialiseDevice();
        void PauseTasks();
    }
}
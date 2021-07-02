using System;

namespace CurrentMonitor.WPF.Events
{
    public class FaultDetectedEventArgs : EventArgs
    {
        public string Description { get; set; }
        public TimeSpan EventTime { get; set; }
    }
}

using System;

namespace CurrentMonitor.Common.Events
{
    public class DeviceConnectionChangedEventArgs : EventArgs
    {
        public string Name { get; set; }
        public string DeviceID { get; set; }
        public bool IsConnected { get; set; }
    }
}
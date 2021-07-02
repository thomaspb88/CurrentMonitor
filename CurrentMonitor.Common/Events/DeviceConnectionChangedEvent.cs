using Prism.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CurrentMonitor.Common.Events
{
    public class DeviceConnectionChangedEvent : PubSubEvent<DeviceConnectionChangedEventArgs>
    {
    }
}

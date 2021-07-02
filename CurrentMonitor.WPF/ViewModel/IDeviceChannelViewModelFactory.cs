using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CurrentMonitor.WPF.ViewModel
{
    public class IDeviceChannelViewModelFactory
    {
        public delegate DeviceChannelViewModel Create();
    }
}

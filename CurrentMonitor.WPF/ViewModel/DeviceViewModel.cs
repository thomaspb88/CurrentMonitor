using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CurrentMonitor.WPF.ViewModel
{
    public class DeviceViewModel : BindableBase
    {
        public  ObservableCollection<DeviceChannelViewModel> Channels { get; set; }

        public string Name { get; set; }

        public DeviceViewModel()
        {
            Channels = new ObservableCollection<DeviceChannelViewModel>();
        }
    }
}

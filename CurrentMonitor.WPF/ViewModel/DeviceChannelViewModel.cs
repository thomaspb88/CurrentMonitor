using Prism.Mvvm;

namespace CurrentMonitor.WPF.ViewModel
{
    public class DeviceChannelViewModel : BindableBase, IDeviceChannelViewModel
    {
        public void Load(string name, bool isSelected)
        {
            this.Name = name;
            this.IsSelected = isSelected;
        }

        private string name;

        public string Name
        {
            get { return name; }
            set
            {
                name = value;
                RaisePropertyChanged();
            }
        }

        private bool isSelected;

        public bool IsSelected
        {
            get { return isSelected; }
            set
            {
                isSelected = value;
                RaisePropertyChanged();
            }
        }



    }
}
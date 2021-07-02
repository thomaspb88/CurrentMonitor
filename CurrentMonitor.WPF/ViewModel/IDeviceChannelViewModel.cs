namespace CurrentMonitor.WPF.ViewModel
{
    public interface IDeviceChannelViewModel
    {
        bool IsSelected { get; set; }
        string Name { get; set; }
        void Load(string name, bool isSelected);
    }
}
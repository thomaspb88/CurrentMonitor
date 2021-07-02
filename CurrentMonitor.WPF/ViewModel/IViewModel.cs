namespace CurrentMonitor.WPF.ViewModel
{
    public interface IViewModel
    {
        void Load();

        bool IsBusy { get; }
    }
}

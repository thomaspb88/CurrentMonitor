namespace CurrentMonitor.WPF.Shared
{
    public interface ISettings
    {
        object this[string propertyName] { get; set; }

        void Save();
    }
}

using System;
using System.Threading;
using System.Windows.Input;

namespace CurrentMonitor.WPF.ViewModel
{
    public interface IMonitorViewModel
    {
        bool HasFlagged { get; set; }
        ICommand ResetFailCommand { get; set; }
        TimeSpan TimeElapsed { get; set; }
        void ClearChart();
        void OnResetCommand();
        void PushData(double[] data, TimeSpan timeElapsed);
        void SetUp(string description, CancellationToken cancellationToken);
    }
}
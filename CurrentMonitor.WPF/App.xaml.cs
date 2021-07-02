using Autofac;
using CurrentMonitor.WPF.StartUp;
using NationalInstruments.DAQmx;
using System;
using System.Windows;

namespace CurrentMonitor.WPF
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private IContainer container;

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            var bootstrapper = new Bootstrapper();
            container = bootstrapper.Bootstrap();

            WindowView windowView = container.Resolve<WindowView>();
            windowView.Show();

            
        }

        private void Application_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            MessageBox.Show("Unexpected error occured. Please inform the admin."
                + Environment.NewLine + e.Exception.Message, "Unexpected error");
            e.Handled = true;
        }
    }
}

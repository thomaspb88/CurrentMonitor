using Autofac;
using Autofac.Builder;
using Autofac.Core;
using CurrentMonitor.DataAccess;
using CurrentMonitor.Devices;
using CurrentMonitor.WPF.Shared;
using CurrentMonitor.WPF.ViewModel;
using Prism.Events;
using UsbDeviceUtility;

namespace CurrentMonitor.WPF.StartUp
{
    public class Bootstrapper
    {
        public IContainer Bootstrap()
        {
            var builder = new ContainerBuilder();
            builder.RegisterType<MainWindow>().AsSelf();
            builder.RegisterType<MainViewModel>().AsSelf();
            builder.RegisterType<WindowView>().AsSelf();
            builder.RegisterType<TestConsoleViewModel>().AsSelf();
            builder.RegisterType<SettingsViewModel>().AsSelf();
            builder.RegisterType<NavigationViewModelFactory>().AsSelf();
            builder.RegisterType<AppSettings>().As<ISettings>();
            builder.RegisterType<TestDataProvider>().As<ITestDataProvider>();
            builder.RegisterType<MonitorViewModel>().As<IMonitorViewModel>();
            builder.RegisterType<USBDeviceUtility>().As<IDeviceMonitoringService>();
            builder.RegisterType<DAQDataProvider>().As<IDAQDataProvider>();
            builder.RegisterType<NIDaqC9174>().As<IDAQDevice>().SingleInstance();
            builder.RegisterType<EventAggregator>().As<IEventAggregator>().SingleInstance();
            builder.RegisterType<DeviceChannelViewModel>().As<IDeviceChannelViewModel>();
            builder.RegisterGeneratedFactory<MonitorViewModelFactory.Create>(new TypedService(typeof(IMonitorViewModel)));
            builder.RegisterGeneratedFactory<IDeviceChannelViewModelFactory.Create>(new TypedService(typeof(IDeviceChannelViewModel)));
            return builder.Build();
        }
    }
}

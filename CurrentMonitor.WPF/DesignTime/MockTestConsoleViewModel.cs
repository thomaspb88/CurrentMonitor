using Prism.Mvvm;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows.Threading;

namespace CurrentMonitor.WPF.DesignTime
{
    public class MockTestConsoleViewModel : BindableBase
    {
        private readonly DispatcherTimer timer;
        private readonly Stopwatch stopwatch;
        private readonly Random rand = new Random();

        private ObservableCollection<MockMonitorViewModel> monitors;

        public MockTestConsoleViewModel()
        {
            timer = new DispatcherTimer
            {
                Interval = new TimeSpan(0, 0, 0, 0, 200)
            };
            timer.Tick += Timer_Tick;

            stopwatch = new Stopwatch();

            Monitors = new ObservableCollection<MockMonitorViewModel>
            {
                new MockMonitorViewModel(),
                new MockMonitorViewModel(),
                new MockMonitorViewModel()
            };

            MyList = new ObservableCollection<string>() { "Test String" };

            Hours = 2;
            Duration = new TimeSpan(2, 0, 0);

            timer.Start();
            stopwatch.Start();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            Time = stopwatch.Elapsed - TimeOffset;
            var timeRemaining = (Duration - Time).TotalSeconds;
            var timeRemainingRoundedUp = (TimeSpan.FromSeconds(Math.Ceiling(timeRemaining)));

            TimeRemaining = timeRemainingRoundedUp.ToString();

            Voltages = new ObservableCollection<float>() { rand.Next(299, 301), rand.Next(299, 301), rand.Next(299, 301), rand.Next(498, 502), rand.Next(498, 502), rand.Next(498, 502) };
        }

        public ObservableCollection<MockMonitorViewModel> Monitors
        {
            get { return monitors; }
            set
            {
                monitors = value;
                RaisePropertyChanged();
            }
        }

        private TimeSpan _time;

        public TimeSpan Time
        {
            get { return _time; }
            set
            {
                _time = value;
                RaisePropertyChanged();
            }
        }

        private string incomingData;

        public string IncomingData
        {
            get { return incomingData; }
            set
            {
                incomingData = value;
                RaisePropertyChanged();
            }
        }

        private ObservableCollection<float> voltages;

        public ObservableCollection<float> Voltages
        {
            get { return voltages; }
            set
            {
                voltages = value;
                RaisePropertyChanged();
            }
        }

        private string volt;

        public string Volt
        {
            get { return volt; }
            set
            {
                volt = value;
                RaisePropertyChanged();
            }
        }

        private TimeSpan dur;

        public TimeSpan Duration
        {
            get { return dur; }
            set
            {
                dur = value;
                RaisePropertyChanged();
            }
        }

        private int hours;

        public int Hours
        {
            get { return hours; }
            set
            {
                hours = value;
                RaisePropertyChanged();
            }
        }

        private int minutes;

        public int Minutes
        {
            get { return minutes; }
            set
            {
                minutes = value;
                RaisePropertyChanged();
            }
        }

        private int seconds;

        public int Seconds
        {
            get { return seconds; }
            set
            {
                seconds = value;
                RaisePropertyChanged();
            }
        }

        private TimeSpan timeOffset;

        public TimeSpan TimeOffset
        {
            get { return timeOffset; }
            set
            {
                timeOffset = value;
                RaisePropertyChanged();
            }
        }

        private string timeRemaining;

        public string TimeRemaining
        {
            get { return timeRemaining; }
            set
            {
                timeRemaining = value;
                RaisePropertyChanged();
            }
        }

        private ObservableCollection<string> myList;

        public ObservableCollection<string> MyList
        {
            get { return myList; }
            set
            {
                myList = value;
                RaisePropertyChanged();
            }
        }


    }
}

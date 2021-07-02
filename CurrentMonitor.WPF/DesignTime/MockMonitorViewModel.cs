using OxyPlot;
using OxyPlot.Annotations;
using OxyPlot.Axes;
using OxyPlot.Series;
using Prism.Mvvm;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Threading;

namespace CurrentMonitor.WPF.DesignTime
{
    public class MockMonitorViewModel : BindableBase
    {
        private readonly LineSeries dataLine;
        private int num = 5;
        readonly Random rand;

        public MockMonitorViewModel()
        {
            TimeElapsed = new TimeSpan(0, 5, 0);
            hasFlagged = false;

            Points = new ObservableCollection<DataPoint>();

            rand = new Random();

            MyModel = new PlotModel() { Background = OxyColor.FromRgb(0, 0, 0) };

            MyModel.Axes.Add(new LinearAxis
            {
                Position = AxisPosition.Left,
                AbsoluteMinimum = -6,
                AbsoluteMaximum = 6,
                Minimum = -6,
                Maximum = 6,
                AxislineColor = OxyColors.DarkGray,
                AxislineThickness = 1,
                AxislineStyle = LineStyle.Solid,
                TicklineColor = OxyColors.DarkGray,
                MinorStep = 1,
                MajorGridlineStyle = LineStyle.Solid,
                MinorGridlineStyle = LineStyle.Solid,
                MajorGridlineThickness = 2,
                MinorGridlineThickness = 1,
                MajorGridlineColor = OxyColor.FromArgb(210, 17, 69, 33),
                MinorGridlineColor = OxyColor.FromArgb(160, 17, 69, 33)
            });

            MyModel.Axes.Add(new LinearAxis
            {
                Position = AxisPosition.Bottom,
                Maximum = 1000,
                IsAxisVisible = false,
                AxislineColor = OxyColors.Gray,
                AxislineThickness = 1,
                AxislineStyle = LineStyle.Solid
            });

            dataLine = new LineSeries()
            {
                Title = "Series 1",
                Color = OxyColors.SkyBlue,
            };

            LineAnnotation Line = new LineAnnotation()
            {
                StrokeThickness = 1,
                Color = OxyColors.Red,
                Type = LineAnnotationType.Horizontal,
                Text = "Maximum",
                TextColor = OxyColors.White,
                X = 2.0D
            };

            MyModel.Annotations.Add(Line);
            MyModel.Series.Add(dataLine);

            DispatcherTimer dispatcherTimer = new DispatcherTimer
            {
                Interval = new TimeSpan(0, 0, 0, 0, 20)
            };
            dispatcherTimer.Tick += DispatcherTimer_Tick;
            dispatcherTimer.Start();
        }

        private void DispatcherTimer_Tick(object sender, EventArgs e)
        {

            for (var i = 0; i < 10; i++)
            {
                var data = rand.NextDouble() * 0.2 + 2;
                dataLine.Points.Add(new DataPoint(num++, data));
            }

            var axis = MyModel.Axes.First(x => x.Position == AxisPosition.Bottom);

            if (num > 1000)
            {
                double panStep = axis.Transform(-10 + axis.Offset);
                axis.Pan(panStep);
            }

            if (dataLine.Points.Count > 8000)
            {
                for (var i = 0; i < 1000; i++)
                {
                    dataLine.Points.RemoveAt(0);
                }
            }

            MyModel.InvalidatePlot(false);
        }


        private TimeSpan timeElapsed;

        public TimeSpan TimeElapsed
        {
            get { return timeElapsed; }
            set
            {
                timeElapsed = value;
                RaisePropertyChanged();
            }
        }

        private bool hasFlagged;

        public bool HasFlagged
        {
            get { return hasFlagged; }
            set
            {
                hasFlagged = value;
                RaisePropertyChanged();
            }
        }

        public ObservableCollection<DataPoint> Points { get; set; }

        public PlotModel MyModel { get; private set; }


    }
}

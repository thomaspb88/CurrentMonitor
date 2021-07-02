using CurrentMonitor.WPF.Events;
using CurrentMonitor.WPF.Shared;
using OxyPlot;
using OxyPlot.Annotations;
using OxyPlot.Axes;
using OxyPlot.Series;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

namespace CurrentMonitor.WPF.ViewModel
{
    public class MonitorViewModel : BindableBase, IMonitorViewModel
    {
        private readonly IEventAggregator _eventAggregator;
        private readonly double _lowerTriggerThreshold;
        private readonly int _samplesDisplayed;
        private readonly ISettings _settings;
        private readonly double _upperTriggerThreshold;
        private readonly double _chartYAxisMax;
        private readonly double _chartYAxisMin;
        private string _chartTitle;
        private BlockingCollection<double[]> _dataQueue = new BlockingCollection<double[]>();
        private LineSeries chartLine;
        private int triggerDelay;
        private bool hasTriggered;
        private int num = 5;
        private TimeSpan timeElapsed;

        public MonitorViewModel(IEventAggregator eventAggregator, ISettings settings)
        {
            _eventAggregator = eventAggregator;
            _settings = settings;
            _samplesDisplayed = (int)_settings["NumberOfSamplesToDisplay"];
            _upperTriggerThreshold = (double)_settings["UpperThreshold"];
            _lowerTriggerThreshold = (double)_settings["LowerThreshold"];
            _chartYAxisMax = (double)_settings["ChartYAxisMax"];
            _chartYAxisMin = (double)_settings["ChartYAxisMin"];

            ResetFailCommand = new DelegateCommand(OnResetCommand, CanExecuteResetCommand);

            MyModel = new PlotModel() { Background = OxyColor.FromRgb(255, 255, 255), PlotAreaBorderColor = OxyColors.Transparent, TextColor = OxyColors.DarkGray, IsLegendVisible = false };
            DispatcherTimer timer = new DispatcherTimer() { Interval = new TimeSpan(0, 0, 0, 0, 200) };
            timer.Tick += Timer_Tick;
            timer.Start();
        }

        public bool HasFlagged
        {
            get { return hasTriggered; }
            set
            {
                hasTriggered = value;
                RaisePropertyChanged();
            }
        }

        public PlotModel MyModel { get; }

        public ICommand ResetFailCommand { get; set; }

        public TimeSpan TimeElapsed
        {
            get { return timeElapsed; }
            set
            {
                timeElapsed = value;
                RaisePropertyChanged();
            }
        }

        public void ClearChart()
        {
            chartLine.Points.Clear();
            PanChart(_samplesDisplayed);
            MyModel.InvalidatePlot(true);
            triggerDelay = 0;
            TimeElapsed = new TimeSpan(0);
            HasFlagged = false;
        }

        public void OnResetCommand()
        {
            HasFlagged = false;
            ((DelegateCommand)ResetFailCommand).RaiseCanExecuteChanged();
            triggerDelay = 0;
        }

        public void PushData(double[] datas, TimeSpan timeElapsed)
        {
            _dataQueue.Add(datas);
            if (triggerDelay == 0)
            {
                TimeElapsed = timeElapsed;
            }
        }

        public void SetUp(string description, CancellationToken cancellationToken)
        {
            _dataQueue = new BlockingCollection<double[]>();
            _chartTitle = description;
            MyModel.Background = OxyColor.FromAColor(255, OxyColors.Black);
            MyModel.Axes.Add(new LinearAxis
            {
                Position = AxisPosition.Left,
                AbsoluteMinimum = _chartYAxisMin,
                AbsoluteMaximum = _chartYAxisMax,
                Minimum = _chartYAxisMin,
                Maximum = _chartYAxisMax,
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
                Maximum = _samplesDisplayed,
                IsAxisVisible = true,
                AxislineColor = OxyColors.Gray,
                AxislineThickness = 1,
                AxislineStyle = LineStyle.Solid,
                Selectable = false
            });
            chartLine = new LineSeries()
            {
                Title = "Series 1",
                Color = OxyColors.SkyBlue
            };
            MyModel.Series.Add(chartLine);
            LineAnnotation MaxLine = new LineAnnotation()
            {
                StrokeThickness = 1,
                Color = OxyColors.DarkRed,
                Type = LineAnnotationType.Horizontal,
                Text = "Max Trigger",
                TextColor = OxyColors.White,
                TextVerticalAlignment = OxyPlot.VerticalAlignment.Top,
                FontSize = 10,
                Y = (double)_settings["UpperThreshold"]
            };
            LineAnnotation MinLine = new LineAnnotation()
            {
                StrokeThickness = 1,
                Color = OxyColors.DarkRed,
                Type = LineAnnotationType.Horizontal,
                Text = "Min Trigger",
                TextColor = OxyColors.White,
                TextVerticalAlignment = OxyPlot.VerticalAlignment.Top,
                FontSize = 10,
                Y = (double)_settings["LowerThreshold"]
            };
            MyModel.Annotations.Add(MaxLine);
            MyModel.Annotations.Add(MinLine);
            MyModel.InvalidatePlot(true);

            Task.Run(() => Update(cancellationToken), cancellationToken);
        }

        private void Update(CancellationToken cancellationToken)
        {
            try
            {
                foreach (var data in _dataQueue.GetConsumingEnumerable(cancellationToken))
                {
                    if (triggerDelay < _samplesDisplayed / 2)
                    {
                        Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                        {
                            lock (MyModel.SyncRoot)
                            {
                                AddDataToChart(data);
                            }
                        }));
                    }

                    if (HasFlagged && triggerDelay < _samplesDisplayed / 2)
                    {
                        triggerDelay += data.Length;
                    }
                }
            }
            catch (OperationCanceledException)
            {
                _dataQueue.CompleteAdding();
                _dataQueue.Dispose();
                return;
            }
        }

        private void AddDataToChart(double[] datas)
        {
            foreach (var data in datas)
            {
                chartLine.Points.Add(new DataPoint(num++, data));
                if ((data > _upperTriggerThreshold || data < _lowerTriggerThreshold) && !HasFlagged)
                {
                    var eventArgs = new FaultDetectedEventArgs() { Description = _chartTitle, EventTime = TimeElapsed };
                    _eventAggregator.GetEvent<FaultDetectedEvent>().Publish(eventArgs);
                    HasFlagged = true;
                    AddFaultAnnotationToChart(data);
                    ((DelegateCommand)ResetFailCommand).RaiseCanExecuteChanged();
                    break;
                }
            }
        }

        private void AddFaultAnnotationToChart(double data)
        {
            OxyPlot.Annotations.EllipseAnnotation fault;
            //ArrowAnnotation fault;
            //if (data > 0)
            //{
            //fault = new ArrowAnnotation()
            //{
            //    EndPoint = new DataPoint(num, data),
            //    StartPoint = new DataPoint(num, _chartYAxisMax),
            //    HeadWidth = 2.5,
            //    HeadLength = 3,
            //    Color = OxyColors.Yellow,
            //    Text = "Fault",
            //    TextColor = OxyColors.Yellow
            //};
            fault = new EllipseAnnotation()
            {
                X = num,
                Y = data,
                Height = _chartYAxisMax/6,
                Width = _samplesDisplayed/75,
                Stroke = OxyColors.Yellow,
                StrokeThickness = 2,
                Fill = OxyColors.Transparent
            };
            //}
            //else
            //{
            //    //fault = new ArrowAnnotation()
            //    //{
            //    //    EndPoint = new DataPoint(num, data),
            //    //    StartPoint = new DataPoint(num, _chartYAxisMin),
            //    //    HeadWidth = 2.5,
            //    //    HeadLength = 3,
            //    //    Color = OxyColors.Yellow,
            //    //    Text = "Fault",
            //    //    TextColor = OxyColors.Yellow
            //    //};
            //}
            MyModel.Annotations.Add(fault);
            
        }

        private bool CanExecuteResetCommand()
        {
            return HasFlagged;
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            Application.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                lock (MyModel.SyncRoot)
                {
                    if (chartLine.Points.Count > _samplesDisplayed)
                    {
                        PanChart(chartLine.Points.Count - _samplesDisplayed);
                        chartLine.Points.RemoveRange(0, (chartLine.Points.Count - _samplesDisplayed));
                    }
                    MyModel.InvalidatePlot(true);
                }
            }));
        }
        
        private void PanChart(int panAmount)
        {
            var axis = MyModel.Axes.First(x => x.Position == AxisPosition.Bottom);
            double panStep = axis.Transform((-1 * panAmount) + axis.Offset);
            axis.Pan(panStep);
        }

        public string ChartTitle
        {
            get { return _chartTitle; }
            set
            {
                _chartTitle = value;
                RaisePropertyChanged();
            }
        }
    }
}

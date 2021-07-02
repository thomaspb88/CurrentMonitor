using CurrentMonitor.Model;
using NationalInstruments;
using NationalInstruments.DAQmx;
using System;
using System.Collections.Generic;
using niTask = NationalInstruments.DAQmx.Task;
using Task = System.Threading.Tasks.Task;
using System.Linq;
using System.Windows;
using System.Collections.Concurrent;

namespace CurrentMonitor.DataAccess
{
    public class CurrentDataProvider : ICurrentDataProvider
    {
        private niTask _myTask;
        private niTask _runningTask;
        private AsyncCallback analogCallback;
        private AnalogMultiChannelReader myAnalogReader;
        private AnalogWaveform<double>[] datas;

        private ConcurrentQueue<CurrentData>[] _currentDatas;

        public CurrentDataProvider()
        {
            _myTask = new niTask();
            myAnalogReader = new AnalogMultiChannelReader(_myTask.Stream);
            myAnalogReader.SynchronizeCallbacks = true;
            
        }

        protected virtual void OnThresholdReached(EventArgs e)
        {
            ThresholdReached?.Invoke(this, e);
        }

        public event EventHandler ThresholdReached;

        public void SetUpDaqChannels(int numberOfChannels)
        {
            NumberOfChannels = numberOfChannels;

            _currentDatas = new ConcurrentQueue<CurrentData>[NumberOfChannels];

            for(var i = 0; i < _currentDatas.GetLength(0); i++)
            {
                _currentDatas[i] = new ConcurrentQueue<CurrentData>();
            }

            try
            {
                var physicalChannelsAvailable = DaqSystem.Local.GetPhysicalChannels(PhysicalChannelTypes.AI, PhysicalChannelAccess.External);

                for (var i = 0; i < NumberOfChannels; i++)
                {
                    _myTask.AIChannels.CreateVoltageChannel(physicalChannelsAvailable[i], "",
                           (AITerminalConfiguration)(-1), Convert.ToDouble(-10),
                            Convert.ToDouble(10), AIVoltageUnits.Volts);
                }

                // Configure the timing parameters
                _myTask.Timing.ConfigureSampleClock("", Convert.ToDouble(1000),
                    SampleClockActiveEdge.Rising, SampleQuantityMode.ContinuousSamples, 10);

                // Verify the Task
                _myTask.Control(TaskAction.Verify);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                _runningTask = null;
                _myTask.Dispose();
                _myTask = null;
            }
        }

        public async Task BeginScan()
        {
            if (NumberOfChannels < 1) throw new ArgumentException("Number of channels has to be greater than 0");
            
            await Task.Run(() =>
            {
                analogCallback = new AsyncCallback(AnalogInCallback);
                _runningTask = _myTask;
                myAnalogReader.BeginReadWaveform(Convert.ToInt32(1000), analogCallback, _myTask);
            });
        }

        //public CurrentData[][] GetCurrentData(int samplesToRetrieve)
        //{
        //    var outputArray = new CurrentData[NumberOfChannels][];

        //    for (var h = 0; h < outputArray.GetLength(0); h++)
        //    {
        //        if(!_currentDatas[h].IsEmpty)
        //        {
        //            int v = samplesToRetrieve > _currentDatas[h].Count ? _currentDatas[h].Count : samplesToRetrieve;
        //            int dataCount = v;

        //            outputArray[h] = new CurrentData[dataCount];

        //            for (var i = 0; i < dataCount; i++)
        //            {
        //                if (_currentDatas[h].TryDequeue(out CurrentData result))
        //                {
        //                    outputArray[h][i] = result;
        //                }
        //                else
        //                {
        //                    var p = 0;
        //                }
        //            }
        //        }
        //    }
        //    return outputArray;
        //}

        public List<CurrentData>[] GetCurrentData()
        {

            var outputArray = new List<CurrentData>[NumberOfChannels];

            for (var h = 0; h < outputArray.GetLength(0); h++)
            {
                outputArray[h] = new List<CurrentData>();

                if (!_currentDatas[h].IsEmpty)
                {
                    for (var i = 0; i < _currentDatas[h].Count; i++)
                    {
                        while(_currentDatas[h].TryDequeue(out CurrentData result))
                        {
                            outputArray[h].Add(result);
                        }
                    }
                }
            }
            return outputArray;
        }

        private void AnalogInCallback(IAsyncResult ar)
        {
            try
            {
                while (_runningTask != null && _runningTask == ar.AsyncState)
                {
                    // Read the available data from the channels
                    datas = myAnalogReader.EndReadWaveform(ar);

                    FeedDataToQueue(datas);
                    
                    if(_currentDatas[0].Count > 5000)
                    {
                        OnThresholdReached(EventArgs.Empty);
                    }

                    myAnalogReader.BeginMemoryOptimizedReadWaveform(Convert.ToInt32(10),
                        analogCallback, _myTask, datas);
                }
            }
            catch (DaqException ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void FeedDataToQueue(AnalogWaveform<double>[] sourceArray)
        {
            // Iterate over channels
            int currentLineIndex = 0;
            foreach (AnalogWaveform<double> waveform in sourceArray)
            {
                var v = waveform.Samples.Count;

                foreach(var sample in waveform.Samples)
                {
                    var newData = new CurrentData() { Time = sample.TimeStamp, Current = sample.Value };
                    _currentDatas[currentLineIndex].Enqueue(newData);
                }
                currentLineIndex++;
            }
        }

        public int NumberOfChannels { get; set; }

    }
}

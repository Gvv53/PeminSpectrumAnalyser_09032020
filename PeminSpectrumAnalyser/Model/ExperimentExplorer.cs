﻿using IOMeasurementData;
using Microsoft.Win32;
using PeminSpectrumData;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;
using UnitedTools.Chart;
using DevExpress.Xpf.Charts;

namespace PeminSpectrumAnalyser.Model
{
    public class ExperimentExplorer
    {
        public SequenceCtrl SequenceCtrl;
        public event Action rbSSCheckedEvent, rbDSCheckedEvent,TimeStart,TimeEnd; //изменение выбора режима СС/ДС
        public event Action<string> HardTypeChanged;
        public event Action<int> WriteThreadId, pBarValue,pBarMax;
        public event Action<bool,ParametersCtrl> StateButtunChart;
        //---------------------------------------------------------------------
        // Данные эксперимента
        //---------------------------------------------------------------------

        public Experiment Experiment = new Experiment();


        public bool SaveExperiment()
        {
            bool result = false;

            SaveFileDialog fd = new SaveFileDialog();

            fd.InitialDirectory = Experiment.ExperimentSettings.ExperimentPath;
            fd.DefaultExt = "PEMIN_DATA";
            fd.Filter = "PEMIN DATA FILES (*.PEMIN_DATA)|*.PEMIN_DATA";
            fd.Title = "SAVE PEMIN DATA FILE";

            if ((bool)fd.ShowDialog())
            {
                try
                {
                    Experiment.SaveToFile(fd.FileName);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("ОШИБКА ЗАПИСИ ФАЙЛА ЭКСПЕРИМЕНТА! " + ex.ToString());
                }

                try
                {
                    Solution.ExportToSCV(Experiment.Intervals, fd.FileName + @".csv");
                }
                catch (Exception ex)
                {
                    MessageBox.Show("ОШИБКА ЗАПИСИ ФАЙЛА CSV! " + ex.ToString());
                }
            }

            return result;
        }

        public bool LoadExperiment()
        {
            bool result = false;

            OpenFileDialog fd = new OpenFileDialog();

            fd.InitialDirectory = Experiment.ExperimentSettings.ExperimentPath;
            fd.DefaultExt = "PEMIN_DATA";
            fd.Filter = "PEMIN DATA FILES (*.PEMIN_DATA)|*.PEMIN_DATA";
            fd.Title = "OPEN PEMIN DATA FILE";

            if ((bool)fd.ShowDialog())
            {
                Experiment = Experiment.LoadFromFile(fd.FileName);

                SequenceCtrl.ClearIntervalsUIList();

                SequenceCtrl.rbDS.IsChecked = Experiment.Intervals.Count == 0 || !Experiment.Intervals[0].IntervalSettings.isAuto;
                if ((bool)SequenceCtrl.rbDS.IsChecked) //режим ДС
                {
                    SequenceCtrl.HandMode_Frequency.Value = Experiment.Ft;
                    SequenceCtrl.HandMode_Quantity.Text = Experiment.Intervals.Count.ToString();
                }
                if (Experiment.Intervals.Count > 0)
                    for (int counter = 0; counter < Experiment.Intervals.Count; counter++)
                        SequenceCtrl.LoadInterval(Experiment.Intervals[counter]);

                SequenceCtrl.Address = Experiment.ExperimentSettings.HardwareSettings.IP;

                result = true;
            }
            return result;
        }

        public void LinkToExperiment(Experiment experiment)
        {
            SequenceCtrl.ClearIntervalsUIList();

            foreach (Interval interval in experiment.Intervals)
                SequenceCtrl.AddNewInterval(interval);

            SequenceCtrl.Address = Experiment.ExperimentSettings.HardwareSettings.IP;
        }


        public ExperimentExplorer()
        {            
            StartPollingProcess();
        }

        public void SettingsOpen()
        {
            String oldValue = Experiment.ExperimentSettings.HardwareSettings.HardwareType.ToString();
            SettingsWindow SettingsWindow = new SettingsWindow();
            SettingsWindow.Settings = Experiment.ExperimentSettings;
            SettingsWindow.ShowDialog();
            String newValue = Experiment.ExperimentSettings.HardwareSettings.HardwareType.ToString();//SettingsWindow.Settings.HardwareSettings.HardwareType.ToString();
            if (oldValue != newValue)  //выбран другой ИП            
                HardTypeChanged?.Invoke(newValue);
        }

        //---------------------------------------------------------------------
        // Логгирование состояний
        //---------------------------------------------------------------------

        //public Action<bool, string> ConnectionStateChanged;
        public Action<bool> ConnectionStateChanged;
        public Action<double[], double[]> NewData;

        //---------------------------------------------------------------------
        // Взаимодействие с аппаратурой
        //---------------------------------------------------------------------
        public DataReader Reader;

        public double[] ResultsX;
        public double[] ResultsY;
        public bool Emulation => Experiment.ExperimentSettings.Emulation;
        public bool IsConnected { get; set; } = false;

        public bool Connect()
        {
            bool result = false;

            Reader = new DataReader(Experiment.ExperimentSettings.HardwareSettings);


            if (Emulation)
            {           
                result = true;
            }
            else
            {
                Reader.HardwareSettings.IP = Experiment.ExperimentSettings.HardwareSettings.IP;
                Reader.HardwareSettings.Port = Experiment.ExperimentSettings.HardwareSettings.Port;
                Reader.HardwareSettings.DbMkvShift = Experiment.ExperimentSettings.HardwareSettings.DbMkvShift;
                Reader.HardwareSettings.PointsQuantity = Experiment.ExperimentSettings.HardwareSettings.PointsQuantity;
                Reader.HardwareSettings.CommonShift = Experiment.ExperimentSettings.CommonShift;

                result = Reader.Connect();
            }

            IsConnected = result;
            ConnectionStateChanged?.Invoke(result);
            return result;
        }

        public bool ReadSignal(long frequency, long bandWidth, long span, long band,bool isManualSWP,double ManualSWP)
        {
            AverageType averageType = AverageType.Off;

            if (Experiment.ExperimentSettings.AverageTypeForSignalOff)
                averageType = AverageType.Off;

            if (Experiment.ExperimentSettings.AverageTypeForSignalMinimum)
                averageType = AverageType.Minimum;

            if (Experiment.ExperimentSettings.AverageTypeForSignalMiddle)
                averageType = AverageType.Middle;

            if (Experiment.ExperimentSettings.AverageTypeForSignalMaximum)
                averageType = AverageType.Maximum;

            return Read(frequency, bandWidth, span, band,
                Experiment.ExperimentSettings.HardwareSettings.SignalTraceDetector,
                averageType,
                Experiment.ExperimentSettings.HardwareSettings.PointsQuantity,
                Experiment.ExperimentSettings.MeasurementCountForSignal,
                Experiment.ExperimentSettings.HardwareSettings.SignalTraceType,
                Experiment.ExperimentSettings.HardwareSettings.SignalAttenuation,
                Experiment.ExperimentSettings.HardwareSettings.CountSignalTraceMode,
                isManualSWP, ManualSWP);
        }


        public bool ReadNoise(long frequency, long bandWidth, long span, long band, bool isManualSWP, double ManualSWP)
        {
            AverageType averageType = AverageType.Off;

            if (Experiment.ExperimentSettings.AverageTypeForNoiseOff)
                averageType = AverageType.Off;

            if (Experiment.ExperimentSettings.AverageTypeForNoiseMinimum)
                averageType = AverageType.Minimum;

            if (Experiment.ExperimentSettings.AverageTypeForNoiseMiddle)
                averageType = AverageType.Middle;

            if (Experiment.ExperimentSettings.AverageTypeForNoiseMaximum)
                averageType = AverageType.Maximum;

            return Read(frequency, bandWidth, span, band,
                    Experiment.ExperimentSettings.HardwareSettings.NoiseTraceDetector,
                    averageType,
                    Experiment.ExperimentSettings.HardwareSettings.PointsQuantity,
                    Experiment.ExperimentSettings.MeasurementCountForNoise,
                    Experiment.ExperimentSettings.HardwareSettings.NoiseTraceType,
                    Experiment.ExperimentSettings.HardwareSettings.NoiseAttenuation,
                    Experiment.ExperimentSettings.HardwareSettings.CountNoiseTraceMode,
                    isManualSWP,
                    ManualSWP);
        }


        public bool Read(long frequency, long bandWidth, long span, long band,
                            string traceDetector,
                            AverageType averageType,
                            int pointsQuantity,
                            int measurementCount,
                            string traceType,
                            long attenuation,
                            long countTraceMode,
                            bool isManualSWP, 
                            double ManualSWP)

        {
            Reader.HardwareSettings.Frequency = frequency;
            Reader.HardwareSettings.BandWidth = bandWidth;
            Reader.HardwareSettings.Span = span;
            Reader.HardwareSettings.Band = band;
            Reader.HardwareSettings.TraceDetector = traceDetector;
            Reader.HardwareSettings.AverageType = averageType;
            Reader.HardwareSettings.PointsQuantity = pointsQuantity;
            Reader.HardwareSettings.MeasurementCount = measurementCount;
            Reader.HardwareSettings.TraceType = traceType;
            Reader.HardwareSettings.Attenuation = attenuation;
            Reader.HardwareSettings.CountTraceMode = countTraceMode;
            Reader.HardwareSettings.isManualSWP = isManualSWP;
            Reader.HardwareSettings.ManualSWP = ManualSWP;
            ResultsX = new double[Reader.HardwareSettings.PointsQuantity];
            ResultsY = new double[Reader.HardwareSettings.PointsQuantity];

            if (Emulation)
            {
                for (int counter = 0; counter < Reader.HardwareSettings.PointsQuantity; counter++)
                {
                    ResultsX[counter] = counter;
                   // if (DataMeasuringType != DataMeasuringType.Signal)  //шум
                    ResultsY[counter] = Math.Sin(counter) * 30;
                }
                if (DataMeasuringType == DataMeasuringType.Signal)
                {
                    ResultsY[0] = 30;
                    ResultsY[10] = 35;
                    ResultsY[20] = 40;
                    ResultsY[40] = 45;
                    ResultsY[60] = 40;
                    ResultsY[70] = 35;
                    ResultsY[80] = 30;
                }
                NewData?.Invoke(ResultsX, ResultsY);
                return true;
            }
            else
            if (Reader.GetDataPoints())
            {
                ResultsX = Reader.ResultsX.ToArray();
                ResultsY = Reader.ResultsY.ToArray();
                for (int counter = 0; counter < Reader.HardwareSettings.PointsQuantity; counter++)
                    if (ResultsY[counter] == 0)
                        ResultsY[counter] = 0.001;

                NewData?.Invoke(ResultsX, ResultsY);
                return true;
            }
            return false;
        }
        //---------------------------------------------------------------------
        // Отработка цикла сканирования
        //---------------------------------------------------------------------
        public event Action<string, string> IntervalChangeEvent;
        public event Action<string, string> PointChangeEvent;
        public event Action<string> ScanProcessEvent;
        public event Action SignalReadyEvent, SignalClearEvent;
        public event Action SignalReadyIntervalEvent, NoiseReadyIntervalEvent;
        public event Action NoiseReadyEvent, NoiseClearEvent;

        public DataMeasuringState DataMeasuringState;

        public DataMeasuringType DataMeasuringType;

        public void StartDataMeasuring() => DataMeasuringState = DataMeasuringState.Start;
        public void PauseDataMeasuring() => DataMeasuringState = DataMeasuringState.Pause;
        public void ContinurDataMeasuring() => DataMeasuringState = DataMeasuringState.Continue;
        public void FinishDataMeasuring() => DataMeasuringState = DataMeasuringState.Finish;

        public void rbSSChecked()
        {
            rbSSCheckedEvent?.Invoke();
        }
        public void rbDSChecked()
        {
            rbDSCheckedEvent?.Invoke();
        }
        public void ShowPollingStatus()
        {
            if (DataMeasuringState == DataMeasuringState.Clear) //изменение строки состояния для нового измерения
            {
                ScanProcessEvent?.Invoke("ОЖИДАНИЕ ЗАПУСКА");
                SignalClearEvent?.Invoke();
                NoiseClearEvent?.Invoke();
            }

            if (DataMeasuringState == DataMeasuringState.Pause)
                ScanProcessEvent?.Invoke("ПАУЗА");

            if (DataMeasuringState == DataMeasuringState.Finish)
            {
                ScanProcessEvent?.Invoke("СКАНИРОВАНИЕ ЗАВЕРШЕНО");

                if (DataMeasuringType == DataMeasuringType.Signal)
                    SignalReadyEvent?.Invoke();

                if (DataMeasuringType == DataMeasuringType.Noise)
                    NoiseReadyEvent?.Invoke();
            }

            if (DataMeasuringState == DataMeasuringState.Interrupted)
                ScanProcessEvent?.Invoke("СКАНИРОВАНИЕ ПРЕРВАНО");

            if (DataMeasuringState == DataMeasuringState.Start || DataMeasuringState == DataMeasuringState.Continue)
            {
                if (DataMeasuringType == DataMeasuringType.Signal)
                    ScanProcessEvent?.Invoke("СКАНИРОВАНИЕ СИГНАЛА");

                if (DataMeasuringType == DataMeasuringType.Noise)
                    ScanProcessEvent?.Invoke("СКАНИРОВАНИЕ ШУМА");
            }
            //индикация просканированных интервалов
            IntervalChangeEvent?.Invoke(Experiment.Intervals.Count.ToString(), (_LocalIntervalCount).ToString());
            PointChangeEvent?.Invoke(_LocalIntervalFrequencys.ToString(), (_LocalPointCount).ToString());
        }

        void ShowIntervalMessage(IntervalSettings intervalSettings)
        {

            if (intervalSettings.EnableMessage1BeforeStartMeasuring)
                if (DataMeasuringType == DataMeasuringType.Signal)
                    SequenceCtrl.Dispatcher.Invoke(() => {
                        MessageWindow messageWindow = new MessageWindow();
                        messageWindow.Message = Experiment.ExperimentSettings.HardwareSettings.HardwareType.ToString() + "," +
                                                "Частотный диапазон:" + (intervalSettings.FrequencyStart/1000000).ToString() + "мГц - " + (intervalSettings.FrequencyStop / 1000000).ToString() + "мГц,"+
                                                Environment.NewLine + 
                                                intervalSettings.Message1BeforeStartMeasuring;
                        messageWindow.ShowDialog();
                    });

            if (intervalSettings.EnableMessage2BeforeStartMeasuring)
                if (DataMeasuringType == DataMeasuringType.Signal)
                    SequenceCtrl.Dispatcher.Invoke(() =>
                    {
                        MessageWindow messageWindow = new MessageWindow();
                        messageWindow.Message = intervalSettings.Message2BeforeStartMeasuring;
                        messageWindow.ShowDialog();
                    });

            if (intervalSettings.EnableMessage3BeforeStartMeasuring)
                if (DataMeasuringType == DataMeasuringType.Signal)
                    SequenceCtrl.Dispatcher.Invoke(() =>
                    {
                        MessageWindow messageWindow = new MessageWindow();
                        messageWindow.Message = intervalSettings.Message3BeforeStartMeasuring;
                        messageWindow.ShowDialog();
                    });


            if (intervalSettings.EnableMessage1BeforeStartMeasuringForNoise)
                if (DataMeasuringType == DataMeasuringType.Noise)
                    SequenceCtrl.Dispatcher.Invoke(() => {
                        MessageWindow messageWindow = new MessageWindow();
                        messageWindow.Message = Experiment.ExperimentSettings.HardwareSettings.HardwareType.ToString() + "," +
                                                "Частотный диапазон:" + intervalSettings.FrequencyStart.ToString() + " - " + intervalSettings.FrequencyStop.ToString() + "," + intervalSettings.Message1BeforeStartMeasuring;
                        messageWindow.ShowDialog();
                    });

            if (intervalSettings.EnableMessage2BeforeStartMeasuringForNoise)
                if (DataMeasuringType == DataMeasuringType.Noise)
                    SequenceCtrl.Dispatcher.Invoke(() =>
                    {
                        MessageWindow messageWindow = new MessageWindow();
                        messageWindow.Message = intervalSettings.Message2BeforeStartMeasuring;
                        messageWindow.ShowDialog();
                    });

            if (intervalSettings.EnableMessage3BeforeStartMeasuringForNoise)
                if (DataMeasuringType == DataMeasuringType.Noise)
                    SequenceCtrl.Dispatcher.Invoke(() =>
                    {
                        MessageWindow messageWindow = new MessageWindow();
                        messageWindow.Message = intervalSettings.Message3BeforeStartMeasuring;
                        messageWindow.ShowDialog();
                    });
        }

        int _LocalIntervalCount = 0;
        int _LocalIntervalFrequencys = 0;
        int _LocalPointCount = 0;
        public delegate void InvokeDelegate();
        public void Polling(int ThreadId)
        {
            ShowPollingStatus();
            if (DataMeasuringState == DataMeasuringState.Start)
            {
                _LocalIntervalCount = 0;

                ShowPollingStatus();
                WriteThreadId?.Invoke(ThreadId);
                TimeStart?.Invoke();
                //количество интервалов измерений
                int countInt = 0;
                foreach(var interval in Experiment.Intervals.Where(p => p.isActive))
                {
                    countInt += interval.CenterFrequencys.Count;
                }
                pBarMax?.Invoke(countInt);
                int Value = 0;
                //StateButtunChart?.Invoke(false);  //деактивация кнопок График
                foreach (Interval currentInterval in Experiment.Intervals)
                {
                    if (currentInterval.isActive)
                    {                                                
                        ShowPollingStatus();

                        ShowIntervalMessage(currentInterval.IntervalSettings);

                        if (DataMeasuringType == DataMeasuringType.Signal)
                            currentInterval.SignalClear();
                        else
                            currentInterval.NoiseClear();

                        currentInterval.Frequencys.Clear();
                        //количество центральных частот в текущем интервале(>1, точек измерения получается больше, чем может отобразить прибор)
                        _LocalIntervalFrequencys = currentInterval.CenterFrequencys.Count;
                        _LocalPointCount = 0;
                        if (currentInterval.CenterFrequencys.Count == 0)
                        {
                            MessageBox.Show("Не возможно выполнить измерения." + Environment.NewLine + "Вероятно Вы забыли нажать кнопку 'РАССЧИТАТЬ'");
                            DataMeasuringState = DataMeasuringState.Clear;
                            return;
                        }
                        StateButtunChart?.Invoke(false,(ParametersCtrl)currentInterval.IntervalSettings.LinkToVisualControl);  //деактивация кнопок График
                        //pBarMax?.Invoke(Experiment.Intervals.Where(p => p.isActive).Count() * currentInterval.CenterFrequencys.Count);
                        foreach (long currentFrequency in currentInterval.CenterFrequencys)
                        {

                            ShowPollingStatus();

                            while (DataMeasuringState == DataMeasuringState.Pause)
                            {

                                if (DataMeasuringState == DataMeasuringState.Interrupted)
                                {

                                    if (DataMeasuringType == DataMeasuringType.Signal)
                                        currentInterval.SignalClear();
                                    else
                                        currentInterval.NoiseClear();

                                    ShowPollingStatus();
                                    return;
                                }

                                ShowPollingStatus();
                                Thread.Sleep(100);
                            }

                            if (DataMeasuringState == DataMeasuringState.Interrupted)
                            {
                                if (DataMeasuringType == DataMeasuringType.Signal)
                                    currentInterval.SignalClear();
                                else
                                    currentInterval.NoiseClear();

                                ShowPollingStatus();
                                return;
                            }

                            bool dataReady = false;                            

                            if (DataMeasuringType == DataMeasuringType.Signal)
                            {
                                dataReady = ReadSignal(currentFrequency,
                                    currentInterval.IntervalSettings.BandWidth,
                                    currentInterval.IntervalSettings.Span,
                                    currentInterval.IntervalSettings.Band,
                                    currentInterval.IntervalSettings.isManuaSWPTime,
                                    currentInterval.IntervalSettings.ManuaSWPTime);

                                if (dataReady)
                                {
                                    if (currentInterval.Frequencys.Count != 0 )
                                    {
                                        double[] tmpX = new double[ResultsX.Length - 1]; 
                                        Array.Copy(ResultsX, 1, tmpX, 0, ResultsX.Length - 1);

                                        double[] tmpY = new double[ResultsY.Length - 1];
                                        Array.Copy(ResultsY, 1, tmpY, 0, ResultsY.Length - 1);

                                        currentInterval.Frequencys.AddRange(tmpX);
                                        currentInterval.OriginalSignal.AddRange(tmpY);
                                    }
                                    else
                                    {
                                        currentInterval.Frequencys.AddRange(ResultsX);
                                        currentInterval.OriginalSignal.AddRange(ResultsY);
                                    }

                                }
                            }
                            else
                            {
                                dataReady = ReadNoise(currentFrequency,
                                    currentInterval.IntervalSettings.BandWidth,
                                    currentInterval.IntervalSettings.Span,
                                    currentInterval.IntervalSettings.Band,
                                    currentInterval.IntervalSettings.isManuaSWPTime,
                                    currentInterval.IntervalSettings.ManuaSWPTime);

                                if (dataReady)
                                {
                                    if (currentInterval.Frequencys.Count != 0)
                                    {
                                        double[] tmpX = new double[ResultsX.Length - 1];
                                        Array.Copy(ResultsX, 1, tmpX, 0, ResultsX.Length - 1);

                                        double[] tmpY = new double[ResultsY.Length - 1];
                                        Array.Copy(ResultsY, 1, tmpY, 0, ResultsY.Length - 1);

                                        currentInterval.Frequencys.AddRange(tmpX);
                                        currentInterval.OriginalNoise.AddRange(tmpY);
                                    }
                                    else
                                    {
                                        currentInterval.Frequencys.AddRange(ResultsX);
                                        currentInterval.OriginalNoise.AddRange(ResultsY);
                                    }
                                }
                            }

                            _LocalPointCount++;
                            Value++;
                            pBarValue?.Invoke(Value);
                           
                        }

                        if (currentInterval.IntervalSettings.isAuto)//только для СС
                            currentInterval.Computer(); //обработка полученных сигналов измерения
                        else
                            currentInterval.Restore(); //для СС. Копирование ориг. значений без обработки

                        //сигнал, меньший шума, подтянем до уровня шума
                        if (currentInterval.Signal.Count != 0 && currentInterval.Noise.Count != 0)
                            for (int i = 0; i < currentInterval.Frequencys.Count; i++)
                            {
                                if (currentInterval.Signal[i] < currentInterval.Noise[i])
                                    currentInterval.Signal[i] = currentInterval.Noise[i];
                            }

                        _LocalIntervalCount++;
                        StateButtunChart?.Invoke(true, (ParametersCtrl)currentInterval.IntervalSettings.LinkToVisualControl);
                    }
                }
                TimeEnd?.Invoke();
                DataMeasuringState = DataMeasuringState.Finish;
                ShowPollingStatus();
              
                if (DataMeasuringType == DataMeasuringType.Signal)
                {
                    SignalReadyIntervalEvent?.Invoke();                    
                   MessageBox.Show(Experiment.ExperimentSettings.HardwareSettings.HardwareType
                                           + "- -СЪЕМ СИГНАЛА ЗАВЕРШЕН"+ Environment.NewLine
                                           + "Поток - " + ThreadId.ToString());
                }
                if (DataMeasuringType == DataMeasuringType.Noise)
                {
                    MessageBox.Show(Experiment.ExperimentSettings.HardwareSettings.HardwareType
                                           + " - СЪЕМ ШУМА ЗАВЕРШЕН" + Environment.NewLine
                                           + "Поток - " + ThreadId.ToString());
                    NoiseReadyIntervalEvent?.Invoke();
                }
                pBarValue?.Invoke(0);
                pBarMax?.Invoke(0);
              //  StateButtunChart?.Invoke(true);  //активация кнопок График

            }
        }

        bool EnablePolling { get; set; } = true;
        int _ErrorCount = 0;

        private Thread _PollingThread;

        public void StopPollingProcess() => EnablePolling = false;
        public void StartPollingProcess()
        {
            EnablePolling = true;
            _ErrorCount = 0;

            (_PollingThread = new Thread(new ThreadStart(() =>
            {
                while (EnablePolling)
                {
                    Thread.Sleep(10);
                    if (IsConnected)
                    {
                        try
                        {
                            Polling(_PollingThread.ManagedThreadId);
                        }
                        catch (Exception ex)
                        {
                            _ErrorCount++;

                            if (_ErrorCount == 2)
                            {
                                MessageBox.Show("Превышено заданное количество возникших подряд исключительных ситуаций при обработке потока опроса аппаратуры. Текст последней: "
                                    + ex.ToString() +
                                    "          Опрос остановлен. Рекомендуется перезапустить приложение проверив настройки и параметры подключения а так же состояние аппаратуры и физического соединения");

                                EnablePolling = false;
                            }
                        }
                    }
                }
            }))).Start();
        }
        public void ShowChart(Interval interval, List<double> sourceSignal, List<double> sourceNoise)
        {
            if (interval.Frequencys.Count == 0)
            {
                MessageBox.Show("Данные для отображения отсутствуют");
                return;
            }
            if (sourceSignal.Count != 0 && interval.Frequencys.Count != sourceSignal.Count)
            {
                MessageBox.Show("Количество частот != количеству сигналов");
                return;
            }
            if (sourceNoise.Count != 0 && interval.Frequencys.Count != sourceNoise.Count)
            {
                MessageBox.Show("Количество частот != количеству шума");
                return;
            }
            ObservableCollection<PointForChart> dataSignal = new ObservableCollection<PointForChart>();
            int iDS=0; //маркер точки измерения для ДС
            for (int i = 0; i < interval.Frequencys.Count; i++)
            {
                var point = new PointForChart(interval.Frequencys[i], sourceSignal.Count != 0 ? sourceSignal[i] : 0,
                                              sourceNoise.Count != 0 ? sourceNoise[i] : 0);

                if (interval.Markers.Contains(i))
                {
                    
                    point.signal_marker = sourceSignal.Count != 0 ? sourceSignal[i] : 0;
                    point.noise_marker = sourceNoise.Count != 0 ? sourceNoise[i] : 0;
                    if (!interval.IntervalSettings.isAuto) //ДС
                        iDS = i;
                }
                dataSignal.Add(point);
            }
            SeriesPoint pointUpdated = new SeriesPoint();
            ChartWindow chartWindow = new ChartWindow(dataSignal,!interval.IntervalSettings.isAuto, pointUpdated); 
            chartWindow.Title = Experiment.ExperimentSettings.HardwareSettings.HardwareType.ToString();
            chartWindow.ShowDialog();
            if (iDS != 0 && pointUpdated != null && !String.IsNullOrEmpty(pointUpdated.Argument) && pointUpdated.Value != null) //ДС, мог быть скорректирован
            {
                interval.Frequencys[iDS] = double.Parse(pointUpdated.Argument);
                interval.CenterFrequency = (long)double.Parse(pointUpdated.Argument);
                interval.CenterFrequencys[0] = (long)double.Parse(pointUpdated.Argument);
                interval.IntervalSettings.HandCenterFrequency = (long)double.Parse(pointUpdated.Argument);

                interval.Signal[iDS] = pointUpdated.Value;
                interval.OriginalSignal[iDS] = pointUpdated.Value;
                //обновление значений на форме
                ((ParametersCtrl)interval.IntervalSettings.LinkToVisualControl).HandCenterFrequency.Value = (long)interval.Frequencys[iDS];
                ((ParametersCtrl)interval.IntervalSettings.LinkToVisualControl).tbSignal.Text = pointUpdated.Value.ToString();
            }
            //chartWindow.Show();
        } 
       
        //---------------------------------------------------------------------
        // Отображение шума и сигнала
        //---------------------------------------------------------------------
        //public void ShowSignalAndNoise(Interval interval, List<double> sourceSignal, List<double> sourceNoise,
        //    Action<double> newSignalShift = null, Action<double> newNoiseShift = null, Action<double> newXShift = null)
        //{
        //    if (interval.Frequencys.Count == 0)
        //    {
        //        MessageBox.Show("Данные для отображения отсутствуют");
        //        return;
        //    }

        //    GraphWindow GraphWindowStatic = new GraphWindow();


        //    GraphWindowStatic.CurrentChart.NewXShift += (value) =>
        //    {
        //        newXShift?.Invoke(value);
        //    };


        //    GraphWindowStatic.WindowState = WindowState.Normal;
        //    GraphWindowStatic.Topmost = true;
        //    GraphWindowStatic.Show();

        //    double signalShift = 0;
        //    double noiseShift = 0;
        //    double xScale = 0;
        //    double yScale = 0;

        //    ShowSignalAndNoise(GraphWindowStatic,
        //                        interval,
        //                        sourceSignal,
        //                        sourceNoise,
        //                        signalShift,
        //                        noiseShift,
        //                        yScale,
        //                        xScale);



        //    GraphWindowStatic.CurrentShiftTuner.TunerSignalShift.NewValue += (value, oldValue) =>
        //    {
        //        newSignalShift?.Invoke(value);

        //        signalShift = value;
        //        ShowSignalAndNoise(GraphWindowStatic,
        //                           interval,
        //                           sourceSignal,
        //                           sourceNoise,
        //                           signalShift,
        //                           noiseShift,
        //                           yScale,
        //                           xScale);

        //        if (GraphWindowStatic.CurrentShiftTuner.LinkSignalNoise)
        //        {
        //            double linkedShift = value - oldValue;
        //            GraphWindowStatic.CurrentShiftTuner.LinkSignalNoise = false;
        //            GraphWindowStatic.CurrentShiftTuner.TunerNoiseShift.Value = GraphWindowStatic.CurrentShiftTuner.TunerNoiseShift.Value += linkedShift;
        //            GraphWindowStatic.CurrentShiftTuner.LinkSignalNoise = true;

        //        }
        //    };

        //    GraphWindowStatic.CurrentShiftTuner.TunerNoiseShift.NewValue += (value, oldValue) =>
        //    {
        //        newNoiseShift?.Invoke(value);

        //        noiseShift = value;
        //        ShowSignalAndNoise(GraphWindowStatic,
        //           interval,
        //           sourceSignal,
        //           sourceNoise,
        //           signalShift,
        //           noiseShift,
        //           yScale,
        //           xScale);

        //        if (GraphWindowStatic.CurrentShiftTuner.LinkSignalNoise)
        //        {
        //            double linkedShift = value - oldValue;
        //            GraphWindowStatic.CurrentShiftTuner.LinkSignalNoise = false;
        //            GraphWindowStatic.CurrentShiftTuner.TunerSignalShift.Value = GraphWindowStatic.CurrentShiftTuner.TunerSignalShift.Value += linkedShift;
        //            GraphWindowStatic.CurrentShiftTuner.LinkSignalNoise = true;

        //        }



        //    };

        //    GraphWindowStatic.CurrentShiftTuner.TunerXScale.NewValue += (value, oldValue) =>
        //    {
        //        xScale = value;
        //        ShowSignalAndNoise(GraphWindowStatic,
        //           interval,
        //           sourceSignal,
        //           sourceNoise,
        //           signalShift,
        //           noiseShift,
        //           yScale,
        //           xScale);
        //    };

        //    GraphWindowStatic.CurrentShiftTuner.TunerYScale.NewValue += (value, oldValue) =>
        //    {
        //        yScale = value;
        //        ShowSignalAndNoise(GraphWindowStatic,
        //           interval,
        //           sourceSignal,
        //           sourceNoise,
        //           signalShift,
        //           noiseShift,
        //           yScale,
        //           xScale);
        //    };

        //}

        //public void ShowSignalAndNoise(GraphWindow graphWindowStatic,
        //                               Interval interval,
        //                               List<double> sourceSignal,
        //                               List<double> sourceNoise,
        //                               double signalShift,
        //                               double noiseShift,
        //                               double yShift,
        //                               double xShift)
        //{
        //    double maxValue = 0, minValue = 0, maxValueSignal = 0, minValueSignal = 0, maxValueNoise = 0, minValueNoise = 0; ;

        //    if (sourceSignal.Count != 0)
        //    {
        //        maxValueSignal = sourceSignal.Max();
        //        minValueSignal = sourceSignal.Min();
        //    }
        //    if (sourceNoise.Count != 0)
        //    {
        //        maxValueNoise = sourceNoise.Max();
        //        minValueNoise = sourceNoise.Min();
        //    }
        //    maxValue = maxValueSignal == 0 ? maxValueNoise:(maxValueNoise == 0 ? maxValueSignal : Math.Max(maxValueSignal, maxValueNoise));
        //    minValue = minValueSignal == 0 ? minValueNoise:(minValueNoise == 0 ? minValueSignal : Math.Min(minValueSignal, minValueNoise));
        
        //    graphWindowStatic.Dispatcher.Invoke(DispatcherPriority.Normal, new
        //    Action(() =>
        //    {

        //        graphWindowStatic.CurrentChart.YAxisBeginValue = minValue - 15;
        //        graphWindowStatic.CurrentChart.YAxisEndValue = maxValue + 15;

        //        double xOnePercent = (interval.Frequencys[interval.Frequencys.Count - 1] - interval.Frequencys[0]) / 100;

        //        graphWindowStatic.CurrentChart.XAxisBeginValue = interval.Frequencys[0] - xOnePercent * xShift;
        //        graphWindowStatic.CurrentChart.XAxisEndValue = interval.Frequencys[interval.Frequencys.Count - 1] + xOnePercent * xShift;

        //        graphWindowStatic.Title = "ЗАПИСАННЫЕ ДАННЫЕ ДЛЯ ДИАПАЗОНА " +
        //            graphWindowStatic.CurrentChart.XAxisBeginValue / 1000000 + " MHz  -  "
        //            + graphWindowStatic.CurrentChart.XAxisEndValue / 1000000 + " MHz";


        //        double[] frequencys = interval.ConvertXDataForChartView(Experiment.ExperimentSettings.HardwareSettings.PointsQuantity,
        //                                                                interval.Frequencys);

        //        double[] signal = interval.ConvertYDataForChartView(Experiment.ExperimentSettings.HardwareSettings.PointsQuantity,
        //                                                sourceSignal);

        //        double[] noise = interval.ConvertYDataForChartView(Experiment.ExperimentSettings.HardwareSettings.PointsQuantity,
        //                                sourceNoise);


        //        Graph graphSignal = new Graph();
        //        graphSignal.PointRadius = 10;
        //        graphSignal.Color = new SolidColorBrush(Colors.Red);

        //        Graph graphNoise = new Graph();
        //        graphNoise.PointRadius = 10;
        //        graphNoise.Color = new SolidColorBrush(Colors.Gray);

        //        if (signal.Count() > 0)
        //            for (int counter = 0; counter < Experiment.ExperimentSettings.HardwareSettings.PointsQuantity; counter++)
        //                if (counter < frequencys.Count() & counter < signal.Count())
        //                    graphSignal.Points.Add(new System.Windows.Point() { X = frequencys[counter], Y = signal[counter] + signalShift });

        //        if (noise.Count() > 0)
        //            for (int counter = 0; counter < Experiment.ExperimentSettings.HardwareSettings.PointsQuantity; counter++)
        //                if (counter < frequencys.Count() & counter < noise.Count())
        //                    graphNoise.Points.Add(new System.Windows.Point() { X = frequencys[counter], Y = noise[counter] + noiseShift });

        //        graphWindowStatic.CurrentChart.Graphs.Clear();
        //        graphWindowStatic.CurrentChart.Graphs.Add(graphSignal);
        //        graphWindowStatic.CurrentChart.Graphs.Add(graphNoise);

        //        graphWindowStatic.CurrentChart.Draw();
        //    }));
        //}
    }
}
﻿using PeminSpectrumAnalyser.Model;
using PeminSpectrumData;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System;
using Forms = System.Windows.Forms;
using IOMeasurementData;

namespace PeminSpectrumAnalyser
{
    /// <summary>
    /// Interaction logic for SequenceCtrl.xaml
    /// </summary>
    public partial class SequenceCtrl : UserControl
    {
        //Тактовая частота для ДС
        public double Ft
        {
            set;
            get;
        }
        //полосы пропускания фильтра для ДС
        public long BandWidth_DS { get; set; }
        public long Band_DS { get; set; }
        public string Address
        {
            set => address.Content = value;
            get => address.Content.ToString();
        }

        public ExperimentExplorer ExperimentExplorer = new ExperimentExplorer();

        public event Action SolutionNameClear;
        public event Action ChangedButtonEnabled;

        public SequenceCtrl()
        {
            InitializeComponent();


            ExperimentExplorer.SequenceCtrl = this;

            BandWidth_DS = new IntervalSettings().BandWidth;
            Band_DS = new IntervalSettings().Band;

            ExperimentExplorer.IntervalChangeEvent += (quantity, count) => intervals.Dispatcher.Invoke(() => { intervals.Content = quantity + " / " + count; });
            ExperimentExplorer.PointChangeEvent += (quantity, count) => points.Dispatcher.Invoke(() => { points.Content = quantity + " / " + count; });
            ExperimentExplorer.ScanProcessEvent += (description) => scanProcess.Dispatcher.Invoke(() => { scanProcess.Content = description; });
            ExperimentExplorer.SignalReadyEvent += () => SignalStateLabel.Dispatcher.Invoke(() =>
            {
                SignalStateLabel.Content = "СИГНАЛ СНЯТ";                
            });
            ExperimentExplorer.SignalReadyIntervalEvent += () => SignalStateLabel.Dispatcher.Invoke(() =>
            {             
                //отобразим снятый сигнал для интервалов ДС
                foreach (ParametersCtrl par in ParametersList.Items)
                {
                    if (par.Interval.IntervalSettings.isAuto)
                        return;
                    par.tbSignal.Text = par.Interval.OriginalSignal[par.Interval.Markers[0]].ToString();
                }
            });
            ExperimentExplorer.NoiseReadyEvent += () => NoiseStateLabel.Dispatcher.Invoke(() =>
            {
                NoiseStateLabel.Content = "ШУМ СНЯТ" ;
                
            });
            ExperimentExplorer.NoiseReadyIntervalEvent += () => NoiseStateLabel.Dispatcher.Invoke(() =>
            {
               
                //отобразим снятый шум для интервалов ДС
                foreach (ParametersCtrl par in ParametersList.Items)
                {
                    if (par.Interval.IntervalSettings.isAuto)
                        return;
                    par.tbNoise.Text = par.Interval.OriginalNoise[par.Interval.Markers[0]].ToString();
                }

            });
            ExperimentExplorer.SignalClearEvent += () => SignalStateLabel.Dispatcher.Invoke(() =>
            {
                SignalStateLabel.Content = "СИГНАЛ НЕ СНЯТ";

            });
            ExperimentExplorer.NoiseClearEvent += () => NoiseStateLabel.Dispatcher.Invoke(() =>
            {
                NoiseStateLabel.Content ="ШУМ НЕ СНЯТ";
            });
            //обработчик состояния подключения, управляет видимостью кнопок снятия измерений            
            ExperimentExplorer.ConnectionStateChanged += (bool isConnected) => buttonStartNOISE.Dispatcher.Invoke(() =>
            {
                var intervals = ExperimentExplorer.Experiment.Intervals;
                bool enabled = false;
                if (intervals.Count > 0 && intervals[0].Markers.Count > 0)
                    enabled = true;
                buttonStartNOISE.IsEnabled = isConnected && enabled;
                buttonStartSIGNAL.IsEnabled = isConnected && enabled;
                //MainWindow mw = ((MainWindow)((Grid)((GroupBox)Parent).Parent).Parent);
                //mw.unit1.ChangedButtonEnabled?.Invoke();
                //mw.unit2.ChangedButtonEnabled?.Invoke();
            });
            //режим СС
            ExperimentExplorer.rbSSCheckedEvent += () => 
            {
                gbDS.IsEnabled = false;
                gbDS.Visibility = Visibility.Hidden;
                //spFrequencyMax.IsEnabled = false;
                //gbRBWVBW.IsEnabled = false;
                ParametersList.Items.Clear();
                ExperimentExplorer.Experiment.Intervals.Clear();
                AddNewInterval();
                //активность кнопок измерения
                buttonStartNOISE.IsEnabled = false;
                buttonStartSIGNAL.IsEnabled = false;
            };
            //режим ДС
            ExperimentExplorer.rbDSCheckedEvent += () =>
            {
                gbDS.IsEnabled = true;
                gbDS.Visibility = Visibility.Visible;
                ParametersList.Items.Clear();
                ExperimentExplorer.Experiment.Intervals.Clear();
                //активность кнопок измерения
                buttonStartNOISE.IsEnabled = false;
                buttonStartSIGNAL.IsEnabled = false;
                //полоса фильтра активна только для FSH4
                if (ExperimentExplorer.Experiment.ExperimentSettings.HardwareSettings.HardwareType == HardwareType.FSH4)
                    gbRBWVBW.IsEnabled = true;
                else
                    gbRBWVBW.IsEnabled = false;
                HandRBW.IsEnabled = (bool)cbRBW.IsChecked;
                HandVBW.IsEnabled = (bool)cbVBW.IsChecked;
                CheckMsg();
            };
            
            //обработчик изменения тактовой частоты ДС
            HandMode_Frequency.FrequencyCtrlChanged += () => HandMode_Frequency.Dispatcher.Invoke(() =>
            {
                if (this.ParametersList.Items.Count > 0)
                {
                    if (MessageBoxResult.No == MessageBox.Show("Изменилось значение тактовой частоты. Подтвердите удаление интервалов.", String.Empty, MessageBoxButton.YesNo))
                    {
                        if (MessageBoxResult.No == MessageBox.Show("Восстановить прежнее значение тактовой частоты? При отказе список интервалов будет удалён", String.Empty, MessageBoxButton.YesNo))
                        {
                            NewExperiment();
                            return;
                        }
                        else
                        {
                            //восстановление тактовой частоты по значению частоты 1-го интервала
                            HandMode_Frequency.Value = ((ParametersCtrl)ParametersList.Items[0]).HandCenterFrequency.Value;
                            return;
                        }
                    }
                    NewExperiment();
                }
            });
            //обработчик изменения ширины полосы пропускания фильтров для ДС
            HandRBW.HandRBWChanged += (long value) => {
                foreach (ParametersCtrl par in ParametersList.Items)
                {                    
                    par.Interval.IntervalSettings.BandWidth = value;
                }
            };
            HandVBW.HandVBWChanged += (long value) => {
                foreach (ParametersCtrl par in ParametersList.Items)
                {
                    par.Interval.IntervalSettings.Band = value;                   
                }
            };
            //заполнение статус бара из потока
            ExperimentExplorer.TimeStart += () => stbBegin.Dispatcher.Invoke(() => {stbBegin.Text = DateTime.Now.ToLongTimeString();});
            ExperimentExplorer.TimeEnd += () => stbEnd.Dispatcher.Invoke(() => {stbEnd.Text = DateTime.Now.ToLongTimeString();});
            ExperimentExplorer.WriteThreadId += (int numberId) => stbThread.Dispatcher.Invoke(() => { stbThread.Text = numberId.ToString(); });
            ExperimentExplorer.pBarMax += (int pbMax) => stbThread.Dispatcher.Invoke(() => {  if (pbMax == 0)
                {                   
                    stbThread.Text = String.Empty;
                    stbBegin.Text = String.Empty;
                    stbEnd.Text = String.Empty;
                }
            else
                 pBar.Maximum = pbMax;
            });
            ExperimentExplorer.pBarValue += (int pbValue) => stbThread.Dispatcher.Invoke(() => { pBar.Value = pbValue; });
            
        }

        private void RbSS_Checked(object sender, RoutedEventArgs e)
        {
            ExperimentExplorer.rbSSChecked();
        }
        private void RbDS_Checked(object sender, RoutedEventArgs e)
        {
            ExperimentExplorer.rbDSChecked();
        }
        public void Connect()
        {
            if (!ExperimentExplorer.IsConnected)
            {
                if (ExperimentExplorer.Connect())
                    connectionState.Fill = new SolidColorBrush(Colors.Lime);
                else
                    connectionState.Fill = new SolidColorBrush(Colors.Red);
            }
        }

        public void Disconnect()
        {
            ExperimentExplorer.IsConnected = false;
        }

        private void ButtonConnect_Click(object sender, RoutedEventArgs e)
        {
            Disconnect();
            Connect();
        }

        private void ButtonNew_Click(object sender, RoutedEventArgs e) => NewExperiment();


        public Experiment NewExperiment()
        {
            ExperimentSettings oldExperimentSettings = ExperimentExplorer.Experiment.ExperimentSettings;
            ExperimentExplorer.Experiment = new Experiment();
            ExperimentExplorer.Experiment.ExperimentSettings = oldExperimentSettings;     //установки оборудования сохраняем
            ParametersList.Items.Clear();
            //Address = ExperimentExplorer.Experiment.ExperimentSettings.HardwareSettings.IP; // не меняется
            //сброс строки состояний в исходное значение
            // DataMeasuringState oldState = ExperimentExplorer.DataMeasuringState;
            ExperimentExplorer.DataMeasuringState = DataMeasuringState.Clear;
            ExperimentExplorer.ShowPollingStatus();
            if ((bool)rbSS.IsChecked) //для СС сразу добавляем окно интервала
                AddNewInterval();
            // ExperimentExplorer.DataMeasuringState = oldState;
            SolutionNameClear?.Invoke(); //очистка названия загруженного файла
            return ExperimentExplorer.Experiment;
        }

        public void AddressRefresh() => Address = ExperimentExplorer.Experiment.ExperimentSettings.HardwareSettings.IP + ":" +
                                                                        ExperimentExplorer.Experiment.ExperimentSettings.HardwareSettings.Port.ToString();


        private void ButtonLoad_Click(object sender, RoutedEventArgs e)
        {
            ExperimentExplorer.LoadExperiment();
            Address = ExperimentExplorer.Experiment.ExperimentSettings.HardwareSettings.IP + ":" +
                                                                        ExperimentExplorer.Experiment.ExperimentSettings.HardwareSettings.Port.ToString();
        }

        private void ButtonSave_Click(object sender, RoutedEventArgs e)
        {
            ExperimentExplorer.SaveExperiment();
        }

        private void ButtonPlus_Click(object sender, RoutedEventArgs e) => AddNewInterval();

        //создание контрола ParametersCtrl со значениями элементов из interval
        public void LoadInterval(Interval interval)
        {
            interval.SetExperimentExplorer(ExperimentExplorer);

            ParametersCtrl intervalParametersCtrl = new ParametersCtrl(ExperimentExplorer.Experiment.ExperimentSettings.HardwareSettings);
            intervalParametersCtrl.Interval = interval;

            interval.IntervalSettings.LinkToVisualControl = intervalParametersCtrl;

            intervalParametersCtrl.UIFrom(interval);

            intervalParametersCtrl.Delete += (target) =>
            {
                ExperimentExplorer.Experiment.Intervals.Remove(target.Interval);
                ParametersList.Items.Remove(target);
            };

            ParametersList.Items.Add(intervalParametersCtrl);
        }
        //добавление контрола ParametersCtrl.Значения параметров по умолчанию - для СС
        public void AddNewInterval(Interval interval = null, bool isAutoStyle = true, long frequencyCenter = 1000000)
        {

            Interval newInterval = interval ?? new Interval();

            newInterval.SetExperimentExplorer(ExperimentExplorer);

            newInterval.IntervalSettings.isAuto = isAutoStyle;

            newInterval.IntervalSettings.HandCenterFrequency = frequencyCenter;

            ExperimentExplorer.Experiment.Intervals.Add(newInterval);

            ParametersCtrl intervalParametersCtrl = new ParametersCtrl(ExperimentExplorer.Experiment.ExperimentSettings.HardwareSettings);
            //для СС для полоса фильтра активна только для FSH4
            if ((bool)rbSS.IsChecked && ExperimentExplorer.Experiment.ExperimentSettings.HardwareSettings.HardwareType == HardwareType.FSH4)            
                intervalParametersCtrl.gbFilter.IsEnabled = true;
            else
                intervalParametersCtrl.gbFilter.IsEnabled = false;


            intervalParametersCtrl.IsAutoStyle = newInterval.IntervalSettings.isAuto;

            //Маркеры определяются при рассчёте
            //if (newInterval.IntervalSettings.isAuto)
            //    newInterval.Markers.Add(((newInterval.IntervalSettings.PointsQuantity - 1) / 2) + 1);


            intervalParametersCtrl.Interval = newInterval;

            newInterval.IntervalSettings.LinkToVisualControl = intervalParametersCtrl;
            intervalParametersCtrl.UIFrom(newInterval);

            //кнопка РАССЧИТАТЬ для диф.спектра
            if (!newInterval.IntervalSettings.isAuto)
            {
                intervalParametersCtrl.UITo(newInterval);
                newInterval.BuildAutomaticPoints();
                intervalParametersCtrl.UIFrom(newInterval);
                ExperimentExplorer.ConnectionStateChanged?.Invoke(ExperimentExplorer.IsConnected);
            }
            //



            intervalParametersCtrl.Delete += (target) =>
            {
                ExperimentExplorer.Experiment.Intervals.Remove(target.Interval);
                ParametersList.Items.Remove(target);
            };


            ParametersList.Items.Add(intervalParametersCtrl);

        }


        public void ClearIntervalsUIList()
        {
            ParametersList.Items.Clear();
        }


        private void ButtonStartNOISE_Click(object sender, RoutedEventArgs e) => StartNoiseScan();

        public void StartNoiseScan()
        {

            if (ExperimentExplorer.DataMeasuringState == DataMeasuringState.Pause)
            {
                ExperimentExplorer.DataMeasuringState = DataMeasuringState.Continue;
                return;
            }

            ExperimentExplorer.DataMeasuringType = DataMeasuringType.Noise;
            ExperimentExplorer.DataMeasuringState = DataMeasuringState.Start;
        }

        private void ButtonStartSIGNAL_Click(object sender, RoutedEventArgs e) => StartSignalScan();

        public void StartSignalScan()
        {
            if (ExperimentExplorer.DataMeasuringState == DataMeasuringState.Pause)
            {
                ExperimentExplorer.DataMeasuringState = DataMeasuringState.Continue;
                return;
            }

            ExperimentExplorer.DataMeasuringType = DataMeasuringType.Signal;
            ExperimentExplorer.DataMeasuringState = DataMeasuringState.Start;
        }

        private void ButtonPause_Click(object sender, RoutedEventArgs e)
        {
            ExperimentExplorer.DataMeasuringState = DataMeasuringState.Pause;
        }

        private void ButtonStop_Click(object sender, RoutedEventArgs e)
        {
            ExperimentExplorer.DataMeasuringState = DataMeasuringState.Interrupted;
        }

        private void ButtonSettings_Click(object sender, RoutedEventArgs e)
        {
            bool EmulationPrev = ExperimentExplorer.Emulation; //режим эмуляции  
            ExperimentExplorer.SettingsOpen();

            if (EmulationPrev != ExperimentExplorer.Emulation) //изменился режим эмуляции, выполним переподключение
            {
                // NewExperiment(); //возврат в исходное состояние
                if (ExperimentExplorer.Emulation)
                {
                    Disconnect();
                    Connect();
                }
                else
                {
                    Disconnect(); //ExperimentExplorer.IsConnected = false;   
                    connectionState.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFF1F1F6"));
                }
            }
            Address = ExperimentExplorer.Emulation ? "РЕЖИМ ЭМУЛЯЦИИ" : ExperimentExplorer.Experiment.ExperimentSettings.HardwareSettings.IP + ":"+
                                                                        ExperimentExplorer.Experiment.ExperimentSettings.HardwareSettings.Port.ToString();
            ExperimentExplorer.ConnectionStateChanged?.Invoke(ExperimentExplorer.IsConnected);
            //активность полос фильтра в зависимости от ИП и режима(СС/ДС)
            if (ExperimentExplorer.Experiment.ExperimentSettings.HardwareSettings.HardwareType == HardwareType.FSH4)
                if ((bool)rbDS.IsChecked)
                    gbRBWVBW.IsEnabled = true;
                else
                    foreach (ParametersCtrl par in ParametersList.Items)
                        par.gbFilter.IsEnabled = true;

            else //Агилент
                if ((bool)rbDS.IsChecked)
                   gbRBWVBW.IsEnabled = false;
                else
                   foreach (ParametersCtrl par in ParametersList.Items)
                   {
                       par.gbFilter.IsEnabled = false;
                      par.MsgBand.Visibility = Visibility;
                   }

            //if ((bool)rbDS.IsChecked && ExperimentExplorer.Experiment.ExperimentSettings.HardwareSettings.HardwareType == HardwareType.FSH4)
            //    gbRBWVBW.IsEnabled = true;
            //else
            //    gbRBWVBW.IsEnabled = false;
        }

    
        private void HandMode_PlusOne_Click(object sender, RoutedEventArgs e)
        {
            AddNewInterval(null, false);
        }

        private void HandMode_PlusMany_Click(object sender, RoutedEventArgs e)
        {
            int quantity = int.Parse(HandMode_Quantity.Text);
            long startFrequency = HandMode_Frequency.Value; //тактовая частота для ДС
            Ft = startFrequency;
            long shiftFrequency = startFrequency;
            //интервалы по гармоникам тактовой частоты
            if (quantity > 0)
                for (int counter = 0; counter < quantity; counter++)
                {
                    AddNewInterval(null, false, shiftFrequency);
                    shiftFrequency += startFrequency;
                }
        }

        private void CbRBW_Click(object sender, RoutedEventArgs e)
        {
            HandRBW.IsEnabled = (bool)cbRBW.IsChecked;
            CheckMsg();
        }

        private void CbVBW_Click(object sender, RoutedEventArgs e)
        {
            HandVBW.IsEnabled = (bool)cbVBW.IsChecked;
            CheckMsg();
        }
        private void CheckMsg()
        {
            if ((bool)rbDS.IsChecked && (!HandRBW.IsEnabled || !HandVBW.IsEnabled))
                MsgBand.Visibility = Visibility.Visible;
            else
                MsgBand.Visibility = Visibility.Hidden;
        }

        private void ButtonStartSignalNoise_IsEnabledChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (Parent == null)
                return;
            MainWindow mw = ((MainWindow)((Grid)((GroupBox)Parent).Parent).Parent);
            mw.unit1.ChangedButtonEnabled?.Invoke();
            mw.unit2.ChangedButtonEnabled?.Invoke();
        }
    }

}

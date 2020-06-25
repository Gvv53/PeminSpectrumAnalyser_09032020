using PeminSpectrumAnalyser.Model;
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
        public long Ft
        {
            set;
            get;
        }
        //полосы пропускания фильтра для ДС
        //public long BandWidth_DS { get; set; }
        //public long Band_DS { get; set; }
        public string Address
        {
            set => address.Content = value;
            get => address.Content.ToString();
        }

        public ExperimentExplorer ExperimentExplorer = new ExperimentExplorer();

     

        public event Action SolutionNameClear;
        public event Action ChangedButtonEnabled;
        public event Action<long,long> NextSequence; //

        public SequenceCtrl()
        {
            InitializeComponent();


            ExperimentExplorer.SequenceCtrl = this;

            //BandWidth_DS = new IntervalSettings().BandWidth;
            //Band_DS = new IntervalSettings().Band;

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
                    //par.tbSignal.Text = par.Interval.OriginalSignal[par.Interval.Markers[0]].ToString();
                    par.tbSignal.Text = par.Interval.Signal[par.Interval.Markers[0]].ToString();
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
                    //par.tbNoise.Text = par.Interval.OriginalNoise[par.Interval.Markers[0]].ToString();
                    par.tbNoise.Text = par.Interval.Noise[par.Interval.Markers[0]].ToString();
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
                
            });
            //режим СС
            ExperimentExplorer.rbSSCheckedEvent += () => 
            {
                gbDS.Visibility = Visibility.Hidden;
             
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
                gbDS.Visibility = Visibility.Visible;
                HandRBW.Value = 100000;
                HandVBW.Value = 100000;
                ParametersList.Items.Clear();
                ExperimentExplorer.Experiment.Intervals.Clear();
                //активность кнопок измерения
                buttonStartNOISE.IsEnabled = false;
                buttonStartSIGNAL.IsEnabled = false;
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
            HandRBW.HandRBWChanged += (long value) =>
            {
                RefreshRbw(value);
            };
            HandVBW.HandVBWChanged += (long value) =>
            {
                RefreshVbw(value);
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
            ExperimentExplorer.StateButtunChart += (bool state) => stbBegin.Dispatcher.Invoke(() =>
            {
                foreach (ParametersCtrl par in ExperimentExplorer.SequenceCtrl.ParametersList.Items)
                {
                    par.gbFilter.IsEnabled = state;
                    par.showChart.IsEnabled = state;
                    par.showSignalAndNoise_Copy.IsEnabled = state;
                    ExperimentExplorer.SequenceCtrl.buttonStartSIGNAL.IsEnabled = state;
                    ExperimentExplorer.SequenceCtrl.buttonStartNOISE.IsEnabled = state;
                }
            });
        }
        private void RefreshRbw(long value)
        {
            foreach (var interval in ExperimentExplorer.Experiment.Intervals)
            {
                interval.IntervalSettings.BandWidth = value;
            }
            foreach (ParametersCtrl par in ParametersList.Items)
            {
                par.Interval.IntervalSettings.BandWidth = value;
            }
        }
        private void RefreshVbw(long value)
        {
            foreach (var interval in ExperimentExplorer.Experiment.Intervals)
            {
                interval.IntervalSettings.Band = value;
            }
            foreach (ParametersCtrl par in ParametersList.Items)
            {
                par.Interval.IntervalSettings.Band = value;
            }
        }
        private void RbSS_Checked(object sender, RoutedEventArgs e)
        {
            ExperimentExplorer.rbSSChecked();
        }
        private void RbDS_Checked(object sender, RoutedEventArgs e)
        {
            ExperimentExplorer.rbDSChecked();
            RefreshRbw(HandRBW.Value);
            RefreshVbw(HandVBW.Value);
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

            newInterval.IntervalSettings.PointsQuantity = ExperimentExplorer.Experiment.ExperimentSettings.HardwareSettings.PointsQuantity;

            newInterval.IntervalSettings.HandCenterFrequency = frequencyCenter;

            ExperimentExplorer.Experiment.Intervals.Add(newInterval);

            ParametersCtrl intervalParametersCtrl = new ParametersCtrl(ExperimentExplorer.Experiment.ExperimentSettings.HardwareSettings);
           
            //if ((bool)rbDS.IsChecked)
                //newInterval.IntervalSettings.Span = (long)Ft;   //для ДС span = тактовой частоте
            
            intervalParametersCtrl.IsAutoStyle = newInterval.IntervalSettings.isAuto;

            //Маркеры определяются при рассчёте
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

        private void BeforeMeasuring()
        {
            //обновим ширину пропускания фильтров(на тот случай, если пользователь забыл нажать Enter)
            if ((bool)rbSS.IsChecked) //CC
            {
                foreach (ParametersCtrl par in ParametersList.Items)
                    par.UITo(par.Interval);
            }
            else  //ДС
            {
                RefreshRbw(HandRBW.Value);
                RefreshVbw(HandVBW.Value);
            }
        }
        public void StartNoiseScan()
        {
            BeforeMeasuring();
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
            BeforeMeasuring();
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
            string hardvareType;
            string hardvareTypePrev = ExperimentExplorer.Experiment.ExperimentSettings.HardwareSettings.HardwareType.ToString();
            bool EmulationPrev = ExperimentExplorer.Emulation; //режим эмуляции  
            ExperimentExplorer.SettingsOpen();
            hardvareType = ExperimentExplorer.Experiment.ExperimentSettings.HardwareSettings.HardwareType.ToString();
            if (hardvareType != hardvareTypePrev)    //если поменялось оборудование, надо пересчитать точки
            {
                foreach (ParametersCtrl par in ParametersList.Items)
                    par.createPoints.IsEnabled = true;
                foreach (Interval inter in ExperimentExplorer.Experiment.Intervals)
                    inter.IntervalSettings.PointsQuantity = ExperimentExplorer.Experiment.ExperimentSettings.HardwareSettings.PointsQuantity;
            }
            if (EmulationPrev != ExperimentExplorer.Emulation && ExperimentExplorer.Emulation) //режим эмуляции, выполним переподключение
            {
                // NewExperiment(); //возврат в исходное состояние
                //f (ExperimentExplorer.Emulation)
                //{
                    Disconnect();
                    Connect();
            }
            else
            {
                Disconnect(); //ExperimentExplorer.IsConnected = false;   
                connectionState.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFF1F1F6"));
            }
          
            Address = ExperimentExplorer.Emulation ? "РЕЖИМ ЭМУЛЯЦИИ" : ExperimentExplorer.Experiment.ExperimentSettings.HardwareSettings.IP + ":"+
                                                                        ExperimentExplorer.Experiment.ExperimentSettings.HardwareSettings.Port.ToString();
            //ExperimentExplorer.ConnectionStateChanged?.Invoke(ExperimentExplorer.IsConnected);
            //активность полос фильтра в зависимости от ИП и режима(СС/ДС)
          // if (ExperimentExplorer.Experiment.ExperimentSettings.HardwareSettings.HardwareType == HardwareType.FSH4)
                //if ((bool)rbDS.IsChecked)
                //    gbRBWVBW.IsEnabled = true;
                //else
                //    foreach (ParametersCtrl par in ParametersList.Items)
                //        par.gbFilter.IsEnabled = true;

            //else //Агилент
            //    if ((bool)rbDS.IsChecked) //ДС
            //       gbRBWVBW.IsEnabled = false;
            //    else   //СС
            //       foreach (ParametersCtrl par in ParametersList.Items)
            //       {
            //        par.gbFilter.IsEnabled = true;// false;
            //          par.MsgBand.Visibility = Visibility;
            //       }           
        }
  
        private void HandMode_PlusOne_Click(object sender, RoutedEventArgs e)
        {
            AddNewInterval(null, false);
        }

        private void HandMode_PlusMany_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Выбор приборов в 'стаканах' соответствует реально подключенным приборам?","",MessageBoxButton.YesNo) != MessageBoxResult.Yes)
            {
                return;
            }

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
                    if ((bool)cbMove.IsChecked && FrequencyMax.Value != 0 && shiftFrequency > FrequencyMax.Value)
                    {
                        NextSequence?.Invoke(Ft, shiftFrequency);  // интервалы, перенесённые в другой стакан
                        counter = quantity;
                    }
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

        private void CbMove_Checked(object sender, RoutedEventArgs e)
        {
            FrequencyMax.IsEnabled = (bool)cbMove.IsChecked;
        }
    }

}

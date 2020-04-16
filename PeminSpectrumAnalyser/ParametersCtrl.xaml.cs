using IOMeasurementData;
using PeminSpectrumData;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Input;
using PeminSpectrumAnalyser.Model;

namespace PeminSpectrumAnalyser
{
    /// <summary>
    /// Interaction logic for ParametersCtrl.xaml
    /// Контрол с параметрами измерений для сплошного спектра
    /// </summary>
    /// 
    public partial class ParametersCtrl : UserControl
    {
        IOMeasurementData.HardwareSettings _HardwareSettings;

        bool _IsAutoStyle = true;
        public bool IsAutoStyle
        {
            get => _IsAutoStyle;
            set
            {
                _IsAutoStyle = value;
                if (_IsAutoStyle)
                {
                    GridHand.Visibility = Visibility.Collapsed;
                    GridAuto.Visibility = Visibility.Visible;
                }
                else
                {
                    GridHand.Visibility = Visibility.Visible;
                    GridAuto.Visibility = Visibility.Collapsed;
                }
            }
        }

        public ParametersCtrl(IOMeasurementData.HardwareSettings hardwareSettings)
        {
            InitializeComponent();

            _HardwareSettings = hardwareSettings;

            DeltaShiftStartFrequency.FrequencyCtrlChanged += () => { UITo(Interval); };

            StartFrequency.FrequencyCtrlChanged += () => { UITo(Interval); };
            StopFrequency.FrequencyCtrlChanged += () => { UITo(Interval); };
            Band.FrequencyCtrlChanged += () => { UITo(Interval); };
            BandWidth.FrequencyCtrlChanged += () => { UITo(Interval); };
            //centerFrequency.FrequencyCtrlChanged += () => { UITo(Interval); };
            //Span.FrequencyCtrlChanged += () => { UITo(Interval); };
            //StepFrequency.FrequencyCtrlChanged += () => { UITo(Interval); };
            InnerStepFrequency.FrequencyCtrlChanged += () => { UITo(Interval); };
            StartFrequency.ParameterCtrChanged += () => { createPoints.IsEnabled = true; };
            StopFrequency.ParameterCtrChanged += () => { createPoints.IsEnabled = true; };
            InnerStepFrequency.ParameterCtrChanged += () => { createPoints.IsEnabled = true; };
            BandWidth.ParameterCtrChanged += () => { createPoints.IsEnabled = true; };
            Band.ParameterCtrChanged += () => { createPoints.IsEnabled = true; };
        }

        bool _IsSelected = false;
        public bool IsSelected
        {
            get => _IsSelected;

            set
            {
                _IsSelected = value;

                if (value)
                {
                    BorderBrush = new SolidColorBrush(new Color() { A = 0xFF, R = 0xFF, G = 0x4B, B = 0x00 });
                }
                else
                {
                    BorderBrush = null;
                }
            }
        }

        public Interval Interval;

        bool DisableUITo = false;
        
        //актуализация значений из UI в объект класса Interval  
        public void UITo(Interval linkToInterval)
        {
            if (linkToInterval == null)
                return;

            if (DisableUITo)
                return;

            linkToInterval.isActive = (bool)this.isActiveCheckBox.IsChecked;
            linkToInterval.isModify = (bool)this.cbModify.IsChecked;

            linkToInterval.IntervalSettings.FrequencyStart = this.StartFrequency.Value;
            linkToInterval.IntervalSettings.FrequencyStop = this.StopFrequency.Value;
          //  linkToInterval.IntervalSettings.FrequencyStep = long.Parse(this.tbStepFrequency.Value);//не используется
            linkToInterval.IntervalSettings.FrequencyInnerStep = this.InnerStepFrequency.Value;

            if (linkToInterval.IntervalSettings.isAuto) // CC
            { 
                linkToInterval.IntervalSettings.BandWidth = BandWidth.Value;
                linkToInterval.IntervalSettings.Band = Band.Value;
            }
            else //DS
            {
                    //linkToInterval.GetExperimentExplorer().SequenceCtrl.BandWidth_DS = linkToInterval.GetExperimentExplorer().SequenceCtrl.HandRBW.Value;
                    //linkToInterval.GetExperimentExplorer().SequenceCtrl.Band_DS = linkToInterval.GetExperimentExplorer().SequenceCtrl.HandVBW.Value;
            }
            // linkToInterval.IntervalSettings.Span = long.Parse(this.tbSpan.Text);//поле только для чтения

            linkToInterval.IntervalSettings.MaxNoise = Converters.DoubleValueFromUI(this.maxLimitNoise.Text);
            linkToInterval.IntervalSettings.MinNoise = Converters.DoubleValueFromUI(this.minLimitNoise.Text);
            linkToInterval.IntervalSettings.MaxSignal = Converters.DoubleValueFromUI(this.maxLimitSignal.Text);
            linkToInterval.IntervalSettings.MinSignal = Converters.DoubleValueFromUI(this.minLimitSignal.Text);
            linkToInterval.IntervalSettings.ShiftNoise = Converters.DoubleValueFromUI(this.ShiftNoise.Text);
            linkToInterval.IntervalSettings.ShiftSignal = Converters.DoubleValueFromUI(this.ShiftSignal.Text);
            linkToInterval.IntervalSettings.DeltaShiftFrequency = this.DeltaShiftStartFrequency.Value;
            linkToInterval.IntervalSettings.Message1BeforeStartMeasuring = this.messageText1.Text;
            linkToInterval.IntervalSettings.Message2BeforeStartMeasuring = this.messageText2.Text;
            linkToInterval.IntervalSettings.Message3BeforeStartMeasuring = this.messageText3.Text;
            linkToInterval.IntervalSettings.EnableMessage1BeforeStartMeasuring = (bool)this.message1Enable.IsChecked;
            linkToInterval.IntervalSettings.EnableMessage2BeforeStartMeasuring = (bool)this.message2Enable.IsChecked;
            linkToInterval.IntervalSettings.EnableMessage3BeforeStartMeasuring = (bool)this.message3Enable.IsChecked;

            linkToInterval.IntervalSettings.EnableMessage1BeforeStartMeasuringForNoise = (bool)this.message1EnableForNoise.IsChecked;
            linkToInterval.IntervalSettings.EnableMessage2BeforeStartMeasuringForNoise = (bool)this.message2EnableForNoise.IsChecked;
            linkToInterval.IntervalSettings.EnableMessage3BeforeStartMeasuringForNoise = (bool)this.message3EnableForNoise.IsChecked;


            linkToInterval.IntervalSettings.SpecialSignalNoiseShift = Converters.DoubleValueFromUI(this.SpecialDelta.Text);

            linkToInterval.IntervalSettings.HandCenterFrequency = this.HandCenterFrequency.Value; 
            linkToInterval.IntervalSettings.isAuto = this.IsAutoStyle;


        }

        public void UIFrom(Interval linkToInterval)
        {
            try
            {
                DisableUITo = true;

                Interval = linkToInterval;

                this.StartFrequency.Value = linkToInterval.IntervalSettings.FrequencyStart;
                this.StopFrequency.Value = linkToInterval.IntervalSettings.FrequencyStop;
               // this.StepFrequency.Value = linkToInterval.IntervalSettings.FrequencyStep; // не используется
                this.InnerStepFrequency.Value = linkToInterval.IntervalSettings.FrequencyInnerStep;

                if (linkToInterval.IntervalSettings.isAuto) // CC
                {
                    BandWidth.Value = linkToInterval.IntervalSettings.BandWidth;
                    this.tbSpan.Text = ((double)linkToInterval.IntervalSettings.Span / 1000000).ToString();//рассчитывается, пользователем не меняется
                    Band.Value = linkToInterval.IntervalSettings.Band;
                }
                else //для ДС полосы пропускания находятся на уровне SequenceCtr
                {                   
                        //linkToInterval.GetExperimentExplorer().SequenceCtrl.HandRBW.Value = linkToInterval.GetExperimentExplorer().SequenceCtrl.BandWidth_DS;                      
                        //linkToInterval.GetExperimentExplorer().SequenceCtrl.HandVBW.Value = linkToInterval.GetExperimentExplorer().SequenceCtrl.Band_DS;                    
                }
                this.maxLimitNoise.Text = linkToInterval.IntervalSettings.MaxNoise.ToString();
                this.minLimitNoise.Text = linkToInterval.IntervalSettings.MinNoise.ToString();
                this.maxLimitSignal.Text = linkToInterval.IntervalSettings.MaxSignal.ToString();
                this.minLimitSignal.Text = linkToInterval.IntervalSettings.MinSignal.ToString();
                this.ShiftNoise.Text = linkToInterval.IntervalSettings.ShiftNoise.ToString();
                this.ShiftSignal.Text = linkToInterval.IntervalSettings.ShiftSignal.ToString();
                this.DeltaShiftStartFrequency.Value = linkToInterval.IntervalSettings.DeltaShiftFrequency;
                this.messageText1.Text = linkToInterval.IntervalSettings.Message1BeforeStartMeasuring;
                this.messageText2.Text = linkToInterval.IntervalSettings.Message2BeforeStartMeasuring;
                this.messageText3.Text = linkToInterval.IntervalSettings.Message3BeforeStartMeasuring;
                this.message1Enable.IsChecked = linkToInterval.IntervalSettings.EnableMessage1BeforeStartMeasuring;
                this.message2Enable.IsChecked = linkToInterval.IntervalSettings.EnableMessage2BeforeStartMeasuring;
                this.message3Enable.IsChecked = linkToInterval.IntervalSettings.EnableMessage3BeforeStartMeasuring;

                this.message1EnableForNoise.IsChecked = linkToInterval.IntervalSettings.EnableMessage1BeforeStartMeasuringForNoise;
                this.message2EnableForNoise.IsChecked = linkToInterval.IntervalSettings.EnableMessage2BeforeStartMeasuringForNoise;
                this.message3EnableForNoise.IsChecked = linkToInterval.IntervalSettings.EnableMessage3BeforeStartMeasuringForNoise;


                

                this.SpecialDelta.Text = linkToInterval.IntervalSettings.SpecialSignalNoiseShift.ToString();
                this.HandCenterFrequency.Value = linkToInterval.IntervalSettings.HandCenterFrequency; //гармоники для ДС
                this.tbCenterFrequency.Text = ((double)linkToInterval.CenterFrequency / 1000000).ToString(); //центр частотного диапазона для СС
                this.IsAutoStyle = linkToInterval.IntervalSettings.isAuto;
                this.tbPoints.Text = linkToInterval.Markers.Count.ToString();
            }
            finally
            {
                DisableUITo = false;
            }
        }

        public Action<ParametersCtrl> GoToStartPoint;
        public Action<ParametersCtrl> CreatePoints;
        public Action<ParametersCtrl> CreatePointsHand;
        public Action<ParametersCtrl> ShowSignalAndNoise;
        public Action<ParametersCtrl> MoveStartPointToMarker;
        public Action<ParametersCtrl> Delete;
        //кнопка РАСЧИТАТЬ для интервалов обоих спектров.
        private void CreatePoints_Click(object sender, RoutedEventArgs e)
        {
            Interval.ClearAll();

            UITo(Interval);

            Interval.BuildAutomaticPoints();
            

            UIFrom(Interval);
            if (Interval.IntervalSettings.isAuto) //СС
            {
                createPoints.IsEnabled = false;   //после выполнения расчёта точек измерения, кнопка деактивируется, кнопки выполнения измерений активируются
                Model.ExperimentExplorer EE = Interval.GetExperimentExplorer();
                bool enabled = EE.IsConnected && Interval.Markers.Count > 0;
                EE.SequenceCtrl.buttonStartNOISE.IsEnabled = enabled;
                EE.SequenceCtrl.buttonStartSIGNAL.IsEnabled = enabled;
            }
        }


        private void GoToBegin_Click(object sender, RoutedEventArgs e)
        {
            UITo(Interval);
            Interval.GoToBegin();
            UIFrom(Interval);
        }

        private void GoToBegin1_Click(object sender, RoutedEventArgs e)
        {
            UITo(Interval);
            Interval.GoToBegin1();
            UIFrom(Interval);
        }

        private void GoToEnd1_Click(object sender, RoutedEventArgs e)
        {
            UITo(Interval);
            Interval.GoToEnd1();
            UIFrom(Interval);
        }

        private void GoToEnd_Click(object sender, RoutedEventArgs e)
        {
            UITo(Interval);
            Interval.GoToEnd();
            UIFrom(Interval);
        }

      
        private void ShowSignalAndNoise_Click(object sender, RoutedEventArgs e)
        {
            Interval.GetExperimentExplorer().ShowSignalAndNoise(Interval, Interval.Signal, Interval.Noise,
                (signalShift) => {
                    this.ShiftSignal.Text = signalShift.ToString();
                },
                (noiseShift) => {
                    this.ShiftNoise.Text = noiseShift.ToString();
                },
                (XShift) =>
                {
                    this.HandCenterFrequency.Value = (long)XShift;
                });
        }

        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            Delete?.Invoke(this);
        }

        private void Accept_Click(object sender, RoutedEventArgs e)
        {
            UITo(Interval);
        }

        private void ComputerButton_Click(object sender, RoutedEventArgs e)
        {
            UITo(Interval);
            Interval.Computer();
        }

        private void RestoreButton_Click(object sender, RoutedEventArgs e)
        {
            Interval.Restore();
        }

        private void ShowSignalAndNoise_Signals_Click(object sender, RoutedEventArgs e)
        {
            Interval.GetExperimentExplorer().ShowSignalAndNoise(Interval, Interval.Signal, Interval.OriginalSignal, null, null);
        }

        private void ShowSignalAndNoise_Noises_Click(object sender, RoutedEventArgs e)
        {
            Interval.GetExperimentExplorer().ShowSignalAndNoise(Interval, Interval.Noise, Interval.OriginalNoise, null, null);
        }

        private void UserControl_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            IsSelected = true;
        }

        private void UserControl_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            IsSelected = false;
        }

        private void message1Enable_Checked(object sender, RoutedEventArgs e)
        {
            UITo(Interval);
        }

        private void message3EnableForNoise_Unchecked(object sender, RoutedEventArgs e)
        {
            UITo(Interval);
        }

        private void messageText1_TextChanged(object sender, TextChangedEventArgs e)
        {
            UITo(Interval);
        }

        private void minLimitNoise_TextChanged(object sender, TextChangedEventArgs e)
        {
            UITo(Interval);
        }

        private void isActiveCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            UITo(Interval);
        }

        private void isActiveCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            UITo(Interval);
        }

        private void cbModify_Checked(object sender, RoutedEventArgs e)
        {
            UITo(Interval);
        }
        //private void cbModify_unChecked(object sender, RoutedEventArgs e)
        //{
        //    UITo(Interval);
        //}

        //private void TextBox_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        //{  

        //    if (e.Key == Key.Enter)   //завершён ввод
        //    {
        //        TextBox tb;
        //        long Value;
        //        try
        //        {
        //            tb = (TextBox)sender;
        //            Value = long.Parse(tb.Text);
        //        }
        //        catch (Exception ee)
        //        {
        //            MessageBox.Show("Ошибка преобразования значения полосы пропускания. " + Environment.NewLine + ee.Message);
        //            return;
        //        }
        //        string name = tb.Name;
        //        switch (name)
        //        {
        //            case "tbBandWidth":
        //                this.Interval.IntervalSettings.BandWidth = Value;
        //                break;
        //            case "tbBand":
        //                this.Interval.IntervalSettings.Band = Value;
        //                break;
        //            case "tbBandWidth_DS":
        //                this.Interval.IntervalSettings.BandWidth = Value;
        //                break;
        //            case "tbBand_DS":
        //                this.Interval.IntervalSettings.Band = Value;
        //                break;
        //        }
        //    }
        //}

        private void CreatePoints_IsEnabledChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            ExperimentExplorer EE = Interval.GetExperimentExplorer();
            EE.ConnectionStateChanged?.Invoke(EE.IsConnected);
        }
        private void cbBandWidth_Click(object sender, RoutedEventArgs e)
        {
            BandWidth.IsEnabled = (bool)cbBandWidth.IsChecked;
            CheckMsg();
        }

        private void cbBand_Click(object sender, RoutedEventArgs e)
        {
            Band.IsEnabled = (bool)cbBand.IsChecked;
            CheckMsg();
        }
        private void CheckMsg()
        {
            if ((!BandWidth.IsEnabled || !Band.IsEnabled))
                MsgBand.Visibility = Visibility.Visible;
            else
                MsgBand.Visibility = Visibility.Hidden;
        }
    }
}

using PeminSpectrumAnalyser.Model;
using PeminSpectrumData;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace PeminSpectrumAnalyser
{
    /// <summary>
    /// Interaction logic for SequenceCtrl.xaml
    /// </summary>
    public partial class SequenceCtrl : UserControl
    {
        public string Address
        {
            set => address.Content = value;
            get => address.Content.ToString();
        }

        public ExperimentExplorer ExperimentExplorer = new ExperimentExplorer();



        public SequenceCtrl()
        {
            InitializeComponent();

            ExperimentExplorer.SequenceCtrl = this;

            ExperimentExplorer.IntervalChangeEvent += (quantity, count) => intervals.Dispatcher.Invoke(() => { intervals.Content = quantity + " / " + count; });
            ExperimentExplorer.PointChangeEvent += (quantity, count) => points.Dispatcher.Invoke(() => { points.Content = quantity + " / " + count; });
            ExperimentExplorer.ScanProcessEvent += (description) => scanProcess.Dispatcher.Invoke(() => { scanProcess.Content = description; });
            ExperimentExplorer.SignalReadyEvent += () => SignalStateLabel.Dispatcher.Invoke(()=> 
            { 
                SignalStateLabel.Content = "СИГНАЛ СНЯТ";

            });
            ExperimentExplorer.NoiseReadyEvent += () => NoiseStateLabel.Dispatcher.Invoke(() => 
            { 
                NoiseStateLabel.Content = "ШУМ СНЯТ";
            });
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
            ExperimentExplorer.Experiment.ExperimentSettings = oldExperimentSettings;
            ParametersList.Items.Clear();
            Address = ExperimentExplorer.Experiment.ExperimentSettings.HardwareSettings.IP;

            return ExperimentExplorer.Experiment;
        }

        public void AddressRefresh() => Address = ExperimentExplorer.Experiment.ExperimentSettings.HardwareSettings.IP;


        private void ButtonLoad_Click(object sender, RoutedEventArgs e)
        {
            ExperimentExplorer.LoadExperiment();
            Address = ExperimentExplorer.Experiment.ExperimentSettings.HardwareSettings.IP;
        }

        private void ButtonSave_Click(object sender, RoutedEventArgs e)
        {
            ExperimentExplorer.SaveExperiment();
        }

        private void ButtonPlus_Click(object sender, RoutedEventArgs e) => AddNewInterval();


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

        public void AddNewInterval(Interval interval = null, bool isAutoStyle = true, long frequencyCenter = 1000000)
        {
            Interval newInterval = interval ?? new Interval();

            newInterval.SetExperimentExplorer(ExperimentExplorer);

            newInterval.IntervalSettings.isAuto = isAutoStyle;

            newInterval.IntervalSettings.HandCenterFrequency = frequencyCenter;

            ExperimentExplorer.Experiment.Intervals.Add(newInterval);

            ParametersCtrl intervalParametersCtrl = new ParametersCtrl(ExperimentExplorer.Experiment.ExperimentSettings.HardwareSettings);

            intervalParametersCtrl.IsAutoStyle = newInterval.IntervalSettings.isAuto;

            if (newInterval.IntervalSettings.isAuto)
                newInterval.Markers.Add(((newInterval.IntervalSettings.PointsQuantity - 1) / 2) + 1);


            intervalParametersCtrl.Interval = newInterval;

            newInterval.IntervalSettings.LinkToVisualControl = intervalParametersCtrl;

            intervalParametersCtrl.UIFrom(newInterval);

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
            ExperimentExplorer.SettingsOpen();
            Address = ExperimentExplorer.Experiment.ExperimentSettings.HardwareSettings.IP;
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void HandMode_PlusOne_Click(object sender, RoutedEventArgs e)
        {
            AddNewInterval(null, false);
        }

        private void HandMode_PlusMany_Click(object sender, RoutedEventArgs e)
        {
            int quantity = int.Parse(HandMode_Quantity.Text);
            long startFrequency = HandMode_Frequency.Value;
            long shiftFrequency = startFrequency;

            if (quantity > 0)
                for(int counter = 0; counter < quantity; counter++)
                {
                    AddNewInterval(null, false, shiftFrequency);
                    shiftFrequency += startFrequency;
                }
        }
    }

}

using IOMeasurementData;
using PeminSpectrumData;
using System;
using System.Windows;

namespace PeminSpectrumAnalyser
{
    /// <summary>
    /// Interaction logic for SettingsWindow.xaml
    /// </summary>
    public partial class SettingsWindow : Window
    {
        ExperimentSettings _Settings = new ExperimentSettings();
        public ExperimentSettings Settings
        {
            get => _Settings;

            set
            {
                _Settings = value;
                SettingsToUI();
            }
        }

        public SettingsWindow()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            SettingsFromUI();
            Close();
        }


        public void SettingsFromUI()
        {
            Settings.HardwareSettings.HardwareType = (HardwareType)HardwareComboBox.SelectedIndex;
            switch (Settings.HardwareSettings.HardwareType)
            {
                case HardwareType.Agilent934хC:
                    Settings.HardwareSettings.PointsQuantity = 461;
                    break;
                case HardwareType.AGILENT90х0:
                    Settings.HardwareSettings.PointsQuantity = 1001;
                    break;
                case HardwareType.FSH4:
                    Settings.HardwareSettings.PointsQuantity = 631;
                    break;
                case HardwareType.FSH18:
                    Settings.HardwareSettings.PointsQuantity = 501;
                    break;

                case HardwareType.AGILENT9915x:
                    Settings.HardwareSettings.PointsQuantity = 401;
                    break;
                default:
                    Settings.HardwareSettings.PointsQuantity = 631;
                    break;

            }
            Settings.HardwareSettings.HardwareDescription = HardwareComboBox.Text;

            Settings.HardwareSettings.IP =  Address.Text;
            Func<int> conv = () => { try { return int.Parse(Port.Text); }
                catch (Exception e){ MessageBox.Show("Неверно задан номер порта"); return 0; } };
            Settings.HardwareSettings.Port = conv();


            Settings.ExperimentPath = LeftPanelPath.Text;
            Settings.HardwareSettings.NoiseTraceDetector = cbNoiseDetector.Text;
            Settings.HardwareSettings.SignalTraceDetector = cbSignalDetector.Text;
            Settings.HardwareSettings.SignalTraceType = cbSignalTraceType.Text;
            Settings.HardwareSettings.NoiseTraceType = cbNoiseTraceType.Text;

            if (Int32.TryParse(tbSignalAttenuation.Text, out int buffer))
                Settings.HardwareSettings.SignalAttenuation = buffer;
            else
                MessageBox.Show("Неверно задано значение ослабления шума");

            if (Int32.TryParse(tbNoiseAttenuation.Text, out buffer))
                Settings.HardwareSettings.NoiseAttenuation = buffer;
            else
                MessageBox.Show("Неверно задано значение ослабления сигнала ");

            if (Int32.TryParse(tbCountNoiseTraceMode.Text, out buffer))
                Settings.HardwareSettings.CountNoiseTraceMode = buffer;
            else
                MessageBox.Show("Неверно задано количество измерений шума TRACE MODE ");

            if (Int32.TryParse(tbCountSignalTraceMode.Text, out buffer))
                Settings.HardwareSettings.CountSignalTraceMode = buffer;
            else
                MessageBox.Show("Неверно задано количество измерений сигнала TRACE MODE ");

            if (double.TryParse(tbSignalAttenuation.Text, out double dbuffer))
                Settings.HardwareSettings.SignalAttenuation = (long)dbuffer;
            else
                MessageBox.Show("Неверно задан параметр SignalAttenuation");

            if (double.TryParse(tbNoiseAttenuation.Text, out dbuffer))
                Settings.HardwareSettings.NoiseAttenuation = (long)dbuffer;
            else
                MessageBox.Show("Неверно задан параметр NoiseAttenuation");

            if (Int32.TryParse(tbCountNoiseTraceMode.Text, out buffer))
                Settings.HardwareSettings.CountNoiseTraceMode = buffer;
            else
                MessageBox.Show("Неверно задано количество измерений шума TRACE MODE ");

            if (Int32.TryParse(tbCountSignalTraceMode.Text, out buffer))
                Settings.HardwareSettings.CountSignalTraceMode = buffer;
            else
                MessageBox.Show("Неверно задано количество измерений сигнала TRACE MODE ");

            Settings.AverageTypeForNoiseOff = false;
            Settings.AverageTypeForNoiseMaximum = false;
            Settings.AverageTypeForNoiseMiddle = false;
            Settings.AverageTypeForNoiseMinimum = false;

            Settings.AverageTypeForSignalOff = false;
            Settings.AverageTypeForSignalMaximum = false;
            Settings.AverageTypeForSignalMiddle = false;
            Settings.AverageTypeForSignalMinimum = false;

            Settings.Emulation = (bool)Emulation.IsChecked;
            Settings.HardwareSettings.Preamp = (bool)cbPreamp.IsChecked;

        }

        public void SettingsToUI()
        {
            HardwareComboBox.SelectedIndex = (int)Settings.HardwareSettings.HardwareType;

            Address.Text = Settings.HardwareSettings.IP;

            Port.Text = (int)Settings.HardwareSettings.HardwareType == 0 ? "5555" : "5025";// Settings.HardwareSettings.Port.ToString();

            LeftPanelPath.Text = Settings.ExperimentPath;
            cbNoiseDetector.Text = Settings.HardwareSettings.NoiseTraceDetector;
            cbSignalDetector.Text = Settings.HardwareSettings.SignalTraceDetector;
            cbSignalTraceType.Text = Settings.HardwareSettings.SignalTraceType;
            cbNoiseTraceType.Text = Settings.HardwareSettings.NoiseTraceType;

            tbSignalAttenuation.Text = Settings.HardwareSettings.SignalAttenuation.ToString();
            tbNoiseAttenuation.Text = Settings.HardwareSettings.NoiseAttenuation.ToString();
            tbCountNoiseTraceMode.Text = Settings.HardwareSettings.CountNoiseTraceMode.ToString();
            tbCountSignalTraceMode.Text = Settings.HardwareSettings.CountSignalTraceMode.ToString();
            tbSignalAttenuation.Text = Settings.HardwareSettings.SignalAttenuation.ToString();
            tbNoiseAttenuation.Text = Settings.HardwareSettings.NoiseAttenuation.ToString();

            Emulation.IsChecked = Settings.Emulation;
            cbPreamp.IsChecked = Settings.HardwareSettings.Preamp;
        }

        private void HardwareComboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (Port == null)
                return;
            if (HardwareComboBox.SelectedIndex == 0)
            {   
                 Port.Text = "5555";
            }
            else
            {
                 //Settings.HardwareSettings.Port = 5025;
                 Port.Text = "5025";
            }
        }
    }
}

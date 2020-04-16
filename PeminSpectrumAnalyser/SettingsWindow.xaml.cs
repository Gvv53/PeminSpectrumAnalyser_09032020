﻿using IOMeasurementData;
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

            Settings.HardwareSettings.IP =  Address.Text;
            Func<int> conv = () => { try { return int.Parse(Port.Text); }
                catch (Exception e){ MessageBox.Show("Неверно задан номер порта"); return 0; } };
            Settings.HardwareSettings.Port = conv();


            Settings.ExperimentPath = LeftPanelPath.Text;
            Settings.HardwareSettings.TraceModeForNoise = cbNoiseDetector.Text;
            Settings.HardwareSettings.TraceModeForSignal = cbSignalDetector.Text;
            Settings.HardwareSettings.SignalTraceType = cbSignalTraceType.Text;
            Settings.HardwareSettings.NoiseTraceType = cbNoiseTraceType.Text;

            if (double.TryParse(DbMKvShift.Text, out double dbuffer))
                Settings.HardwareSettings.DbMkvShift = dbuffer;
            else
                MessageBox.Show("Неверно задан параметр DbMKvShift");


            if (double.TryParse(CommonShift.Text, out dbuffer))
                Settings.CommonShift = dbuffer;
            else
                MessageBox.Show("Неверно задан параметр CommonShift");

            if (Int32.TryParse(MiddleCounterNoise.Text, out int buffer))
                Settings.MeasurementCountForNoise = buffer;
            else
                MessageBox.Show("Неверно задано количество усреднений для шума ");


            if (Int32.TryParse(MiddleCounterSignal.Text, out buffer))
                Settings.MeasurementCountForSignal = buffer;
            else
                MessageBox.Show("Неверно задано количество усреднений для шума ");


            try
            {
                Settings.MeasurementCountForNoise = int.Parse(MiddleCounterNoise.Text);
            }
            finally
            {

            }

            try
            {
                Settings.MeasurementCountForSignal = int.Parse(MiddleCounterSignal.Text);
            }
            finally
            {

            }

            Settings.AverageTypeForNoiseOff = (bool)MiddleSwitchOffForNoise.IsChecked;
            Settings.AverageTypeForNoiseMaximum = (bool)MiddleMaxValueForNoise.IsChecked;
            Settings.AverageTypeForNoiseMiddle = (bool)MiddleMiddleForNoise.IsChecked;
            Settings.AverageTypeForNoiseMinimum = (bool)MiddleMinValueForNoise.IsChecked;

            Settings.AverageTypeForSignalOff = (bool)MiddleSwitchOffForSignal.IsChecked;
            Settings.AverageTypeForSignalMaximum = (bool)MiddleMaxValueForSignal.IsChecked;
            Settings.AverageTypeForSignalMiddle = (bool)MiddleMiddleForSignal.IsChecked;
            Settings.AverageTypeForSignalMinimum = (bool)MiddleMinValueForSignal.IsChecked;

            Settings.Emulation = (bool)Emulation.IsChecked;
        }

        public void SettingsToUI()
        {
            HardwareComboBox.SelectedIndex = (int)Settings.HardwareSettings.HardwareType;

            Address.Text = Settings.HardwareSettings.IP;

            Port.Text = (int)Settings.HardwareSettings.HardwareType == 0 ? "5555" : "5025";// Settings.HardwareSettings.Port.ToString();

            LeftPanelPath.Text = Settings.ExperimentPath;
            cbNoiseDetector.Text = Settings.HardwareSettings.TraceModeForNoise;
            cbSignalDetector.Text = Settings.HardwareSettings.TraceModeForSignal;
            cbSignalTraceType.Text = Settings.HardwareSettings.SignalTraceType;
            cbNoiseTraceType.Text = Settings.HardwareSettings.NoiseTraceType;

            DbMKvShift.Text = Settings.HardwareSettings.DbMkvShift.ToString();
            CommonShift.Text = Settings.CommonShift.ToString();
            MiddleCounterNoise.Text = Settings.MeasurementCountForNoise.ToString();
            MiddleCounterSignal.Text = Settings.MeasurementCountForSignal.ToString();

            MiddleSwitchOffForNoise.IsChecked = Settings.AverageTypeForNoiseOff;
            MiddleMaxValueForNoise.IsChecked = Settings.AverageTypeForNoiseMaximum;
            MiddleMiddleForNoise.IsChecked = Settings.AverageTypeForNoiseMiddle;
            MiddleMinValueForNoise.IsChecked = Settings.AverageTypeForNoiseMinimum;

            MiddleSwitchOffForSignal.IsChecked = Settings.AverageTypeForSignalOff;
            MiddleMaxValueForSignal.IsChecked = Settings.AverageTypeForSignalMaximum;
            MiddleMiddleForSignal.IsChecked = Settings.AverageTypeForSignalMiddle;
            MiddleMinValueForSignal.IsChecked = Settings.AverageTypeForSignalMinimum;

            Emulation.IsChecked = Settings.Emulation;
        }

        private void HardwareComboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (HardwareComboBox.SelectedIndex == 0)
            {   
                 //Settings.HardwareSettings.Port = 5555;
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

﻿using System;
using System.Collections.ObjectModel;
using System.Windows.Controls;
using System.Windows.Input;

namespace PeminSpectrumAnalyser
{
    /// <summary>
    /// Interaction logic for FrequencyCtrl.xaml
    /// </summary>
    public partial class FrequencyCtrl : UserControl
    {
        public const string MHz = "MHz";
        public const string KHz = "KHz";
        public const string Hz = "Hz";

        public FrequencyCtrl()
        {
            InitializeComponent();

            comboBox1.ItemsSource = new ObservableCollection<string>() { MHz, KHz, Hz };
            comboBox1.Text = MHz;

        }

        long _Value = 0;


        public long Value
        {
            get
            {
                return _Value;
            }

            set
            {
                _Value = value;
                
                if (comboBox1.Text == MHz) DataTextBox.Text = Converters.ValueToUI(value, 1000000);
                if (comboBox1.Text == KHz) DataTextBox.Text = Converters.ValueToUI(value, 1000);
                if (comboBox1.Text == Hz)  DataTextBox.Text = Converters.ValueToUI(value, 1);
            }
        }

        public string DataType
        {
            get => comboBox1.Text;

            set => comboBox1.Text = value;
        }


        bool _DontChange = false;

        private void DataTextBox_TextChanged_1(object sender, TextChangedEventArgs e)
        {
            if (_DontChange)
                return;

            long result = 0;

            if (comboBox1.Text == MHz) result = Converters.ValueFromUI(DataTextBox.Text, 1000000);
            if (comboBox1.Text == KHz) result = Converters.ValueFromUI(DataTextBox.Text, 1000);
            if (comboBox1.Text == Hz) result = Converters.ValueFromUI(DataTextBox.Text, 1);

            _Value = result;

            
        }

        public event Action FrequencyCtrlChanged;
        public event Action ParameterCtrChanged; //изменились параметры для расчёта количества и фиксирования точек измерения

        private void ComboBox1_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (((ComboBox)sender).SelectedValue.ToString() == MHz)
            {
                _DontChange = true;
                try
                {
                    DataTextBox.Text = Converters.ValueToUI(_Value, 1000000);
                }
                finally
                {
                    _DontChange = false;
                }
            } 
            if (((ComboBox)sender).SelectedValue.ToString() == KHz)
                {
                    _DontChange = true;
                    try
                    {
                        DataTextBox.Text = Converters.ValueToUI(_Value, 1000);
                    }
                    finally
                    {
                        _DontChange = false;
                    }
                }

            if (((ComboBox)sender).SelectedValue.ToString() == Hz)
                {
                    _DontChange = true;
                    try
                    {
                        DataTextBox.Text = Converters.ValueToUI(_Value, 1);
                    }
                    finally
                    {
                        _DontChange = false;
                    }
                }

        }

        private void DataTextBox_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Enter)   //завершён ввод
            {
                string name = ((FrequencyCtrl)((Grid)((TextBox)sender).Parent).Parent).Name;
                if (name == "HandMode_Frequency") //тактовая частота
                {
                    FrequencyCtrlChanged?.Invoke();
                    return;
                }
                if (name == "StartFrequency" ||
                    name == "StopFrequency" ||
                    name == "InnerStepFrequency") //параметры для расчёта количества точек измерения
                    ParameterCtrChanged?.Invoke();
            }
        }
    }
}

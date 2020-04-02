using System;
using System.Windows;
using System.Windows.Controls;

namespace UnitedTools.Chart
{
    /// <summary>
    /// Interaction logic for Tuner.xaml
    /// </summary>
    public partial class Tuner : UserControl
    {

        public string Title
        {
            get => labelTitle.Content.ToString();

            set => labelTitle.Content = value;
        }


        public double Value
        {
            get => sliderValue.Value;

            set => sliderValue.Value = value;
        }

        public Action<double, double> NewValue;


        public Tuner()
        {
            InitializeComponent();
        }

        private void Slider_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {

        }


        double _OldValue = 0;
        private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            labelValue.Content = e.NewValue.ToString("F2");
            NewValue?.Invoke(e.NewValue, _OldValue);
            _OldValue = e.NewValue;
        }
    }
}

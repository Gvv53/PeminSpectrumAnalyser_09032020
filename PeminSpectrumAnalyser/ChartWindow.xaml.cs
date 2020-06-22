using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using PeminSpectrumAnalyser.Helpers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Linq;



namespace PeminSpectrumAnalyser
{
    /// <summary>
    /// Логика взаимодействия для ChartWindow.xaml
    /// </summary>
    public partial class ChartWindow : Window
    {
        ViewModelChart vmc = new ViewModelChart();
       //public ChartWindow(ObservableCollection<PointForChart> dataForChart)
       public ChartWindow(ObservableCollection<PointForChart> dataSignal)
        {
            try
            {
                InitializeComponent();
                vmc = new ViewModelChart();
                vmc.dataForChart = dataSignal;
                vmc.dataForChartCalc = new ObservableCollection<PointForChart>( dataSignal.Where(p => p.noise_marker != 0 || p.signal_marker != 0));
                this.DataContext = vmc;
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
                return;
            }
        }
    }
    public class PointForChart
    {
        public PointForChart(double f,double s,double n)
        { freq = f; signal = s; noise = n; }
        public double freq { get; set; }
        public double signal { get; set; }
        public double noise { get; set; }
        public double signal_marker { get; set; }
        public double noise_marker { get; set; }
        // public double noise;
    }

    public class ViewModelChart //: NotificationObject
    {
        public ObservableCollection<PointForChart> dataForChart { get; set; }
        public ObservableCollection<PointForChart> dataForChartCalc { get; set; }

        public ViewModelChart()
        {
            dataForChart = new ObservableCollection<PointForChart>();
            dataForChartCalc = new ObservableCollection<PointForChart>();
        }
       
       
    }
}

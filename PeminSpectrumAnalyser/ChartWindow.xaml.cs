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
using DevExpress.Charts;
using DevExpress.Xpf.Charts;
using DevExpress.Data.Mask;

namespace PeminSpectrumAnalyser
{
    /// <summary>
    /// Логика взаимодействия для ChartWindow.xaml
    /// </summary>
    public partial class ChartWindow : Window
    {
        ObservableCollection<PointForChart> dataSignal;
        SeriesPoint pointUpdated,pointCurrent = new SeriesPoint();
        ViewModelChart vmc = new ViewModelChart();
       //public ChartWindow(ObservableCollection<PointForChart> dataForChart)
       public ChartWindow(ObservableCollection<PointForChart> dataSignal,bool isDS,SeriesPoint pointUpdated)
        {
            try
            {
                this.pointUpdated = pointUpdated;
                InitializeComponent();               
                if (isDS) //обработчики для изменения точки съёма сигнала
                {
                    chart.MouseMove += chart_MouseMove;
                    //chart.MouseLeave += chart_MouseLeave;
                    chart.MouseDoubleClick += chart_MouseDoubleClick;
                }
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

      
        private void chart_MouseMove(object sender, MouseEventArgs e)
        {
            ChartHitInfo hitInfo = chart.CalcHitInfo(e.GetPosition(chart));

            if (hitInfo != null && hitInfo.SeriesPoint != null)
            {
                pointCurrent = hitInfo.SeriesPoint;
            }
        }
        private void chart_MouseLeave(object sender, MouseEventArgs e)
        {
            //pointSignal = null;
        }

      

        private void chart_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (System.Windows.Forms.MessageBox.Show("Вы действительно хотите заменить точку съёма измерения?", "", System.Windows.Forms.MessageBoxButtons.YesNo) == System.Windows.Forms.DialogResult.No)
            {
                pointUpdated = null;
            }
            else
            {

                pointUpdated.Argument = pointCurrent.Argument;
                pointUpdated.Value = pointCurrent.Value;
                var temp = vmc.dataForChart.Where(p => p.freq == double.Parse(pointCurrent.Argument)).FirstOrDefault();//новая точка измерения на графике
                if (temp != null)
                {
                    vmc.dataForChartCalc[0].signal = temp.signal;
                    vmc.dataForChartCalc[0].signal_marker = temp.signal;
                    vmc.dataForChartCalc[0].noise = temp.noise;
                    vmc.dataForChartCalc[0].noise_marker = temp.noise;
                    vmc.dataForChartCalc[0].freq = temp.freq;
                    chart.UpdateData();
                }
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

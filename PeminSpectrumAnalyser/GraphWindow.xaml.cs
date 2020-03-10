using System.Windows;
using UnitedTools.Chart;

namespace PeminSpectrumAnalyser
{
    /// <summary>
    /// Interaction logic for GraphWindow.xaml
    /// </summary>
    public partial class GraphWindow : Window
    {
        public GraphWindow()
        {
            InitializeComponent();
        }

        public ChartCtrl CurrentChart
        {
            get => currentChart;
        }

        public ShiftTuner CurrentShiftTuner
        {
            get => shiftTuner;
        }

        public string Caption
        {
            set => Title = value;
            get => Title;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
        }
    }
}

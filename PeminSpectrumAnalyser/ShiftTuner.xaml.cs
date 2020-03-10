using System.Windows.Controls;
using UnitedTools.Chart;

namespace PeminSpectrumAnalyser
{
    /// <summary>
    /// Interaction logic for ShiftTuner.xaml
    /// </summary>
    /// 
    public partial class ShiftTuner : UserControl
    {
        public ShiftTuner()
        {
            InitializeComponent();
        }

        public Tuner TunerYScale
        {
            get => tunerYScale;
            set => tunerYScale = value;
        }

        public Tuner TunerXScale
        {
            get => tunerXScale;
            set => tunerXScale = value;
        }

        public Tuner TunerNoiseShift
        {
            get => tunerNoiseShift;
            set => tunerNoiseShift = value;

        }

        public Tuner TunerSignalShift
        {
            get => tunerSignalShift;
            set => tunerSignalShift = value;
        }

        public bool LinkSignalNoise
        {
            get => (bool)Link.IsChecked;
            set => Link.IsChecked = value;
        }


    }
}

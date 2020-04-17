using IOMeasurementData;
using System;
using System.IO;
using System.Xml.Serialization;

namespace PeminSpectrumData
{
    [Serializable]
    public class ExperimentSettings
    {
        public ExperimentSettings()
        {
            HardwareSettings = new HardwareSettings();
            CommonShift = HardwareSettings.CommonShift;
        }

        public HardwareSettings HardwareSettings;// = new HardwareSettings();

        public string _ExperimentPath;
        public string ExperimentPath
        {
            get
            {
                if (Directory.Exists(_ExperimentPath))
                    return _ExperimentPath;
                else
                {
                    return _ExperimentPath = (Directory.CreateDirectory
                                                (Environment.GetFolderPath
                                                    (Environment.SpecialFolder.MyDocuments)
                                                        + HardwareSettings.ExperimentsPath)).FullName; ;
                }
            }
            set => _ExperimentPath = value;
        }

        public double CommonShift { get; set; }//= HardwareSettings.CommonShift;

        public int MeasurementCountForNoise { get; set; } = 1;
        public int MeasurementCountForSignal { get; set; } = 1;

        public  bool AverageTypeForNoiseOff { get; set; }
        public  bool AverageTypeForNoiseMaximum { get; set; }
        public  bool AverageTypeForNoiseMiddle { get; set; }
        public  bool AverageTypeForNoiseMinimum { get; set; }

        public  bool AverageTypeForSignalOff { get; set; }
        public  bool AverageTypeForSignalMaximum { get; set; }
        public  bool AverageTypeForSignalMiddle { get; set; }
        public  bool AverageTypeForSignalMinimum { get; set; }

        public  bool Emulation { get; set; }

        public bool[] Flags { get; set; } = new bool[32];
        public double[] Values { get; set; } = new double[32];
        public string[] Strings { get; set; } = new string[32];

        [XmlIgnore]
        public Object[] Objects { get; set; } = new string[32];

        [XmlIgnore]
        public string HardwareType
        {
            get => Strings[0];
            set => Strings[0] = value;
        }
    }
}


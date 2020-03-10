using IOMeasurementData;
using System;
using System.Xml.Serialization;

namespace PeminSpectrumData
{
    [Serializable]
    public class IntervalSettings
    {
        /// <summary>
        /// Зарезервировано
        /// </summary>
        public int PointsQuantity { get; set; }
        public long FrequencyStart { get; set; } = 1000000;
        public long FrequencyStop { get; set; } = 10000000;
        public long FrequencyStep { get; set; } = 100000;
        public long FrequencyInnerStep { get; set; } = 0;

        public long BandWidth { get; set; } = 1000000;
        public long Span { get; set; } = 1000000;
        public long Band { get; set; } = 1000000;

        public bool isAuto { get; set; } = true;
        public long _HandCenterFrequency { get; set; } = 1000000;
        public long HandCenterFrequency 
        { 
            get => _HandCenterFrequency;
            set
            {
                _HandCenterFrequency = value;
                if(!isAuto)
                {
                    Span = value;
                    Band = value;
                    BandWidth = value;
                    FrequencyStart = _HandCenterFrequency - _HandCenterFrequency / 2;
                    FrequencyStop = _HandCenterFrequency + _HandCenterFrequency / 2;
                }
            } 
        }

        public string Message1BeforeStartMeasuring { get; set; } = "";
        public bool EnableMessage1BeforeStartMeasuring { get; set; } = false;
        public string Message2BeforeStartMeasuring { get; set; } = "";
        public bool EnableMessage2BeforeStartMeasuring { get; set; } = false;
        public string Message3BeforeStartMeasuring { get; set; } = "";
        public bool EnableMessage3BeforeStartMeasuring { get; set; } = false;

        public Double MaxNoise { get; set; } = 0;
        public Double MinNoise { get; set; } = 0;
        public Double MaxSignal { get; set; } = 0;
        public Double MinSignal { get; set; } = 0;
        public Double ShiftNoise { get; set; } = 0;
        public Double ShiftSignal { get; set; } = 0;

        /// <summary>
        /// Стартовая частота для отработки смещения по дельте
        /// </summary>
        public long DeltaShiftFrequency { get; set; } = 3600000000;
        /// <summary>
        /// Значение для отработки смещения  вручную заданной частоты и дельты
        /// </summary>
        public Double DeltaShiftValue { get; set; } = 0;

        public bool[] Flags { get; set; } = new bool[32];
        public double[] Values { get; set; } = new double[32];
        public string[] Strings { get; set; } = new string[32];

        [XmlIgnore]
        public Object[] Objects { get; set; } = new Object[32];

        [XmlIgnore]
        public double SpecialSignalNoiseShift
        {
            get => Values[1];
            set => Values[1] = value;
        }

        [XmlIgnore]
        public Object LinkToVisualControl
        {
            get => Objects[0];
            set => Objects[0] = (Object)value;
        }

        [XmlIgnore]
        public bool EnableMessage1BeforeStartMeasuringForNoise
        {
            get => Flags[0];
            set => Flags[0] = value;
        }

        [XmlIgnore]
        public bool EnableMessage2BeforeStartMeasuringForNoise 
        {
            get => Flags[1];
            set => Flags[1] = value;
        }

        [XmlIgnore]
        public bool EnableMessage3BeforeStartMeasuringForNoise
        {
            get => Flags[2];
            set => Flags[2] = value;
        }
    }
}

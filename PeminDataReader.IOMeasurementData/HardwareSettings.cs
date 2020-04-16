using System;
using System.Collections.Generic;

namespace IOMeasurementData
{
    [Serializable]
    public class HardwareSettings
    {
        public HardwareType HardwareType { get; set; }
        public string HardwareDescription { get; set; } = "FSH4";
        public string IP { get; set; } = @"192.168.12.233";
        public int Port { get; set; } = 5555;
        public int PointsQuantity { get; set; } = 631;
        public string ExperimentsPath { get; set; } = @"\PEMIN\EXPERIMENTS\";
        public string TraceModeForNoise { get; set; } = @"POSitive";//@"AVERage";
        public string TraceModeForSignal { get; set; } = @"AVERage";
        public string SignalTraceType { get; set; } = @"MAXHold";
        public string NoiseTraceType { get; set; } = @"AVERage";
        public double DbMkvShift { get; set; } = 108.75;
        public double CommonShift { get; set; } = 3;
        public long Frequency { get; set; } = 10000000;
        public long BandWidth { get; set; } = 100000;
        public long Span { get; set; } = 100000;
        public long Band { get; set; } = 100000;
        //передаётся на ИП
        public string TraceDetector { get; set; } = "Detector";
        public string TraceType { get; set; } = "Type";
        //
        public AverageType AverageType { get; set; } = AverageType.Middle;
        public int MeasurementCount { get; set; } = 3;
    }
}

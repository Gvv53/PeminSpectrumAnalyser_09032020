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
     
        public string SignalTraceType { get; set; } = @"MAXHold";
        public string NoiseTraceType { get; set; } = @"AVERage";
        public string TraceType { get; set; }   //передаётся ИП

        public string SignalTraceDetector { get; set; } = @"POSitive" ;
        public string NoiseTraceDetector { get; set; } = @"AVERage";
        public string TraceDetector { get; set; }   //передаётся ИП

        public double DbMkvShift { get; set; } = 108.75;
        public double CommonShift { get; set; } = 3;
        public long Frequency { get; set; } = 10000000;
        public long BandWidth { get; set; } = 100000; //RBW
        public long Span { get; set; } = 100000;
        public long Band { get; set; } = 100000;     //VBW
        //передаётся на ИП
        public long SignalAttenuation { get; set; } = 0; //ослабление 10 дБ
        public long NoiseAttenuation { get; set; } = 0;
        public long Attenuation { get; set; }   //передаётся ИП


       // public string SignalTraceMode { get; set; } = "NONE";
       // public string NoiseTraceMode { get; set; } = "NONE";
       //public string TraceMode { get; set; }   //передаётся ИП

        public long CountSignalTraceMode { get; set; } = 10;
        public long CountNoiseTraceMode { get; set; } = 10; 
        public long CountTraceMode { get; set; }//передаётся ИП
        //
        public AverageType AverageType { get; set; } = AverageType.Middle;
        public int MeasurementCount { get; set; } = 3;
        public bool Preamp { get; set; }
    }
}

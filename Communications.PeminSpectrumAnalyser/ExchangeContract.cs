using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Communications.PeminSpectrumAnalyser
{
    [DataContract]
    public class ExchangeContract
    {
        [DataMember]
        public int ID { get; set; }

        [DataMember]
        public string Description { get; set; }

        [DataMember]
        public List<double> Noise { get; set; } = new List<double>();

        [DataMember]
        public List<double> Signal { get; set; } = new List<double>();

        [DataMember]
        public List<double> Frequencys { get; set; } = new List<double>();

        [DataMember]
        public List<double> OriginalNoise { get; set; } = new List<double>();

        [DataMember]
        public List<double> OriginalSignal { get; set; } = new List<double>();
    }
}

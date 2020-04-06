using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PeminSpectrumAnalyser
{
    public static class HardwareSettings
    {
        public static string Left_Description { get; set; } = "FSH4";
        public static string Rigth_Description { get; set; } = "FSH4";

        public static string Left_IP { get; set; } = @"192.168.12.233";
        public static int Left_Port { get; set; } = 5025;


    }
}

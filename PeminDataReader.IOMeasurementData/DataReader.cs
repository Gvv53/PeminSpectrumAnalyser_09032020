using IOMeasurementData;
using System;
using System.Diagnostics;
using System.Globalization;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Windows;

namespace IOMeasurementData
{
    public class DataReader
    {
        public HardwareSettings HardwareSettings { get; set; }
        public Commands Commands { get; set; }

        public double[] ResultsX { get; set; }
        public double[] ResultsY { get; set; }

        public DataReader(HardwareSettings hardwareSettings) => SetNewHardwareSettings(hardwareSettings);

        public void SetNewHardwareSettings(HardwareSettings hardwareSettings)
        { 
            HardwareSettings = hardwareSettings;
            Commands = CommandsFactory.CreateCommands(hardwareSettings);

        }

        public bool Connect()
        {
            bool result = Commands.Connect(HardwareSettings.IP, HardwareSettings.Port);

            if (result)  //успешное подключение
                Commands.Init();


            return result;
        }

        public bool GetDataPoints()
        {
            bool result = false;

            if (HardwareSettings.MeasurementCount < 1)
                HardwareSettings.MeasurementCount = 1;

            int currentMeasurementCount = HardwareSettings.MeasurementCount;

            if (HardwareSettings.AverageType == AverageType.Off)
                 currentMeasurementCount = 1;


            ResultsX = new double[HardwareSettings.PointsQuantity];
            ResultsY = new double[HardwareSettings.PointsQuantity];

            for (int counter = 0; counter < HardwareSettings.MeasurementCount; counter++)
            {
                try
                {
                    string[] currentSpectrData = 
                        (Encoding.UTF8.GetString
                            (Commands.GetDataResult
                            ( 
                                HardwareSettings.TraceDetector, 
                                HardwareSettings.Frequency,
                                HardwareSettings.BandWidth, 
                                HardwareSettings.Span,
                                HardwareSettings.Band,
                                HardwareSettings.TraceType,
                                HardwareSettings.Attenuation,
                                HardwareSettings.Preamp,
                                HardwareSettings.CountTraceMode
                            )                                                                                                                                )
                        ).Split(',');

                    if (currentSpectrData.Length >= (HardwareSettings.PointsQuantity * 2))
                    {
                        for (int shift = 0; shift < HardwareSettings.PointsQuantity; shift++)
                        {

                            ResultsX[shift] = double.Parse(currentSpectrData[shift * 2], CultureInfo.InvariantCulture);

                            if (HardwareSettings.AverageType == AverageType.Off)
                                ResultsY[shift] = double.Parse(currentSpectrData[shift * 2 + 1], CultureInfo.InvariantCulture);// + HardwareSettings.DbMkvShift;//+ HardwareSettings.CommonShift;

                            //if (HardwareSettings.AverageType == AverageType.Middle)
                            //    ResultsY[shift] = 
                            //        (ResultsY[shift] + (double.Parse(currentSpectrData[shift * 2 + 1], CultureInfo.InvariantCulture) + HardwareSettings.DbMkvShift + HardwareSettings.CommonShift))
                            //                            / (counter > 0 ? 2 : 1);

                            //if (HardwareSettings.AverageType == AverageType.Maximum)
                            //{
                            //    double bufferY = double.Parse(currentSpectrData[shift * 2 + 1], CultureInfo.InvariantCulture) + HardwareSettings.DbMkvShift + HardwareSettings.CommonShift;

                            //    if (bufferY > ResultsY[shift])
                            //        ResultsY[shift] = bufferY;
                            //}

                            //if (HardwareSettings.AverageType == AverageType.Minimum)
                            //{
                            //    double bufferY = double.Parse(currentSpectrData[shift * 2 + 1], CultureInfo.InvariantCulture) + HardwareSettings.DbMkvShift + HardwareSettings.CommonShift;

                            //    if (ResultsY[shift] == Double.MinValue)
                            //        ResultsY[shift] = bufferY;
                            //    else
                            //        if (bufferY < ResultsY[shift])
                            //            ResultsY[shift] = bufferY;
                            //}
                        }
                    }

                    result = true;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                }
            }
            return result;
        }
    }
}


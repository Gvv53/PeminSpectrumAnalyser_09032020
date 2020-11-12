using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IOMeasurementData
{
    public static class CommandsFactory
    {
        public static Commands CreateCommands(HardwareSettings hardwareSettings)
        {
            Commands result;

            switch(hardwareSettings.HardwareType)
            {
                case HardwareType.Agilent934хC:
                    hardwareSettings.PointsQuantity = 461;
                    hardwareSettings.HardwareDescription = HardwareType.Agilent934хC.ToString();
                    result = new CommandsAgilent934хC();
                    break;
                case HardwareType.AGILENT90х0:
                    hardwareSettings.PointsQuantity = 1001;
                    hardwareSettings.HardwareDescription = HardwareType.AGILENT90х0.ToString();
                    result = new CommandsAgilent90х0();
                    break;
                case HardwareType.AGILENT9915x:
                    hardwareSettings.PointsQuantity = 401;
                    hardwareSettings.HardwareDescription = HardwareType.AGILENT9915x.ToString();
                    result = new CommandsAgilent9915x();
                    break;
                case HardwareType.FSH4:
                    hardwareSettings.PointsQuantity = 631;
                    hardwareSettings.HardwareDescription = HardwareType.FSH4.ToString();
                    result = new CommandsFSH4();
                    break;
                case HardwareType.FSH18:
                    hardwareSettings.PointsQuantity = 501;
                    hardwareSettings.HardwareDescription = HardwareType.FSH18.ToString();
                    result = new CommandsFSH18();
                    break;
                default:
                    hardwareSettings.PointsQuantity = 631;
                    result = new CommandsFSH4();
                    break;
            }
            return result;
        }
    }
}

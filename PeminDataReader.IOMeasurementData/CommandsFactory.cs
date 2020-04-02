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
                case HardwareType.AGILENT9XXX:
                    hardwareSettings.PointsQuantity = 1001;
                    result = new CommandsAgilent9XXX();
                    break;
                case HardwareType.FSH4:
                    hardwareSettings.PointsQuantity = 631;
                    result = new CommandsFSH4();
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

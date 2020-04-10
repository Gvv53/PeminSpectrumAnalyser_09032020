using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace IOMeasurementData
{
    public class CommandsAgilent9XXX : Commands
    {
        public static bool SmallInit = false; //не устанавливать настройки
        public CommandsAgilent9XXX()
        { 
        }


        public override void Init()
        {
            Send("*RST");//установка в начальное состояние
            if (SmallInit)
                return;
            Send("(SET DefaultTimeout to 2000)");
            Send(":FORMat:TRACe:DATA ASC,8");
            Send(":FORMat:BORDer SWAPped");
            Send(":INSTrument:SELect SA");
            Send(":CONFigure:SANalyzer");
            Send(":DISPlay:WINDow:TRACe:Y:SCALe:RLEVel - 30");
            Send(":SENSe:AVERage: COUNt 25");
            Send(":DISPlay:WINDow:TRACe:Y:SCALe:PDIVision 7");
            Send(":INITiate:CONTinuous 0");
            Send(":TRACe1:TYPE WRITe");
        }

        public override byte[] GetDataResult(string traceDetector,
                                      long frequency,
                                      long bandWidth,
                                      long span,
                                      long band, 
                                      int errorCount = 0)
        {
            //Размер буфера приема (в байтах). Значение по умолчанию — 8192 байта
            byte[] bytes = new byte[newClient.ReceiveBufferSize];

            try
            {
                Send(":SENSe:DETector:TRACe1 " + traceDetector);
                Send(":FREQUENCY:CENTER " + frequency.ToString() + " Hz");
                
                Send(":SENSe:WAVEform:BANDwidth:RESolution " + bandWidth.ToString() + " Hz");

                Send(":FREQUENCY:SPAN " + span.ToString() + " Hz");

                Send(":BAND: " + band.ToString() + " Hz");

                Send(":SENSe:SWEep:POINts 1001");
                Send(":INITiate:IMMediate");
                Send("*WAI");
                Send(":FETCh:SANalyzer1?");
                Send(":FETCh?");

                Thread.Sleep(30);

                tcpStream.Read(bytes, 0, newClient.ReceiveBufferSize);
            }
            catch
            {
                if (errorCount == 1)
                {
                    MessageBox.Show("Повторная попытка переподключения и повторной посылки данных не удалась. Проверьие соединение с прибором. Попробуйте перезагрузить прибор");
                    Environment.Exit(0);
                }
            }
            return bytes;
        }

    }
}

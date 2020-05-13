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
            //Устанавливает номер счетчика клемм N для типов трассы Среднее, Макс. Удержание и Мин. Удержание. 
            //Драйвер использует это значение для установки атрибута AGMXA_ATTR_AVG_NUMBER.
            Send(":SENSe:AVERage: COUNt 25"); //по умолчанию 100

            Send(":DISPlay:WINDow:TRACe:Y:SCALe:PDIVision 7");
            Send(":INITiate:CONTinuous 0");
           // Send(":TRACe1:TYPE WRITe");
        }

        public override byte[] GetDataResult(string traceDetector,
                                      long frequency,
                                      long bandWidth,
                                      long span,
                                      long band,
                                      string traceType,
                                      long attenuation,
                                      string traceMode,
                                      long countTraceMode,
                                      int errorCount = 0
                                      )
        {
            //Размер буфера приема (в байтах). Значение по умолчанию — 8192 байта
            byte[] bytes = new byte[newClient.ReceiveBufferSize];

            try
            {
                Send(":TRACe1:TYPE" +   traceType);     //тип трассировки

                Send(":SENSe:DETector:TRACe1:AUTO" + "0"); //отключен автодетектор
 
                Send(":SENSe:DETector:TRACe1 " + traceDetector);

                //аттеньюатор
                Send(":SENSe:POWer:ATTenuation:AUTO 0");
                Send(":SENSe:POWer:ATTenuation" + attenuation.ToString());

                //TraceMode
                Send(":SENSe:AVERage:COUNt " + countTraceMode.ToString());
                Send(":SENSe:AVERage:TYPE " + traceMode);
 
                // Send(":FREQUENCY:CENTER " + frequency.ToString() + " Hz");

                // Send(":SENSe:WAVEform:BANDwidth:RESolution " + bandWidth.ToString() + " Hz");
                Send(":SENSe:BANDwidth:RESolution" + bandWidth.ToString() + " Hz");

                Send(":FREQUENCY:SPAN " + span.ToString() + " Hz");

                //Send(":BAND: " + band.ToString() + " Hz");
                Send(":SENSe:BANDwidth:VIDeo:AUTO 0|");
                Send(":SENSe:BANDwidth:VIDeo" + band.ToString() + " Hz");

                Send(":FREQUENCY:CENTER " + frequency.ToString() + " Hz"); //поменяла место

               



                Send(":SENSe:SWEep:POINts 1001");
                Send(":INITiate:IMMediate");  //запуск развёртки
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

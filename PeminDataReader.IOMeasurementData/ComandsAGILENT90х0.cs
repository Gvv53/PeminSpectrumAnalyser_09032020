using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace IOMeasurementData
{
    public class CommandsAgilent90х0 : Commands
    {
        public static bool SmallInit = false; //не устанавливать настройки
        public CommandsAgilent90х0()
        {
        }


        public override void Init()
        {
            Send("*RST");//установка в начальное состояние
            if (SmallInit)
                return;
           
            Send(":UNIT:POWer DBUV");
            Send(":DISPlay:WINDow:TRACe:Y:SCALe:PDIVision 10");
            Send(":INITiate:CONTinuous OFF");
            Send(":INST:SEL SA"); //по умолчанию

            Send(":FORMat:TRACe:DATA ASCii");
            Send(":SENSe:AVERage:STATe ON");
            




             Send(":TRACe1:TYPE WRITe");
        }

        public void Init_Old()
        {
            Send("*RST");//установка в начальное состояние
            if (SmallInit)
                return;
            Send("(SET DefaultTimeout to 2000)");
            Send(":FORMat:TRACe:DATA ASC,8");
            Send(":FORMat:BORDer SWAPped");
            Send(":INSTrument:SELect SA");
            Send(":CONFigure:SANalyzer");
            Send(":DISPlay:WINDow:TRACe:Y:SCALe:RLEVel " + "30");
            Send(":DISPlay:WINDow:TRACe:Y:SCALe:PDIVision DIV5");
            //Устанавливает номер счетчика клемм N для типов трассы Среднее, Макс. Удержание и Мин. Удержание. 
            //Драйвер использует это значение для установки атрибута AGMXA_ATTR_AVG_NUMBER.
            Send(":SENSe:AVERage: COUNt " + "25"); //по умолчанию 100


            Send(":INITiate:CONTinuous " + "1");
            // Send(":TRACe1:TYPE WRITe");
        }
        public override byte[] GetDataResult(string traceDetector,
                                      long frequency,
                                      long bandWidth,
                                      long span,
                                      long band,
                                      string traceType,
                                      long attenuation,
                                      bool preamp,
                                      long countTraceMode,
                                      bool isManualSWP,
                                      double ManualSWP,
                                      int errorCount = 0
                                      )
        {
            //Размер буфера приема (в байтах). Значение по умолчанию — 8192 байта
            newClient.ReceiveBufferSize = 35000;
            byte[] bytes = new byte[newClient.ReceiveBufferSize];

            try
            {

                Send(":SENSe:DETector:AUTO OFF"); //отключен автодетектор

                Send(":SENSe:DETector " + traceDetector);

                Send(":TRACe1:TYPE " + traceType);     //тип трассировки

                //аттеньюатор
                Send(":SENSe:POWer:ATTenuation:AUTO 0");
                Send(":SENSe:POWer:ATTenuation " + attenuation.ToString()); //выставляется значение кратное 5(уменьшается до кратного)

                //TraceMode
                Send(":SENSe:AVERage:COUNt " + countTraceMode.ToString());

                Send(":FREQUENCY:SPAN " + span.ToString() + " Hz");

                Send(":SENSe:BANDwidth:RESolution:AUTO OFF ");
                Send(":SENSe:BANDwidth:RESolution " + bandWidth.ToString() + " Hz");

                Send(":SENSe:BANDwidth:VIDeo:AUTO OFF");
                Send(":SENSe:BANDwidth:VIDeo " + band.ToString() + " Hz");

                Send(":FREQUENCY:CENTER " + frequency.ToString() + " Hz"); //поменяла место               

                Send(":SENSe:POWer:GAIN " + (preamp ? "ON" : "OFF"));
                if (isManualSWP && ManualSWP != 0)
                {
                    Send(":SENSe:SWEep:TIME:AUTO OFF");
                    var cc = Thread.CurrentThread.CurrentCulture;
                    Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");
                    Send(":SENSe:SWEep:TIME " + ((double)ManualSWP / 1000).ToString());
                    Thread.CurrentThread.CurrentCulture = cc;
                }
                else
                    Send(":SENSe:SWEep:TIME:AUTO ON");
                Send(":INITiate:IMMediate");  //запуск развёртки
                Send("*WAI");
                Thread.Sleep(2000);
                Send(":FETC:SAN?");
                Thread.Sleep(2000);
                tcpStream.Read(bytes, 0, newClient.ReceiveBufferSize);
                //этот прибор выдаёт измерения с частотой
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

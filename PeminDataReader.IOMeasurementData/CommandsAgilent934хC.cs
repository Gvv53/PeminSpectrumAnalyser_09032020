using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Threading;
using System.Globalization;

namespace IOMeasurementData
{
    public class CommandsAgilent934хC : Commands
    {
        public static bool SmallInit = false; //не устанавливать настройки
        public CommandsAgilent934хC()
        { 
        }


        public override void Init()
        {
            Send("*RST");//установка в начальное состояние
            if (SmallInit)
                return;
            //Send(":DISPlay:WINDow:TRACe:Y:RLEVel 30");
            Send(":UNIT:POWer DBUV");
            Send(":DISPlay:WINDow:TRACe:Y:SCALe:PDIVision DIV10");
            Send(":INITiate:CONTinuous OFF");
            Send(":INSTrument SA"); //по умолчанию

            Send(":FORMat:TRACe:DATA ASCii");
            

            //Устанавливает номер счетчика клемм N для типов трассы Среднее, Макс. Удержание и Мин. Удержание. 
            //Драйвер использует это значение для установки атрибута AGMXA_ATTR_AVG_NUMBER.
           
            Send(":SENSe:AVERage:TRACe1:COUNt 2"); //по умолчанию 100
            Send(":SENSe:AVERage:TRACe1:STATe ON");
            




            // Send(":TRACe1:TYPE WRITe");
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
            Send(":DISPlay:WINDow:TRACe:Y:SCALe:PDIVision DIV5" );
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
            byte[] bytes = new byte[newClient.ReceiveBufferSize];

            try
            {               

                Send(":SENSe:DETector:TRACe1:AUTO OFF"); //отключен автодетектор
 
                Send(":SENSe:DETector:TRACe1 " + traceDetector);               

                Send(":TRACe1:MODE " + traceType); //тип трассировки
                
                //аттеньюатор               
                Send(":SENSe:POWer:ATTenuation:AUTO 0");
                Send(":SENSe:POWer:ATTenuation " + attenuation.ToString()); //выставляется значение кратное 5(уменьшается до кратного)
               
                //TraceMode
                Send(":SENSe:AVERage:COUNt " + countTraceMode.ToString());               

                Send(":SENSe:BANDwidth:AUTO OFF");
                Send(":SENSe:BANDwidth " + bandWidth.ToString() + " Hz"); //

                Send(":SENSe:BANDwidth:VIDeo:AUTO OFF");
                Send(":SENSe:BANDwidth:VIDeo " + band.ToString() + " Hz");

                Send(":FREQUENCY:SPAN " + span.ToString() + " Hz");

                Send(":FREQUENCY:CENTER " + frequency.ToString() + " Hz"); //поменяла место               
           
                Send(":INSTrument:MEASure OFF");  //мощность измерения
                Send(":SENSe:POWer:GAIN " +(preamp ? "ON" : "OFF"));
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
                Send(":TRACe:DATA? TRACe1");
                
                Thread.Sleep(30);

                tcpStream.Read(bytes, 0, newClient.ReceiveBufferSize);
                string str = Encoding.ASCII.GetString(bytes);
                string[] qresult = str.Split(',');
                //
                if (qresult.Count() > 1)
                {
                    double shift = span / (qresult.Count() - 1);
                    int counter = 0;
                    StringBuilder result = new StringBuilder();

                    long start = frequency - (span / 2);
                    string itemNew; double number;
                    var cc = Thread.CurrentThread.CurrentCulture;
                    Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");   
                    foreach (string item in qresult)
                    {
                        //Double.TryParse(item, out number);
                        //itemNew = (number + 108.75).ToString();
                        if (counter > 0)
                            result.Append(',').Append(start + shift * counter).Append(',').Append(item);
                        else
                            result.Append(start + shift * counter).Append(',').Append(item);

                        counter++;
                    }
                    Thread.CurrentThread.CurrentCulture = cc;
                    byte[] byteResult = Encoding.ASCII.GetBytes(result.ToString());

                    return byteResult;
                }

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

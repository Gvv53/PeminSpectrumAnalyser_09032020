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
    public class CommandsAgilent9915x : Commands
    {
        public static bool SmallInit = false; //не устанавливать настройки
        public CommandsAgilent9915x()
        { 
        }


        public override void Init()
        {
            Send("*RST");//установка в начальное состояние
            Send(":INSTrument 'SA'"); //по умолчанию
            Send(":INITiate:CONTinuous OFF");

            Send(":AMPLitude:UNIT DBUV");
            Send(":DISP:WIND:TRACe1:Y:PDIV 10" );
            Send(":DISP:WIND:TRACe1:Y:RLEV 50");
          
            Send(":FORMat:DATA ASCii");

           
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

                //Send(":SENSe:DETector:AUTO OFF"); //отключен автодетектор
 
                Send(":SENSe:DETector:FUNCtion " + traceDetector);
                traceType = traceType == "AVERage" ? "AVG" : (traceType == "WRITe" ? "CLRW" : traceType);
                Send(":TRACe1:TYPE " + traceType);     //тип трассировки
              
                //аттеньюатор
               
                Send(":SENSe:POWer:ATTenuation:AUTO OFF");
                Send(":SENSe:POWer:ATTenuation " + attenuation.ToString()); //выставляется значение кратное 5(уменьшается до кратного)
               
                //TraceMode
                Send(":SENSe:AVERage:COUNt " + countTraceMode.ToString());


                Send(":SENSe:BANDwidth:AUTO OFF");
                Send(":SENSe:BANDwidth " + bandWidth.ToString() + " Hz"); //

                Send(":SENSe:BANDwidth:VIDeo:AUTO OFF");
                Send(":SENSe:BANDwidth:VIDeo " + band.ToString() + " Hz");
                
                Send(":FREQUENCY:SPAN " + span.ToString() + " Hz");              

                Send(":FREQUENCY:CENTER " + frequency.ToString() + " Hz"); //поменяла место               
           
                Send(":SENSe:POWer:GAIN " +(preamp ? "ON" : "OFF"));

                if (isManualSWP && ManualSWP != 0)
                {
                    Send(":SENSe:SWEep:ACQuisition:AUTO OFF");
                    var cc = Thread.CurrentThread.CurrentCulture;
                    Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");
                    Send(":SENSe:SWEep:ACQuisition " + ((double)ManualSWP ).ToString()); //относительная развёртка 1 - самая быстрая, 5000 - самая медленная
                    Thread.CurrentThread.CurrentCulture = cc;
                }
                else
                    Send(":SENSe:SWEep:ACQuisition:AUTO ON");

                Send(":INITiate:IMMediate");  //запуск развёртки
                Send("*WAI");
                Thread.Sleep(1500);//время д.б.не меньше развёртки

               
                Send(":TRACe1:DATA?");

                Thread.Sleep(1500);//время д.б.не меньше развёртки

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

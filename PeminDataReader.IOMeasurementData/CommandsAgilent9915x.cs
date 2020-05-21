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
            Send(":DISP:WIND:TRAC1:Y:PDIV 10");
            
            

            Send(":FORMat:DATA ASCii");

            //Send(":SENSe:AVERage ON");
            Send(":TRACe1:TYPE CLRW");

            //Устанавливает номер счетчика клемм N для типов трассы Среднее, Макс. Удержание и Мин. Удержание. 
            //Драйвер использует это значение для установки атрибута AGMXA_ATTR_AVG_NUMBER.

            //Send(":SENSe:AVERage:COUNt 2"); //по умолчанию 100
            //Send(":SENSe:AVERage ON");





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

                //Send(":TRACe1:MODE " + traceType);
                //аттеньюатор
               
                Send(":SENSe:POWer:ATTenuation:AUTO OFF");
                Send(":SENSe:POWer:ATTenuation " + attenuation.ToString()); //выставляется значение кратное 5(уменьшается до кратного)
               
                //TraceMode
                Send(":SENSe:AVERage:COUNt " + countTraceMode.ToString());
                //  Send(":SENSe:AVERage:TYPE " + traceMode);

                Send(":SENSe:BANDwidth:AUTO OFF");
                Send(":SENSe:BANDwidth " + bandWidth.ToString() + " Hz"); //

                Send(":SENSe:BANDwidth:VIDeo:AUTO OFF");
                Send(":SENSe:BANDwidth:VIDeo " + band.ToString() + " Hz");

                Send(":FREQUENCY:SPAN " + span.ToString() + " Hz");

                //Send(":SENSe:BANDwidth:RESolution:AUTO OFF");
                //Send(":SENSe:BANDwidth:RESolution" + bandWidth.ToString() + " Hz") ;

              

                Send(":FREQUENCY:CENTER " + frequency.ToString() + " Hz"); //поменяла место               
           
               // Send(":INSTrument:MEASure OFF");  //мощность измерения
                Send(":SENSe:POWer:GAIN " +(preamp ? "ON" : "OFF"));
                Send(":INITiate:IMMediate");  //запуск развёртки
                Thread.Sleep(200);//время д.б.не меньше развёртки

                Send("*WAI");  
                Send(":TRACe1:DATA?");
                
               

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

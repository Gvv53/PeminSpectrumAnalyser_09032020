using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace IOMeasurementData
{
    public class CommandsFSH4 : Commands
    {
        public CommandsFSH4()
        {
        }


        public override void Init()
        {
            Send("*RST; *CLS");
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
            //Размер буфера приема(в байтах).Значение по умолчанию — 8192 байта
            byte[] bytes = new byte[newClient.ReceiveBufferSize];
            byte[] bytesRequestResult = new byte[2];


            try
            {
                //Send("FREQ:CENT " + frequency.ToString() + " Hz");
                Send("FREQ:SPAN " + span.ToString() + " Hz");
                if(!RBWAndVBW.VBW)
                     Send("BAND " + band.ToString() + " Hz");

                if (!RBWAndVBW.RBW)
                    Send("BAND:VID " + bandWidth.ToString() + "Hz");

                Send("FREQ:CENT " + frequency.ToString() + " Hz"); //поменяла место

                Send("*OPC?");

                //Send("INIT");
                //Send("*OPC?");

                Thread.Sleep(3000);


                tcpStream.Read(new byte[2], 0, 2);
                

                Send("FORM ASC;:TRAC? TRACE1");
                Thread.Sleep(3000);

                tcpStream.Read(bytes, 0, newClient.Available);

                string str = Encoding.ASCII.GetString(bytes);
                string[] qresult = str.Split(',');

                if (qresult.Count() > 1)
                {
                    double shift = span / (qresult.Count() - 1);
                    int counter = 0;
                    StringBuilder result = new StringBuilder();

                    long start = frequency - (span / 2);

                    foreach (string item in qresult)
                    {
                        if (counter > 0)
                            result.Append(',').Append(start + shift * counter).Append(',').Append(item);
                        else
                            result.Append(start + shift * counter).Append(',').Append(item);

                        counter++;
                    }

                    byte[] byteResult = Encoding.ASCII.GetBytes(result.ToString());

                    return byteResult;
                }
                else
                {
                    throw new Exception();
                }
            }
            catch(Exception ex)
            {
                if (errorCount == 1)
                {
                    MessageBox.Show("Повторная попытка переподключения и повторной посылки данных не удалась. Проверьие соединение с прибором. Попробуйте перезагрузить прибор " 
                            + Environment.NewLine 
                            + ex.ToString());
                    Environment.Exit(0);
                }
            }
            return bytes;
        }
    }
}

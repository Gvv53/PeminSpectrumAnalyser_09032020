using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace IOMeasurementData
{
    public abstract class Commands
    {
        public abstract void Init();

        public abstract byte[] GetDataResult(string traceDetector,
                                      long frequency,
                                      long bandWidth,
                                      long span,
                                      long band,
                                      string traceType,
                                      long attinuation,
                                      bool preamp,
                                      long countTraceMode,
                                      bool isManualSWP,
                                      double ManualSWP,
                                      int errorCount = 0
                                     
                                      );

        public string IP { get; set; }

        public int Port { get; set; }

        public TcpClient newClient { get; set; }
        public NetworkStream tcpStream { get; set; }

        public bool Connect(string ip, int port)
        {
            bool result = false;
            IP = ip;
            Port = port;

            try
            {
                if (newClient != null) newClient?.Close();

                newClient = new TcpClient();

                try
                {
                    newClient.Connect(ip, port);
                    tcpStream = newClient.GetStream();


                    result = true;
                }
                catch
                {
                    newClient?.Close();
                    newClient = new TcpClient();
                    newClient.Connect(ip, port);
                    tcpStream = newClient.GetStream();


                    result = true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("ОШИБКА СОЕДИНЕНИЯ! ПОВТОРНАЯ ПОПЫТКА СОЕДИНЕНИЯ НЕ УДАЛАСЬ!" + Environment.NewLine + Environment.NewLine
                    //+ " ПРИЛОЖЕНИЕ БУДЕТ ЗАКРЫТО " + Environment.NewLine + Environment.NewLine
                    + " Проверьте физическое подключение к прибору " + Environment.NewLine
                    + " Проверьте настройки адреса (параметр IPADDRESS)  и порта прибора (параметр PORTNUMBER)" + Environment.NewLine
                    + "в файле Eureca.Pemin.DataReader.exe.config " + Environment.NewLine + Environment.NewLine
                    + " СЛУЖЕБНАЯ ИНФОРМАЦИЯ " + Environment.NewLine + Environment.NewLine
                    + " Текст исключения: " + ex.ToString()
                    + " Текст внутреннего исключения: " + ex.InnerException?.ToString());

                //Environment.Exit(0); //при неудачном подключении приложение не закрывается
            }

            return result;

        }

        public void Send(string str)
        {
            byte[] sendBytes = Encoding.ASCII.GetBytes(str += Environment.NewLine);
            tcpStream.Write(sendBytes, 0, sendBytes.Length);
        }

    }
}

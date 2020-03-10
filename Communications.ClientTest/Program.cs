using System;

namespace Communications.ClientTest
{
    class Program
    {
        static void Main(string[] args)
        {

            Console.WriteLine("press key for connect");
            Console.ReadKey();

            // В момент создания клиента мое приложение должно уже быть запущено 
            // Связь устанавливается при вызове конструтора
            PIPEClient client = new PIPEClient();


            // Вот так придут данные от меня
            client.IncomingExchangeContract += (data) =>
            {
                Console.WriteLine("New data incoming:  " + data.ID + "  " + data.Description);
            };

            Console.WriteLine("press key for sending");
            Console.ReadKey();


            // А так можно послать данные мне
            for (int counter = 0; counter < 10; counter++)
            client.SendExchangeContract(new ExchangeContract() { ID = 100, Description = "Hello!" });

            Console.WriteLine("press key for exit");
            Console.ReadKey();
        }
    }
}

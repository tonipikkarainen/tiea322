using System;
using System.Net;

namespace MulticastCliLuokalla
{
    class Program
    {
        static void Main(string[] args)
        {
            //Console.WriteLine("Hello World!");
            IPEndPoint sendEp = new IPEndPoint(IPAddress.Parse("239.0.0.1"), 42000);

            bool paalla = true;

            Console.WriteLine("Anna nimesi: ");
            string user_send = Console.ReadLine();

            UdpMultiClient s = new UdpMultiClient(user_send, sendEp);

            int user_send_pit = user_send.Length;

            do
            {
                Console.WriteLine("Lähetä viesti: (enter for receive)");
                Console.Write("> ");
                string teksti_send;
                teksti_send = Console.ReadLine();
                int teksti_send_pit = teksti_send.Length;

                if (teksti_send == "q")
                {
                    paalla = false;
                }
                else
                {
                    if (teksti_send.Length > 0)
                    {
                        s.Send_msg(teksti_send, 1, 3, user_send, sendEp);
                    }
                    while (!Console.KeyAvailable)
                    {
                        try
                        {
                            string[] msg = s.Receive_msg();
                            Console.WriteLine("{0}: {1}", msg[0], msg[1]);
                        }
                        catch (Exception ex)
                        {

                        }

                    }

                }

            } while (paalla);

            s.DropMulticast(user_send,sendEp);
            s.Close();

        }
    }
}

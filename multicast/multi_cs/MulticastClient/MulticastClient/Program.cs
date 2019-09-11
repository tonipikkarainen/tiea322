using System;
using System.Net.Sockets;
using System.Text;
using System.Net;

namespace MulticastClient
{
    /// <summary>
    /// TODO: fiksumpi toteutus multicast udp-luokalla, join ja leave-viestit ryhmään, näissä viestinumero on 1 tai 2
    /// </summary>
    class Program
    {
        static void Main(string[] args)
        {
            bool paalla = true;
            UdpClient s = new UdpClient();
            // Tämä oli tärkeä, että pystyi käyttämään useaa asiakas!
            s.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);

            IPEndPoint localEp = new IPEndPoint(IPAddress.Any, 42000);
            s.Client.Bind(localEp);

            IPEndPoint sendEp = new IPEndPoint(IPAddress.Parse("239.0.0.1"), 42000);

            IPAddress multicast_ip = IPAddress.Parse("239.0.0.1");
            s.JoinMulticastGroup(multicast_ip);

            String user_send;
            int send_versio = 1;
            int send_viesti = 3;
            String app_name = "TIEA322totapikk";
            int day_send = 1;
            int month_send = 5;
            int year_send = 1495;
            int asov_pit_send = app_name.Length;

            Console.WriteLine("Anna nimesi: ");
            user_send = Console.ReadLine();
            int user_send_pit = user_send.Length;

            s.Client.ReceiveTimeout = 200;

            do
            {
                Console.WriteLine("Lähetä viesti: (enter for receive)");
                Console.Write("> ");
                String teksti_send;
                teksti_send = Console.ReadLine();
                int teksti_send_pit = teksti_send.Length;

                if (teksti_send == "q")
                {
                    paalla = false;
                }
                else
                {
                    byte[] data_snd = new byte[256];
                    data_snd[0] = (byte)((send_versio << 4) ^ (send_viesti));
                    data_snd[1] = (byte)((day_send << 3) ^ (month_send >> 1));
                    data_snd[2] = (byte)((month_send << 7) ^ (year_send >> 4));
                    data_snd[3] = (byte)((year_send << 4) ^ (0x0));
                    data_snd[4] = (byte)asov_pit_send;

                    for (int i = 0; i < asov_pit_send; i++)
                    {
                        data_snd[i + 5] = Encoding.UTF8.GetBytes(app_name)[i];
                    }

                    data_snd[5 + asov_pit_send] = (byte)user_send_pit;

                    for (int i = 0; i < user_send_pit; i++)
                    {
                        data_snd[i + 6 + asov_pit_send] = Encoding.UTF8.GetBytes(user_send)[i];
                    }

                    data_snd[5 + asov_pit_send + user_send_pit + 1] = (byte)teksti_send_pit;

                    for (int i = 0; i < teksti_send_pit; i++)
                    {
                        data_snd[i + 5 + asov_pit_send + user_send_pit + 2] = Encoding.UTF8.GetBytes(teksti_send)[i];
                    }
                    s.Send(data_snd, 5 + asov_pit_send + user_send_pit + 3 + teksti_send_pit, sendEp);

                    while (Console.KeyAvailable == false)
                    {
                        try
                        {
                            Byte[] data = s.Receive(ref localEp);


                            // datan tulkinta
                            int versio = (data[0] >> 4) & 0xf;
                            int viesti_nro = (data[0]) & 0xf;
                            int day = (data[1] >> 3) & 0x1f;
                            int month = ((data[1] << 1) & 0xf) ^ (data[2] >> 7);
                            int year = ((data[2] & 0x7f) << 4) ^ (data[3] >> 4);
                            int asov_pit = data[4];
                            String asov = Encoding.UTF8.GetString(data, 5, asov_pit);
                            int user_pit = data[5 + asov_pit];
                            String user = Encoding.UTF8.GetString(data, 6 + asov_pit, user_pit);
                            int teksti_pit = data[5 + asov_pit + user_pit + 1];
                            String teksti = Encoding.UTF8.GetString(data, 5 + asov_pit + user_pit + 2, teksti_pit);

                            Console.WriteLine("{0}: {1}", user, teksti);
                        }
                        catch
                        {

                        }

                    }
                }

            } while (paalla);
          
            s.DropMulticastGroup(multicast_ip);
            s.Close();
        }
    }
}

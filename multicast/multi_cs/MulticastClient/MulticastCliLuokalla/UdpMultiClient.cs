using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace MulticastCliLuokalla
{
    internal class UdpMultiClient:UdpClient
    {
        // Lähetystiedot
        private string app_name = "TIEA322totapikk";
        private int day_send = 1;
        private int month_send = 5;
        private int year_send = 1495;
        private string ip = "239.0.0.1";
        private IPEndPoint localEp = new IPEndPoint(IPAddress.Any, 42000);
        private List<string> users = new List<string>();

        internal void DropMulticast(string user_send, IPEndPoint sendEp)
        {
            this.Send_msg("/leave", 1, 2, user_send, sendEp);
            this.DropMulticastGroup(IPAddress.Parse(this.ip));
        }


        internal void Send_msg(string teksti_send, int send_versio, int send_viesti, string user_send, IPEndPoint sendEp)
        {
            //throw new NotImplementedException();
            byte[] data_snd = new byte[256];
            data_snd[0] = (byte)((send_versio << 4) ^ (send_viesti));
            data_snd[1] = (byte)((day_send << 3) ^ (month_send >> 1));
            data_snd[2] = (byte)((month_send << 7) ^ (year_send >> 4));
            data_snd[3] = (byte)((year_send << 4) ^ (0x0));
            data_snd[4] = (byte)this.app_name.Length;

            addToData(this.app_name, data_snd, 5);

           
            data_snd[5 + this.app_name.Length] = (byte)user_send.Length;
            addToData(user_send, data_snd, 6 + this.app_name.Length);
           
            data_snd[5 + this.app_name.Length + user_send.Length + 1] = (byte)teksti_send.Length;

            addToData(teksti_send, data_snd, 5 + this.app_name.Length + user_send.Length + 2);
            
            this.Send(data_snd, 5 + this.app_name.Length + user_send.Length + 3 + teksti_send.Length, sendEp);
        }

        private void addToData(string v2, byte[] data_snd, int a)
        {
            for (int i = 0; i < v2.Length; i++)
            {
                data_snd[i + a] = Encoding.UTF8.GetBytes(v2)[i];
            }
        }

        public UdpMultiClient(string user_send, IPEndPoint sendEp)
        {
            this.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            this.Client.Bind(this.localEp);
            //IPEndPoint sendEp = new IPEndPoint(IPAddress.Parse("239.0.0.1"), 42000);
            IPAddress multicast_ip = IPAddress.Parse(this.ip);
            this.JoinMulticastGroup(multicast_ip);
            // Lähetä join-viesti?
            this.Send_msg("/join", 1, 1, user_send, sendEp);

            this.Client.ReceiveTimeout = 200;
        }

        public string[] Receive_msg()
        {
            
            Byte[] data = this.Receive(ref this.localEp);

            // datan tulkinta -- tähän tulkinta myös join ja leave -viesteistä + käyttäjän lisäys listaan.

            int versio = (data[0] >> 4) & 0xf;
            int viesti_nro = (data[0]) & 0xf;
            if (versio >= 1 && (1 <= viesti_nro && viesti_nro <= 3))
            {
                string[] msg = new string[2];
                int day = (data[1] >> 3) & 0x1f;
                int month = ((data[1] << 1) & 0xf) ^ (data[2] >> 7);
                int year = ((data[2] & 0x7f) << 4) ^ (data[3] >> 4);
                int asov_pit = data[4];
                string asov = Encoding.UTF8.GetString(data, 5, asov_pit);
                int user_pit = data[5 + asov_pit];
                string user = Encoding.UTF8.GetString(data, 6 + asov_pit, user_pit);
                int teksti_pit = data[5 + asov_pit + user_pit + 1];
                string teksti = Encoding.UTF8.GetString(data, 5 + asov_pit + user_pit + 2, teksti_pit);
                msg[0] = user;
                msg[1] = teksti;
                //if(teksti == "/join"){
                // users.Append(user) };
                return msg;
            }
            else
            {
                throw new Exception("Väärät arvot");
            }


        }


       
    }
}
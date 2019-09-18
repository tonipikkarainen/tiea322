using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Timers;


namespace MulticastV2
{
    internal class UdpMultiClientV2:UdpClient
    {
        // Lähetystiedot
        private string app_name = "TIEA322totapikk";
        private int day_send = 1;
        private string user_name;
        private int month_send = 5;
        private int year_send = 1495;
        private string ip = "239.0.0.1";
        private List<string> nimet_vrt=new List<string>();

        private IPEndPoint localEp = new IPEndPoint(IPAddress.Any, 42000);
        private List<string> users = new List<string>();
        private Timer timer = new Timer();
        private IPEndPoint sendEp_loc;

        internal void DropMulticast(string user_send, IPEndPoint sendEp)
        {
            this.Send_msg("/leave", 2, 2, user_send, sendEp);
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

            addToData(this.app_name, ref data_snd, 5);
            data_snd[5 + this.app_name.Length] = (byte)user_send.Length;
            addToData(user_send, ref data_snd, 6 + this.app_name.Length);
           
            data_snd[5 + this.app_name.Length + user_send.Length + 1] = (byte)teksti_send.Length;

            addToData(teksti_send, ref data_snd, 5 + this.app_name.Length + user_send.Length + 2);
            
            this.Send(data_snd, 5 + this.app_name.Length + user_send.Length + 3 + teksti_send.Length, sendEp);
        }

        private void addToData(string v2, ref byte[] data_snd, int a)
        {
            for (int i = 0; i < v2.Length; i++)
            {
                data_snd[i + a] = Encoding.UTF8.GetBytes(v2)[i];
            }
        }

        public UdpMultiClientV2(string user_send, IPEndPoint sendEp)
        {
            this.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            this.Client.Bind(this.localEp);
            this.user_name = user_send;
            //IPEndPoint sendEp = new IPEndPoint(IPAddress.Parse("239.0.0.1"), 42000);
            IPAddress multicast_ip = IPAddress.Parse(this.ip);
            this.JoinMulticastGroup(multicast_ip);
            // Lähetä join-viesti?
            this.sendEp_loc = sendEp;
            this.Send_msg("/join", 2, 1, user_send, sendEp);
            this.timer.AutoReset = false;
          
            this.Client.ReceiveTimeout = 100;
            // Mitä tapahtuu kun timer laukeaa?
            // this.timer.Elapsed
        }

        public string[] Receive_msg()
        {
            
            Byte[] data = this.Receive(ref this.localEp);

            // datan tulkinta -- tähän tulkinta myös join ja leave -viesteistä + käyttäjän lisäys listaan.

            int versio = (data[0] >> 4) & 0xf;
            int viesti_nro = (data[0]) & 0xf;
            if (viesti_nro == 4)
            {
                /*if (this.timer.Enabled)
                {
                    this.timer.Enabled = false;
                }*/
                timer.Stop();
              
                string[] nimet = dataOtsikkosta(data);

               
                for (int i = 0; i < nimet.Length; i++)
                {
                    if(!this.users.Contains(nimet[i])) { users.Add(nimet[i]); }
                }
                       
                

                // miten tähän tehdään sellainen, että jätetään viesti huomiotta
                // jos sama lista on tullut joltain muulta??
                /// nyt periaatteessa toimii, mutta kun tulee usea update välillä, niin tulostaa monta kertaa
                /// omat userssit..

                //return new string[0];
                //var sorted = this.nimet_vrt.OrderBy((string arg) => x);
                //Array.Sort(this.nimet_vrt);
                //Array.Sort(nimet);
                
                if (nimet_vrt.Count != nimet.Length){
                    for (int i = 0; i < users.Count; i++)
                    {
                        if (!this.nimet_vrt.Contains(users[i])) { nimet_vrt.Add(users[i]); }
                       
                    }
                    return this.users.ToArray();
                }

                return new string[0];
            }
            else if(versio >= 1 && (1 <= viesti_nro && viesti_nro <= 3))
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
                if (teksti == "/join")
                {
                    this.users.Add(user);
                    if (!((this.users.Count == 1) && (this.users[0] == this.user_name)))
                    {
                        Random rand = new Random();
                        this.timer.Interval = rand.Next(500, 5000);
                        this.timer.Elapsed += Send_update;
                        this.timer.Enabled = true;
                        
                        
                    }

                }
                if(teksti == "/leave")
                {
                    this.users.Remove(user);
                    this.nimet_vrt.Remove(user);
                }
                // users.Append(user) };
                return msg;
            }
            else
            {
                throw new Exception("Väärät arvot");
            }


        }

        private void Send_update(Object source, ElapsedEventArgs e)
        {
            //throw new NotImplementedException();
            string[] nimet = this.users.ToArray();
            byte[] data_snd = asetaTavut(2,4,this.day_send, this.month_send, this.year_send, this.app_name, this.user_name, nimet);

            this.Send(data_snd, data_snd.Length, this.sendEp_loc); 
            //Console.WriteLine("The Elapsed event was raised at {0}", e.SignalTime);
        }


        public byte[] asetaTavut(int versio, int viesti,
                                    int day, int month, int year,
                                    string asiakasnimi,
                                    string usernimi, string[] nimet)
        {
            // selvitä string-kenttien pituudet UTF-8 tavuina
            int clientLength = asiakasnimi.Length; // UTF-8 koodattujen tavujen määrä
            int userLength = usernimi.Length; // UTF-8 koodattujen tavujen määrä
                                              // dataLength pitää olla kaikkien nimien yhteispituus + nimien lukumäärä
                                              // Nimet UTF-8 koodattuina, joita ennen yhden tavun kenttä, joka
                                              // määrittää nimen vaatimien tavujen lukumäärän

            int dataLength = 0;
            for (int i = 0; i < nimet.Length; i++)
            {
                dataLength = nimet[i].Length + dataLength;
            }
            dataLength = dataLength + nimet.Length;
            // Otsikon vakiopituiset kentät on 7 tavua
            int constLength = 7;
            byte[] tavut = new byte[constLength + clientLength + userLength + dataLength];
            tavut[0] = (byte)((versio << 4) ^ (viesti));
            tavut[1] = (byte)((day << 3) ^ (month >> 1));
            tavut[2] = (byte)((month << 7) ^ (year >> 4));
            tavut[3] = (byte)((year << 4) ^ (0x0));
            tavut[4] = (byte)clientLength;
            addToData(asiakasnimi, ref tavut, 5);
            tavut[5 + clientLength] = (byte)usernimi.Length;

            addToData(usernimi, ref tavut, 6 + clientLength);

            tavut[6 + clientLength + userLength] = (byte)dataLength;
            int index = 6 + clientLength + userLength + 1;
            for (int i = 0; i < nimet.Length; i++)
            {
                tavut[index] = (byte)nimet[i].Length;
                addToData(nimet[i], ref tavut, index + 1);
                index = index + nimet[i].Length + 1;
            }

            return tavut;
        }

        public string[] dataOtsikkosta(byte[] tavut)
        {
            var nimetList = new List<string>();
            if (tavut.Length > 0)
            {
                int asov_pit = tavut[4];
                int user_pit = tavut[5 + asov_pit];
                int teksti_pit = tavut[5 + asov_pit + user_pit + 1];
                int index = 5 + asov_pit + user_pit + 2;
                bool paalla = true;
                while (paalla)
                {
                    nimetList.Add(Encoding.UTF8.GetString(tavut, index + 1, tavut[index]));
                    index = index + tavut[index] + 1;
                    if (index >= tavut.Length) { paalla = false; }
                    else if (tavut[index] == 0) { paalla = false; }

                }
            }

            string[] nimet = nimetList.ToArray();
            return nimet;
        }

    }
}
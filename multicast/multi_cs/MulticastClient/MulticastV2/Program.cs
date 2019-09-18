using System;
using System.Net;

// On vielä vähän hasardi, jos tekee liittymisen liian nopeasti alussa. Silloin saattaa jättää
// jonkun huomiotta. Eli esim. 2:lle ei tulekaan ollenkaan tietoa ykkösestä.
// 1 liittyy lähettää /join. 2 ja 3 liittyy -> 1 saa joinin siltä ja 3:lta. Kakkonen
// saa joinin kolmoselta ja kerkeää lähettä updaten. Jossa on 2 ja 3. Nyt tämä update saattaa
// laukaista ykkösen ajastimen, jolloin ykkönen ei pääse lähettämään listaa, joka sillä on.
// näin 2:lta ja 3:lta jää 1 saamatta omaan listaansa. Miten voi estää???!!!??? Pitäisikö, jonkun lähettää update,
// jos se saa updaten, joka ei ole yhtä täydellinen kuin sillä itsellään (...)
namespace MulticastV2
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

            UdpMultiClientV2 s = new UdpMultiClientV2(user_send, sendEp);

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
                        s.Send_msg(teksti_send, 2, 3, user_send, sendEp);
                    }
                    while (!Console.KeyAvailable)
                    {
                        try
                        {
                            // tää vähän kökkö, nyt tulostaa kahden käyttäjän listan muodossa k1: k2 ... :) myöhemmin...
                            string[] msg = s.Receive_msg();
                            if (msg.Length == 2)
                            {
                                Console.WriteLine("{0}: {1}", msg[0], msg[1]);
                            }
                            else if(msg.Length > 0 )
                            {
                                for (int i = 0; i < msg.Length; i++)
                                {
                                    Console.WriteLine(msg[i]);
                                }
                            }
                            
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


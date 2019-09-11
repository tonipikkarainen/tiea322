using System;
using System.Text;
using System.Globalization;
public class test
{
    public static void Main()
    {
        byte[] tavut = asetaTavut(1, 3, 24, 11, 1971, "TIEA322", "totapikk", "Hello TIEA322");

        string heksa = ByteArrayToString(tavut);
        System.Console.WriteLine("Arvoilla versio=1, viesti=3, day=24, month=11, year=1971,\r\n asiakasnimi=TIEA322, usernimi=totapikk, teksti=Hello TIEA322,\r\nkoodisi antaa:\r\n {0}", heksa);

        //System.Console.WriteLine(tavut.ToString());
    }
    /// <summary>
    /// Funktio palauttaa tavu-taulukon, missä on Multicastchat protokollan
    /// kehysrakenteen kenttien mukaiset informaatiot
    /// </summary>
    /// <returns>parametreista muodostetut tavut</returns>
    public static byte[] asetaTavut(int versio, int viesti,
                                    int day, int month, int year,
                                    string asiakasnimi,
                                    string usernimi, string teksti)
    {
        // selvitä string-kenttien pituudet UTF-8 tavuina
        int clientLength = System.Text.Encoding.UTF8.GetBytes(asiakasnimi).Length; // UTF-8 koodattujen tavujen määrä
        int userLength = System.Text.Encoding.UTF8.GetBytes(usernimi).Length; // UTF-8 koodattujen tavujen määrä
        int dataLength = System.Text.Encoding.UTF8.GetBytes(teksti).Length; // UTF-8 koodattujen tavujen määrä
        // Otsikon vakiopituiset kentät on 7 tavua
        int constLength = 7;
        byte[] tavut = new byte[constLength + clientLength + userLength + dataLength];
        // Toteuta
        tavut[0] = (byte)(((versio & 0xF) << 4) ^ (viesti & 0xF));
        tavut[1] = (byte)((day & 0x1f) << 3);
        tavut[1] = (byte)(((month & 0xf) >> 1) ^ tavut[1]);
        tavut[2] = (byte)(((month & 0xf) << 7) ^ ((year >> 4) & 0x7F));
        tavut[3] = (byte)((year << 4));
        tavut[4] = (byte)clientLength;

        byte[] bytes_asiakas = System.Text.Encoding.UTF8.GetBytes(asiakasnimi);
        byte[] bytes_usernimi = System.Text.Encoding.UTF8.GetBytes(usernimi);
        byte[] bytes_teksti = System.Text.Encoding.UTF8.GetBytes(teksti);

        for (int i = 0; i < clientLength; i++)
        {
            tavut[5 + i] = bytes_asiakas[i];
        }

        int nimi_loc = 5 + clientLength+1;
        tavut[5 + clientLength] = (byte)userLength;

        for (int i = 0; i < userLength; i++)
        {
            tavut[nimi_loc + i] = bytes_usernimi[i];
        }

        int teksti_loc = nimi_loc + userLength + 1;
        tavut[nimi_loc + userLength] = (byte)dataLength;

        for (int i = 0; i < dataLength; i++)
        {
            tavut[teksti_loc + i] = bytes_teksti[i];
        }


        return tavut;
    }

    public static string ByteArrayToString(byte[] ba)
    {
        StringBuilder hex = new StringBuilder(ba.Length * 2);
        foreach (byte b in ba)
            hex.AppendFormat("{0:x2}", b);
        return hex.ToString();
    }

}
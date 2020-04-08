using System;

namespace UI2
{
    class Program
    {
        static void Main(string[] args)
        {
            int pocetClenuVGeneraci = 100;
            int pocetGeneraci = 3000;
            int kolikatyPrvekZobrazovat = 1000;
            HledaniCest hledaniCest = new HledaniCest();
            hledaniCest.VygenerujPrvniGeneraci(pocetClenuVGeneraci);                        //Generuje prvni rodice
            hledaniCest.VygenerujDalsiGenerace(pocetGeneraci, kolikatyPrvekZobrazovat);     //Generuje dalsi rodice       
        }
    }
}

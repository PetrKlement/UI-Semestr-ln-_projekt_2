using System;
using System.Collections.Generic;
using System.IO;
using System.Data;
using ExcelDataReader;

namespace UI2
{
    class HledaniCest
    {
        public List<string> JednorozmernyListStringu = new List<string>();
        private double[,] VDouble = new double[35, 35];
        private double[,] VzdalenostiNaZpracovani = new double[35, 35];
        public List<Mista> seznamMist = new List<Mista>();                                    
        private double[] pomocnePole = new double[35];
        public List<Trasa> trasaList = new List<Trasa>();                                     // Hlavni list tras
        public List<Trasa> pomocnyTrasaList = new List<Trasa>();                              // Slouzi ke zpracovani v jednotlivych krocich
        public List<Mista> ulozeniDoTrasy = new List<Mista>();
        public List<Mista> ulozeni = new List<Mista>();
        public Trasa nejkratsiTrasa= new Trasa();
        private Random random;
        public int generaceNejkratsiTrasy = 0;

        public HledaniCest()
        {
            random = new Random();
        }


        public void VygenerujPrvniGeneraci(int pocetClenuVGeneraci)
        {
            string MistoUlozeni = @"d:\Distance.xlsx";                                              // String popisuje cestu k souboru
            using (var stream = File.Open(MistoUlozeni, FileMode.Open, FileAccess.Read))            // Otevření souboru  / založení var stream      
            {
                using (IExcelDataReader reader = ExcelReaderFactory.CreateReader(stream))           // Pokus o otevření
                {
                    DataSet result = reader.AsDataSet(new ExcelDataSetConfiguration()
                    {
                        ConfigureDataTable = (_) => new ExcelDataTableConfiguration() { UseHeaderRow = false }
                    });
                    DataTableCollection excel = result.Tables;

                    foreach (DataTable jednotlivePole in excel)                                     // prochazi jednotliva pole v excelu
                    {
                        for (int c = 1; c < jednotlivePole.Columns.Count; c++)                      // tady si načtu list stringu pro pozdejsi porovnani     
                        {
                            var p = jednotlivePole.Rows[0][c];
                            string s = p.ToString();
                            JednorozmernyListStringu.Add(s);
                        }
                        for (int a = 1; a < jednotlivePole.Rows.Count; a++)                         // 1 je tam kvůli ořezu 1. sloupce a 1. řádku
                        {
                            for (int b = 1; b < jednotlivePole.Columns.Count; b++)
                            {
                                var pomocna = jednotlivePole.Rows[a][b];
                                string slova = pomocna.ToString();
                                JednorozmernyListStringu.Add(slova);
                                double.TryParse(slova, out double number);
                                VDouble[b - 1, a - 1] = number;
                            }
                        }
                    }
                }
            }
            //-----------------------------------------------------------Naplni seznam mist

            for (int a = 0; a < 35; a++)
            {
                for (int b = 0; b < 35; b++)
                {
                    if (VDouble[a, b] != 0)
                        VzdalenostiNaZpracovani[a, b] = VDouble[a, b];
                    else
                        VzdalenostiNaZpracovani[a, b] = VDouble[b, a];      // slouzi k doplneni prazdnych míst
                }
            }

            for (int i = 0; i < 35; i++)
            {
                for (int j = 0; j < 35; j++)
                {
                    pomocnePole[j] = VzdalenostiNaZpracovani[j, i];
                }
                seznamMist.Add(new Mista(JednorozmernyListStringu[i], pomocnePole)); 
                pomocnePole = new double[35];
            }

            // --------------------------------- ------------------------------------- slouzi k naplneni trasaList

            for (int a = 0; a < pocetClenuVGeneraci; a++)                            // a je menší než počet jedinců v generaci
            {
                foreach (Mista m in seznamMist)
                    ulozeni.Add(m);                                            // udela odkaz na dalsi List - ulozeni
                for (int i = 0; i < 35; i++)                                       // Dokud neni prazdny je prochazen
                {
                    int fff = random.Next(ulozeni.Count);                       // nahodne vybereme cislo do mnozstvy zbyvajicich mist
                    ulozeniDoTrasy.Add(ulozeni[fff]);                           // a ulozi
                    ulozeni.RemoveAt(fff);                                      // pouzite smaze
                }
                trasaList.Add(new Trasa(ulozeniDoTrasy));                   //1. generace trasy
                ulozeni = new List<Mista>();
                ulozeniDoTrasy = new List<Mista>();
            }

            // -------------------------------------------------------------------- Urceni celkove vzdalenosti

            Console.WriteLine("Délka cest první generace.");
            int polohaPrvniho = 0;
            int polohaDruheho = 0;
            double coUraziCestujici;
            foreach (Trasa tras in trasaList)                     //Rozebrani na jednotlive trasy  xkrát
            {
                coUraziCestujici = 0;
                for (int t = 0; t < 34; t++)            // zde se prochazi mesta za sebou a mezi vedlejsimy mesty se urci vzdalenost
                {
                    string stringPrvniho = tras.naseTrasa[t].jmenomesta;      // ulozeny jmena prvniho mesta
                    string stringDruheho = tras.naseTrasa[t + 1].jmenomesta;    // ulozeny jmena druheho mesta
                    for (int tt = 0; tt < 35; tt++)
                    {
                        if (stringPrvniho == JednorozmernyListStringu[tt])    // prohleda List stringu se jmeny vsech mest a najde pozici prvniho
                        {
                            polohaPrvniho = tt;
                        }
                        if (stringDruheho == JednorozmernyListStringu[tt])   // prohleda List stringu se jmeny vsech mest a najde pozici druheho
                        {
                            polohaDruheho = tt;
                        }
                    }
                    coUraziCestujici += seznamMist[polohaDruheho].vzdalenosti[polohaPrvniho];      // coUraziCestujici se inkrementuje o vzdalenost mezi mesty
                }
                pomocnyTrasaList.Add(new Trasa(tras.naseTrasa, coUraziCestujici));     // vznik ohodnoceneho pole tras
            }
            trasaList = new List<Trasa>();
            Console.WriteLine("První generace: \n");
            foreach (Trasa t in pomocnyTrasaList)
            {
                trasaList.Add(new Trasa(t.naseTrasa, t.delkaCesty));

            }
            foreach (Trasa t in pomocnyTrasaList)
            {
                Console.Write( t.delkaCesty + " - ");
            }
            Console.WriteLine("\n\n\n\n\n");
            //----------------------------------------------------------------------------- Zde probiha selekce
            pomocnyTrasaList = new List<Trasa>();                                                       
            double celkovyVyberZRulety = 0;     // znulovani celkovyVyberZRulety
            celkovyVyberZRulety = 0;
            foreach (Trasa t in trasaList)
            {
                celkovyVyberZRulety += (1 / (t.delkaCesty));          // zde si scitam jednotlive podily
            }
            double aa = (celkovyVyberZRulety * 1000000000);            // vynasobim
            int rozsah = (int)(aa);                                          // prevedu na int
            for (int a = 0; a < trasaList.Count; a++)            // udelam pro vsechny 
            {
                int rand = random.Next(rozsah);                              // vyberu random
                double randNaMinusPrnvi = ((double)rand / 1000000000);    // zpracuju
                double vyberZRulety = 0;
                foreach (Trasa t in trasaList)
                {
                    vyberZRulety += (1 / (t.delkaCesty));
                    if ((celkovyVyberZRulety - vyberZRulety) <= (randNaMinusPrnvi))
                    {
                        // ohodnocenyTrasaListProSelekci.Add(t);
                        pomocnyTrasaList.Add(new Trasa(t.naseTrasa));    //vytvoreni Listu selektovanych tras
                        break;                                           // je-li podminka splnena skoci na foreach
                    }
                }
            }
            trasaList = new List<Trasa>();
            foreach(Trasa t in pomocnyTrasaList )
            {
                trasaList.Add(new Trasa(t.naseTrasa));
            }

            // ------------------------------------------------------------------ Zde probiha krizeni

            pomocnyTrasaList = new List<Trasa>();                           // vynulovani listu 
            List<Mista> prvniDuplicita = new List<Mista>();                 // listy dulicitnich mist na trase
            List<Mista> druhaDuplicita = new List<Mista>();
            for (int a = 0; a < trasaList.Count; a += 2)
            {
                int randomDelelic = random.Next(2, 33);        
                int zbytekDelice = 35 - randomDelelic;                        
                prvniDuplicita = new List<Mista>();                         // vynulovani
                druhaDuplicita = new List<Mista>();
                List<Mista> prvniPotomek = new List<Mista>();               // priprava potomku
                List<Mista> druhyPotomek = new List<Mista>();
                List<int> prvniPolohyDuplicity = new List<int>();           // pro polohy duplicit
                List<int> druhePolohyDuplicity = new List<int>();
                // vytvoreni 1. potomku s duplicitama
                prvniPotomek.AddRange(trasaList[a].naseTrasa);
                prvniPotomek.RemoveRange(randomDelelic, zbytekDelice);
                prvniPotomek.AddRange(trasaList[a + 1].naseTrasa);
                prvniPotomek.RemoveRange(randomDelelic, randomDelelic);
                // vytvoreni 2. potomku s duplicitama
                druhyPotomek.AddRange(trasaList[a + 1].naseTrasa);
                druhyPotomek.RemoveRange(randomDelelic, zbytekDelice);
                druhyPotomek.AddRange(trasaList[a].naseTrasa);
                druhyPotomek.RemoveRange(randomDelelic, randomDelelic);

                for (int b = 0; b < randomDelelic; b++)
                {
                    for (int c = randomDelelic; c < 35; c++)               // ulozili jsme si dup mista a jejich polohu
                    {
                        // ulozim si duplicitni mista a jejich polohu v 1. potomku
                        if (prvniPotomek[b].jmenomesta == prvniPotomek[c].jmenomesta)
                        {
                            prvniDuplicita.Add(prvniPotomek[b]);
                            prvniPolohyDuplicity.Add(b);
                        }
                    }
                    for (int c = randomDelelic; c < 35; c++)
                    {
                        // ulozim si duplicitni mista a jejich polohu v 2. potomku
                        if (druhyPotomek[b].jmenomesta == druhyPotomek[c].jmenomesta)
                        {
                            druhaDuplicita.Add(druhyPotomek[b]);
                            druhePolohyDuplicity.Add(b);
                        }
                    }
                }

                for (int r = 0; r < prvniPolohyDuplicity.Count; r++)
                {
                    // v potomcich se vymeni dupicitni mista
                    druhyPotomek[druhePolohyDuplicity[r]] = prvniDuplicita[r];
                    prvniPotomek[prvniPolohyDuplicity[r]] = druhaDuplicita[r];
                }
                pomocnyTrasaList.Add(new Trasa( prvniPotomek));               // do listu se prida potomek
                pomocnyTrasaList.Add(new Trasa( druhyPotomek));
            }
            trasaList = new List<Trasa>();
            foreach (Trasa t in pomocnyTrasaList)
            {
                trasaList.Add(new Trasa(t.naseTrasa));
            }
            pomocnyTrasaList = new List<Trasa>();

            // ----------------------------------------------------------------- Zde probiha mutace

            int poziceProMutaci = 0;
            foreach (Trasa tras in trasaList)
            {
                for (int i = 0; i < 35; i++)
                {
                    Mista mistaKVymeneRand = new Mista();
                    Mista mistaKVymene = new Mista();
                    int ran = random.Next(trasaList[1].naseTrasa.Count);            // Do 35 
                    poziceProMutaci++;
                    if (poziceProMutaci == 200)                                     // urcuje kolikate misto v trase zmutuje
                    {
                        if (ran != i)                                               // pokud random neni stejna pozice
                        {
                            mistaKVymeneRand = tras.naseTrasa[ran];                 // ulozim nahodne misto
                            mistaKVymene = tras.naseTrasa[i];                       // ulozim misto i
                            tras.naseTrasa[ran] = mistaKVymene;                     // vymenim je mezi sebou
                            tras.naseTrasa[i] = mistaKVymeneRand;
                            poziceProMutaci = 0;                                    // vyniluju si pozici pro mutaci
                        }
                        else
                            poziceProMutaci = 0;
                    }
                }
            }
        }


        public void VygenerujDalsiGenerace(int pocetgeneraci, int kolikatyZobrazit)                 // Vytvareni dalsich generaci
        {
            for(int pocetOpakovani = 1; pocetOpakovani <= pocetgeneraci; pocetOpakovani++)
            {
                if (pocetOpakovani == 1)
                    nejkratsiTrasa = new Trasa(trasaList[0].naseTrasa, 10000);                  // Cislo vetsi nez predpokladane minimum

                // -------------------------------------------------------------------- Urceni celkove vzdalenosti

                pomocnyTrasaList = new List<Trasa>();
                int polohaPrvniho = 0;
                int polohaDruheho = 0;
                foreach (Trasa ttt in trasaList)                     //Rozebrani na jednotlive trasy  
                {
                    double coUraziCestujici = 0;
                    for (int t = 0; t < 34; t++)             
                    {
                        string stringPrvniho = ttt.naseTrasa[t].jmenomesta;
                        string stringDruheho = ttt.naseTrasa[t + 1].jmenomesta;
                        for (int tt = 0; tt < 35; tt++)
                        {
                            if (stringPrvniho == JednorozmernyListStringu[tt])
                            {
                                polohaPrvniho = tt;
                            }
                            if (stringDruheho == JednorozmernyListStringu[tt])
                            {
                                polohaDruheho = tt;
                            }
                        }
                        coUraziCestujici += seznamMist[polohaDruheho].vzdalenosti[polohaPrvniho];
                    }
                    pomocnyTrasaList.Add(new Trasa(ttt.naseTrasa, coUraziCestujici));     // vznik ohodnoceneho pole tras
                }
                trasaList = new List<Trasa>();
                foreach (Trasa t in pomocnyTrasaList)
                {
                    trasaList.Add(new Trasa(t.naseTrasa, t.delkaCesty));
                    if (nejkratsiTrasa.delkaCesty > t.delkaCesty)
                    {
                        nejkratsiTrasa = new Trasa( t.naseTrasa, t.delkaCesty);
                        generaceNejkratsiTrasy = pocetOpakovani;
                    }
                }

                // ---------------------------------------------------------------- Vypis pravidelne generace

                if (pocetOpakovani % kolikatyZobrazit == 0)
                {
                    Trasa nejkratsiTrasaVDaneGeneraci = trasaList[0];
                    double sumaCelku = 0;
                    double prumernaCesta = 0;
                    Console.WriteLine("----------------------------------------------------------------------------------------------------------");
                    Console.WriteLine("     ---------- Výpis {0}- te generace ----------     ", pocetOpakovani);
                    foreach (Trasa t in trasaList)
                    {
                        sumaCelku += t.delkaCesty;
                        if (t.delkaCesty < nejkratsiTrasaVDaneGeneraci.delkaCesty)
                            nejkratsiTrasaVDaneGeneraci = t;
                    }
                    prumernaCesta = (sumaCelku / trasaList.Count) + (sumaCelku % trasaList.Count);
                    Console.WriteLine(" Průměrná délka cesty v generaci je {0} km. ", prumernaCesta);
                    Console.WriteLine(" Nejkratší délka cesty v dané generaci je {0} km.\n Jeho trasa je : \n",nejkratsiTrasaVDaneGeneraci.delkaCesty);
                    foreach(Mista m in nejkratsiTrasaVDaneGeneraci.naseTrasa)
                    {
                        Console.Write(" {0} - ",m.jmenomesta);
                    }
                    Console.WriteLine("\n----------------------------------------------------------------------------------------------------------");
                    Console.WriteLine("\n\n\n\n\n");
                }

                //----------------------------------------------------------------------------- Zde probiha selekce
                pomocnyTrasaList = new List<Trasa>();         

                double celkovyVyberZRulety;     
                celkovyVyberZRulety = 0;                // znulovani celkovyVyberZRulety
                foreach (Trasa t in trasaList)
                {
                    celkovyVyberZRulety += (1 / (t.delkaCesty));          // zde si scitam jednotlive podily
                }
                double aa = (celkovyVyberZRulety * 1000000000);            // vynasobim
                int rozsah = (int)(aa);                                          // prevedu na int
                for (int a = 0; a < trasaList.Count; a++)            // udelam pro vsechny 
                {
                    int rand = random.Next(rozsah);                              // vyberu random
                    double randNaMinusPrnvi = ((double)rand / 1000000000);    // zpracuju                                             
                    double vyberZRulety = 0;
                    foreach (Trasa t in trasaList)
                    {
                        vyberZRulety += (1 / (t.delkaCesty));
                        if ((celkovyVyberZRulety - vyberZRulety) <= (randNaMinusPrnvi))
                        {
                            pomocnyTrasaList.Add(new Trasa(t.naseTrasa));    //vytvoreni Listu selektovanych tras
                            break;                                     // je-li podminka splnena skoci na foreach
                        }
                    }
                }
                trasaList = new List<Trasa>();
                foreach (Trasa t in pomocnyTrasaList)
                {
                    trasaList.Add(new Trasa(t.naseTrasa));
                }

                // ------------------------------------------------------------------ Zde probiha krizeni

                pomocnyTrasaList = new List<Trasa>();                           // vymazani listu 
                for (int a = 0; a < trasaList.Count; a += 2)
                {
                    int randomDelelic = random.Next(2, 33);
                    int zbytekDelice = 35 - randomDelelic;
                    List<Mista> prvniDuplicita = new List<Mista>();                         // vynulovani
                    List<Mista> druhaDuplicita = new List<Mista>();
                    List<Mista> prvniPotomek = new List<Mista>();               // priprava potomku
                    List<Mista> druhyPotomek = new List<Mista>();
                    List<int> prvniPolohyDuplicity = new List<int>();           // pro polohy duplicit
                    List<int> druhePolohyDuplicity = new List<int>();
                    // vytvoreni 1. potomku s duplicitama
                    prvniPotomek.AddRange(trasaList[a].naseTrasa);
                    prvniPotomek.RemoveRange(randomDelelic, zbytekDelice);
                    prvniPotomek.AddRange(trasaList[a + 1].naseTrasa);
                    prvniPotomek.RemoveRange(randomDelelic, randomDelelic);
                    // vytvoreni 2. potomku s duplicitama
                    druhyPotomek.AddRange(trasaList[a + 1].naseTrasa);
                    druhyPotomek.RemoveRange(randomDelelic, zbytekDelice);
                    druhyPotomek.AddRange(trasaList[a].naseTrasa);
                    druhyPotomek.RemoveRange(randomDelelic, randomDelelic);

                    for (int b = 0; b < randomDelelic; b++)
                    {
                        for (int c = randomDelelic; c < 35; c++)               // ulozili jsme si dup mista a jejich polohu
                        {
                            // ulozim si duplicitni mista a jejich polohu v 1. potomku
                            if (prvniPotomek[b].jmenomesta == prvniPotomek[c].jmenomesta)
                            {
                                prvniDuplicita.Add(prvniPotomek[b]);
                                prvniPolohyDuplicity.Add(b);
                            }
                        }
                        for (int c = randomDelelic; c < 35; c++)
                        {
                            // ulozim si duplicitni mista a jejich polohu v 2. potomku
                            if (druhyPotomek[b].jmenomesta == druhyPotomek[c].jmenomesta)
                            {
                                druhaDuplicita.Add(druhyPotomek[b]);
                                druhePolohyDuplicity.Add(b);
                            }
                        }
                    }

                    for (int r = 0; r < prvniPolohyDuplicity.Count; r++)
                    {
                        // v potomcich se vymeni dupicitni mista
                        druhyPotomek[druhePolohyDuplicity[r]] = prvniDuplicita[r];
                        prvniPotomek[prvniPolohyDuplicity[r]] = druhaDuplicita[r];
                    }
                    pomocnyTrasaList.Add(new Trasa(prvniPotomek));               // do listu se prida potomek
                    pomocnyTrasaList.Add(new Trasa( druhyPotomek));
                }
                trasaList = new List<Trasa>();
                foreach (Trasa t in pomocnyTrasaList)
                {
                    trasaList.Add(new Trasa(t.naseTrasa));
                }
                pomocnyTrasaList = new List<Trasa>();

                // ----------------------------------------------------------------- Zde probiha mutace

                int poziceProMutaci = 0;
                foreach (Trasa tras in trasaList)
                {
                    for (int i = 0; i < 35; i++)
                    {
                        Mista mistaKVymeneRand = new Mista();
                        Mista mistaKVymene = new Mista();
                        int ran = random.Next(trasaList[1].naseTrasa.Count);     // Do 35 
                        poziceProMutaci++;
                        if (poziceProMutaci == 200)                                      // urcuje kolikate misto v trase zmutuje
                        {
                            if (ran != i)                                               // pokud random neni stejna pozice
                            {
                                mistaKVymeneRand = tras.naseTrasa[ran];                 // ulozim nahodne misto
                                mistaKVymene = tras.naseTrasa[i];                       // ulozim misto i
                                tras.naseTrasa[ran] = mistaKVymene;                     // vymenim je mezi sebou
                                tras.naseTrasa[i] = mistaKVymeneRand;
                                poziceProMutaci = 0;                                    // vyniluju si pozici pro mutaci
                            }
                            else
                                poziceProMutaci = 0;
                        }
                    }
                }
            }

            // -------------------------------------------------------------Vypis nejkratsi nalezene delky
            Console.WriteLine("\n\n\n\n\n\n\n\n");
            Console.WriteLine("----------------------------------------------------------------------------------------------------------");
            Console.WriteLine(" Nejkratší délka cesty  je {0} km a nachází se v {1}-te generaci .\n Jeho trasa je : \n", nejkratsiTrasa.delkaCesty, generaceNejkratsiTrasy );
            foreach (Mista m in nejkratsiTrasa.naseTrasa)
            {
                Console.Write(" {0} - ", m.jmenomesta);
            }
            Console.WriteLine("\n----------------------------------------------------------------------------------------------------------");
            Console.WriteLine("\n\n\n\n\n\n\n\n");
            Console.ReadKey();
            Console.ReadKey();
            Console.ReadKey();
            Console.ReadKey();
        }

    }
}

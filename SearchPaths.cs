using ExcelDataReader;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SalesMan
{
    class SearchPaths
    {
        public List<string> Cities = new List<string>();  
        /// <summary>
        /// Seznam měst
        /// </summary>
        public List<City> listOfCities = new List<City>(); 
        /// <summary>
        /// Seznam všech tras
        /// </summary>
        public List<Path> ListOfPaths = new List<Path>();
        /// <summary>
        /// Seznam všech pomocných tras
        /// </summary>
        public List<Path> HelpListOfPaths = new List<Path>();
        /// <summary>
        /// Nejkratší nalezená trasa
        /// </summary>
        public Path ShortestPath = new Path();
        private Random random;
        public int shortestGenerationNumber = 0;
        public SearchPaths()
        {
            random = new Random();
        }        
        /// <summary>
        /// Funkce převede data z excelového souboru
        /// </summary>
        /// <param name="numberOfMembersInGeneration"></param>
        public void WorkExcel(int numberOfMembersInGeneration)
        {
            double[,] VDouble = new double[35, 35];
            double[,] ExcelDistance = new double[35, 35];
            double[] helpField = new double[35];
            List<City> savingToPath = new List<City>();
            List<City> saving = new List<City>();
            // String popisuje cestu k souboru
            string SavePlace = @"d:\Distance.xlsx";
            // Otevření souboru  / založení var stream
            using (var stream = File.Open(SavePlace, FileMode.Open, FileAccess.Read))                  
            {
                // Pokus o otevření
                using (IExcelDataReader reader = ExcelReaderFactory.CreateReader(stream))           
                {
                    DataSet result = reader.AsDataSet(new ExcelDataSetConfiguration()
                    {
                        ConfigureDataTable = (_) => new ExcelDataTableConfiguration() { UseHeaderRow = false }
                    });
                    DataTableCollection excel = result.Tables;
                    // Prochází jednotlivá pole v excelu
                    foreach (DataTable individualBox in excel)                                     
                    {
                        // Zde načtu list stringů pro pozdejší porovnání 
                        for (int c = 1; c < individualBox.Columns.Count; c++)                          
                        {
                            var p = individualBox.Rows[0][c];
                            string s = p.ToString();
                            Cities.Add(s);
                        }
                        // 1 je tam kvůli ořezu 1. sloupce a 1. řádku
                        for (int a = 1; a < individualBox.Rows.Count; a++)                         
                        {
                            for (int b = 1; b < individualBox.Columns.Count; b++)
                            {
                                var help = individualBox.Rows[a][b];
                                string city = help.ToString();
                                Cities.Add(city);
                                double.TryParse(city, out double number);
                                VDouble[b - 1, a - 1] = number;
                            }
                        }
                    }
                }
            }
            //Naplní seznam měst
            for (int a = 0; a < 35; a++)
            {
                for (int b = 0; b < 35; b++)
                {
                    if (VDouble[a, b] != 0)
                        ExcelDistance[a, b] = VDouble[a, b];
                    else
                        // Slouží k doplnění prázdných míst z excel tabulky
                        ExcelDistance[a, b] = VDouble[b, a];   
                }
            }
            for (int i = 0; i < 35; i++)
            {
                for (int j = 0; j < 35; j++)
                {
                    helpField[j] = ExcelDistance[j, i];
                }
                listOfCities.Add(new City(Cities[i], helpField));
                helpField = new double[35];
            }
            // ---------------------------------------------------------------------- Slouží k naplnění ListOfPaths
            // a je menší než počet jedinců v generaci
            for (int a = 0; a < numberOfMembersInGeneration; a++)                            
            {
                // Udělá odkaz na dalsi List 
                foreach (City m in listOfCities)
                    saving.Add(m);
                // Dokud není prazdný, tak je procházen
                for (int i = 0; i < 35; i++)                                       
                {
                    // Náhodné vybrání města
                    int randomCity = random.Next(saving.Count);                       
                    savingToPath.Add(saving[randomCity]);                          
                    saving.RemoveAt(randomCity);                                     
                }
                //1. generace trasy
                ListOfPaths.Add(new Path(savingToPath));                   
                saving.Clear();
                savingToPath.Clear();
            }
        } 

        /// <summary>
        /// Selekce zajistí, že trasy s kratší vzdáleností mají větší šanci splodit další generaci. Založeno na principu váhované rulety.
        /// </summary>
        public void Select()
        {
            HelpListOfPaths.Clear();
            // Znulování totalSelectionOfRoulette při každé selekci v generaci
            double totalSelectionOfRoulette = 0; 
            // Sečtení jednotlivých podílů
            foreach (Path t in ListOfPaths)
            {
                totalSelectionOfRoulette += (1 / (t.leghtOfJourney));          
            }
            // Převedení na int
            int range = (int)(totalSelectionOfRoulette * 1000000000);                                          
            for (int a = 0; a < ListOfPaths.Count; a++)            
            {
                // Vyberu náhodné číslo z celého rozsahu.
                int rand = random.Next(range);                              
                double inverseRandom = ((double)rand / 1000000000);    
                double choiceOfRoulette = 0;
                foreach (Path path in ListOfPaths)
                {
                    choiceOfRoulette += (1 / (path.leghtOfJourney));
                    if ((totalSelectionOfRoulette - choiceOfRoulette) <= (inverseRandom))
                    {
                        //Vytvoření Listu selektovaných tras
                        HelpListOfPaths.Add(new Path(path.OurPath));
                        // Je-li podmínka splněna vyskočí z foreach
                        break;                                           
                    }
                }
            }
            // Uložení do ListOfPaths
            ListOfPaths.Clear();
            foreach (Path path in HelpListOfPaths)
            {
                ListOfPaths.Add(new Path(path.OurPath));
            }
        }

        /// <summary>
        /// Funkce zajistí křížení mezi jednotlivými trasami
        /// </summary>
        public void Cross()
        {
            HelpListOfPaths.Clear();
            // Listy duplicitmích měst mezi dvěmi skříženými trasami
            List<City> firstDuplications = new List<City>();                 
            List<City> secondDuplications = new List<City>();
            // Potomci tras
            List<City> firstDescendants = new List<City>();              
            List<City> secondDescendants = new List<City>();
            // Polohy jednotlivých duplicitních měst v trsách
            List<int> firstPlacesOfDuplications = new List<int>();          
            List<int> secondPlacesOfDuplications = new List<int>();
            for (int a = 0; a < ListOfPaths.Count; a += 2)
            {
                int randomDivider = random.Next(2, 33);
                int restOfDivider = 35 - randomDivider;
                // Smazání proměných
                firstDuplications.Clear();
                secondDuplications.Clear();
                firstDescendants.Clear();              
                secondDescendants.Clear();
                firstPlacesOfDuplications.Clear();           
                secondPlacesOfDuplications.Clear();
                // vytvoření 1. potomku s duplicitami
                firstDescendants.AddRange(ListOfPaths[a].OurPath);
                firstDescendants.RemoveRange(randomDivider, restOfDivider);
                firstDescendants.AddRange(ListOfPaths[a + 1].OurPath);
                firstDescendants.RemoveRange(randomDivider, randomDivider);
                // vytvoření 2. potomku s duplicitami
                secondDescendants.AddRange(ListOfPaths[a + 1].OurPath);
                secondDescendants.RemoveRange(randomDivider, restOfDivider);
                secondDescendants.AddRange(ListOfPaths[a].OurPath);
                secondDescendants.RemoveRange(randomDivider, randomDivider);
                // Zajistí skřížení každých dvou tras
                for (int b = 0; b < randomDivider; b++)
                {
                    for (int c = randomDivider; c < 35; c++)              
                    {
                        // Uložení duplicitních míst a jejich poloh v 1. potomku
                        if (firstDescendants[b].CityName == firstDescendants[c].CityName)
                        {
                            firstDuplications.Add(firstDescendants[b]);
                            firstPlacesOfDuplications.Add(b);
                        }
                    }
                    for (int c = randomDivider; c < 35; c++)
                    {
                        // Uložení duplicitních míst a jejich polohu v 2. potomku
                        if (secondDescendants[b].CityName == secondDescendants[c].CityName)
                        {
                            secondDuplications.Add(secondDescendants[b]);
                            secondPlacesOfDuplications.Add(b);
                        }
                    }
                }
                for (int r = 0; r < firstPlacesOfDuplications.Count; r++)
                {
                    // V potomcích se vymění duplicitní místa
                    secondDescendants[secondPlacesOfDuplications[r]] = firstDuplications[r];
                    firstDescendants[firstPlacesOfDuplications[r]] = secondDuplications[r];
                }
                // Přidání potomků do HelpListOfPaths
                HelpListOfPaths.Add(new Path(firstDescendants));               
                HelpListOfPaths.Add(new Path(secondDescendants));
            }
            ListOfPaths.Clear();
            foreach (Path t in HelpListOfPaths)
            {
                ListOfPaths.Add(new Path(t.OurPath));
            }
            HelpListOfPaths.Clear();
        }

        /// <summary>
        /// Funkce zajistí mutaci v trasách. (Díky tomu nezůstává funkce v lokálních minimech.)
        /// </summary>
        public void Mutate()
        {
            int mutatePosition = 0;
            foreach (Path path in ListOfPaths)
            {
                for (int i = 0; i < 35; i++)
                {
                    City randomPickCity = new City();
                    City chooseCity = new City();
                    // Do 35 
                    int ran = random.Next(ListOfPaths[1].OurPath.Count);            
                    mutatePosition++;
                    // Určuje kolikáté město v trase zmutuje
                    if (mutatePosition == 400)                                    
                    {
                        // Pokud ran není stejná pozice
                        if (ran != i)                                               
                        {
                            // Uložím náhodné město
                            randomPickCity = path.OurPath[ran];                 
                            // Uložím jiné náhodné město
                            chooseCity = path.OurPath[i];                       
                            // Vyměním je mezi sebou
                            path.OurPath[ran] = chooseCity;                     
                            path.OurPath[i] = randomPickCity;                                                              
                        }
                        // Vynulování pozice pro mutaci
                        mutatePosition = 0;
                    }
                }
            }
        }

        /// <summary>
        /// Vykreslení nejkratší trasy
        /// </summary>
        private void DrawResult()
        {
            Console.WriteLine("\n\n\n\n\n\n\n\n");
            Console.WriteLine("----------------------------------------------------------------------------------------------------------");
            Console.WriteLine(" Nejkratší délka cesty  je {0} km a nachází se v {1}-té generaci .\n Jeho trasa je : \n", ShortestPath.leghtOfJourney, shortestGenerationNumber);
            foreach (City m in ShortestPath.OurPath)
            {
                Console.Write(" {0} - ", m.CityName);
            }
            Console.WriteLine("\n----------------------------------------------------------------------------------------------------------");
            Console.WriteLine("\n\n\n\n\n\n\n\n");
            Console.ReadKey();
        }

        /// <summary>
        /// Funkce zajistí všechny kroky při chodu genetického algoritmu
        /// </summary>
        /// <param name="NumberOfGenerations">Počet generací</param>
        /// <param name="displayedGeneration">Zobrazená generace</param>
        /// <param name="numberOfMembersInGeneration">Počet členů v generaci</param>
        public void Generate(int NumberOfGenerations, int displayedGeneration, int numberOfMembersInGeneration)
        {
            // Vytvoření první generace
            WorkExcel(numberOfMembersInGeneration);
            DetermineDistance(0);
            Select();
            Cross();
            Mutate();
            // Vytvoření dalších generací
            for (int numberOfRepetitions = 1; numberOfRepetitions <= NumberOfGenerations; numberOfRepetitions++)
            {
                if (numberOfRepetitions == 1)
                    // Trasa delší než nejdelší předpokládáná
                    ShortestPath = new Path(ListOfPaths[0].OurPath, 10000);                  
                DetermineDistance(numberOfRepetitions);
                Write(numberOfRepetitions, displayedGeneration);
                Select();
                Cross();
                Mutate();               
            }
            DrawResult();
        }

        /// <summary>
        /// Funkce určí vzdálenosti, nachází nejkratší cestu a vypisuje první generaci.
        /// </summary>
        /// <param name="numberOfRepetitions">Informace, o kterou generaci se jedná</param>
        private void DetermineDistance(int numberOfRepetitions)
        {
            if (numberOfRepetitions == 0)
                Console.WriteLine("Délka cest první generace.");
            else
                HelpListOfPaths.Clear();
            int firstPosition = 0;
            int secondPosition = 0;
            // Rozebrání na jednotlivé trasy
            foreach (Path path in ListOfPaths)                      
            {
                double lengthOfPath = 0;
                // Určení délky mezi jednotlivými městy
                for (int t = 0; t < 34; t++)
                {
                    string firstCityName = path.OurPath[t].CityName;
                    string secondCityName = path.OurPath[t + 1].CityName;
                    for (int tt = 0; tt < 35; tt++)
                    {
                        if (firstCityName == Cities[tt])
                        {
                            firstPosition = tt;
                        }
                        if (secondCityName == Cities[tt])
                        {
                            secondPosition = tt;
                        }
                    }
                    // Sečtení jednotlivých délek mezi městy
                    lengthOfPath += listOfCities[secondPosition].Distance[firstPosition];
                }
                HelpListOfPaths.Add(new Path(path.OurPath, lengthOfPath));     // vznik ohodnoceneho pole tras
            }
            ListOfPaths.Clear();
            // Pro první generaci
            if (numberOfRepetitions == 0)
            {
                Console.WriteLine("První generace: \n");
                foreach (Path t in HelpListOfPaths)
                {
                    ListOfPaths.Add(new Path(t.OurPath, t.leghtOfJourney));

                }
                foreach (Path t in HelpListOfPaths)
                {
                    Console.Write(t.leghtOfJourney + " - ");
                }
                Console.WriteLine("\n\n\n\n\n");
            }
            // Pro další generace
            else
            {
                foreach (Path t in HelpListOfPaths)
                {
                    ListOfPaths.Add(new Path(t.OurPath, t.leghtOfJourney));
                    if (ShortestPath.leghtOfJourney > t.leghtOfJourney)
                    {
                        ShortestPath = new Path(t.OurPath, t.leghtOfJourney);
                        shortestGenerationNumber = numberOfRepetitions;
                    }
                }
            }            
        }

        /// <summary>
        /// Pravidelný výpis generací
        /// </summary>
        /// <param name="numberOfRepetitions">Informace, o kterou generaci se jedná</param>
        /// <param name="displayedGeneration">Zobrazená generace</param>
        private void Write(int numberOfRepetitions, int displayedGeneration)
        {
            if (numberOfRepetitions % displayedGeneration == 0)
            {
                Path shortesPathFromGeneration = ListOfPaths[0];
                double lenghtOfPath = 0;
                // Výpis generace
                Console.WriteLine("----------------------------------------------------------------------------------------------------------");
                Console.WriteLine("     ---------- Výpis {0}- té generace ----------     ", numberOfRepetitions);
                foreach (Path t in ListOfPaths)
                {
                    lenghtOfPath += t.leghtOfJourney;
                    if (t.leghtOfJourney < shortesPathFromGeneration.leghtOfJourney)
                        shortesPathFromGeneration = t;
                }
                double lenghtOfAveragePath = (lenghtOfPath / ListOfPaths.Count) + (lenghtOfPath % ListOfPaths.Count);
                Console.WriteLine(" Průměrná délka cesty v generaci je {0} km. ", lenghtOfAveragePath);
                Console.WriteLine(" Nejkratší délka cesty v dané generaci je {0} km.\n Jeho trasa je : \n", shortesPathFromGeneration.leghtOfJourney);
                foreach (City m in shortesPathFromGeneration.OurPath)
                {
                    Console.Write(" {0} - ", m.CityName);
                }
                Console.WriteLine("\n----------------------------------------------------------------------------------------------------------");
                Console.WriteLine("\n\n\n\n\n");
            }
        }
    }
}

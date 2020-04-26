using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SalesMan
{
    class Program
    {
        static void Main(string[] args)
        {
            int numberOfMembersInGeneration = 100;
            int NunberOfGenerations = 2000;
            int displayedGeneration = 100;
            SearchPaths searchPaths = new SearchPaths();
            // Spuštění generování
            searchPaths.Generate(NunberOfGenerations, displayedGeneration, numberOfMembersInGeneration);
     
        }
    }
}

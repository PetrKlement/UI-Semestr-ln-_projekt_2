using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SalesMan
{
    class Path
    {
        // Konkrétní cesta
        public List<City> OurPath = new List<City>();
        // Délka cesty
        public double leghtOfJourney;
        public Path()
        {
        }
        public Path(List<City> m)
        {
            foreach (City c in m)
                OurPath.Add(new City(c.CityName, c.Distance));
        }
        public Path(List<City> m, double delka)
        {
            foreach (City c in m)
                OurPath.Add(new City(c.CityName, c.Distance));
            leghtOfJourney = delka;
        }
    }
}

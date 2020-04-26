using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SalesMan
{
    class City
    {
        public string CityName;
        // Vzdálenosti ostatních měst
        public double[] Distance = new double[35];
        /// <summary>
        /// Konstruktor města
        /// </summary>
        /// <param name="name"></param>
        /// <param name="distance"></param>
        public City(string name, double[] distance)
        {
            CityName = name;
            Distance = distance;
        }
        public City()
        {
        }
    }
}

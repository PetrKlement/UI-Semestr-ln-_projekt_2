using System;

namespace UI2
{
    class Mista
    {
        public string jmenomesta;
        public double[] vzdalenosti = new double[35];
        public Mista(string jmeno, double[] dalka)
        {
            jmenomesta = jmeno;
            vzdalenosti = dalka;
        }
        public Mista()
        {
        }
    }
}

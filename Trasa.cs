using System;
using System.Collections.Generic;


namespace UI2
{
    class Trasa
    {
        public List<Mista> naseTrasa = new List<Mista>();
        public double delkaCesty;
        public Trasa()
        {
        }
        public Trasa( List<Mista> m)
        {          
            naseTrasa = m;
        }
        public Trasa(List<Mista> m, double delka)
        {       
            naseTrasa = m;
            delkaCesty = delka;
        }
    }
}

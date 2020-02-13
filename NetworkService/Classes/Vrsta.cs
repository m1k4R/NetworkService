using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetworkService.Classes
{
    public class Vrsta
    {
        string name;
        string img;

        public string Name
        {
            get
            {
                return name;
            }

            set
            {
                name = value;
            }
        }

        public string Img
        {
            get
            {
                return img;
            }

            set
            {
                img = value;
            }
        }

        public Vrsta(string n, string i)
        {
            Name = n;
            Img = i;
        }

    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetworkService.Classes
{
    public class Slika
    {
        public String imageUri { get; set; }

        public Slika()
        {
            imageUri = "";
        }

        public Slika(String uri)
        {
            imageUri = uri;
        }

    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetworkService.Classes
{
    public class Mjerac : INotifyPropertyChanged
    {
        private int id;
        private string name;
        private Vrsta type;
        private double mvalue;

        public event PropertyChangedEventHandler PropertyChanged;

        private void RaisePropertyChanged(string property)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(property));
            }
        }

        public int Id
        {
            get
            {
                return id;
            }

            set
            {
                id = value;
            }
        }

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

        public Vrsta Type
        {
            get
            {
                return type;
            }

            set
            {
                type = value;
            }
        }

        public double Mvalue
        {
            get
            {
                return mvalue;
            }

            set
            {
                if (mvalue != value)
                {
                    this.mvalue = value;
                    RaisePropertyChanged("Mvalue");
                }
            }
        }

        public Mjerac(int i, string n, Vrsta t)
        {
            Id = i;
            Name = n;
            Type = t;
            Mvalue = 0;
        }

   
    }
}

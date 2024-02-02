using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization.Formatters;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp1.Model
{
    public class Availabilities
    {
        public List<Intervals> Intervals { get; set; }
        public bool Everyday { get; set; }
    }
}

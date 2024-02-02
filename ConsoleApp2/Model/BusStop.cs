using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp1.Model
{

    public class BusStop
    {
        public string Name { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public int Count { get; set; }
        public CompanyMetaData CompanyMetaData { get; set; }
        public List<List<double>> BoundedBy { get; set; }
    }
}

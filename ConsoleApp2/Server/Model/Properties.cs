using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp1.Model
{
    public class Properties
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public CompanyMetaData CompanyMetaData { get; set; }
        public List<List<double>> BoundedBy { get; set; }

    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EZDB_Tests.SimpleTestClasses
{
    public class SimpleClass
    {
        public SimpleClass() 
        {
            myIntList = new();
            myStringList = new();
        }

        public List<int> myIntList { get; set; }

        public List<string> myStringList { get; set; }
    }
}
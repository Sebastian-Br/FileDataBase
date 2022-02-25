using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EZDB_Tests.SimpleTestClasses
{
    public class SimpleClassDB
    {
        public SimpleClassDB()
        {
            simpleClass = new();
            DbName = "";
        }
        SimpleClass simpleClass { get; set; }

        string DbName { get; set; }

        public static SimpleClassDB GetTestDB()
        {
            SimpleClassDB db = new();
            SimpleClass simpleClass = new();
            simpleClass.myIntList.Add(1);
            simpleClass.myIntList.Add(2);
            simpleClass.myIntList.Add(3);
            simpleClass.myStringList.Add("string1");
            simpleClass.myStringList.Add("string2");
            db.simpleClass = simpleClass;
            return db;
        }
    }
}
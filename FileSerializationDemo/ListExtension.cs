using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExtensionMethods
{
    public static class ListExtension
    {
        public static void Pop<T>(this List<T> list)
        {
            list.RemoveAt(list.Count - 1);
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileSerializationDemo.Classes
{
    /// <summary>
    /// Used to navigate objects at runtime to establish references correctly.
    /// </summary>
    public class ObjectLinq
    {
        public ObjectLinq() { }
        public string PropertyName { get; set; }

        public int DBid { get; set; }

        /// <summary>
        /// Creates a deep copy of the original list.
        /// </summary>
        /// <param name="originalList">The original list.</param>
        /// <returns>A deep copy of that list.</returns>
        public static List<ObjectLinq> CopyLinqs(List<ObjectLinq> originalList)
        {
            List<ObjectLinq> copiedList = new();
            if(originalList != null)
            {
                foreach (ObjectLinq objectLinq in originalList)
                {
                    copiedList.Add(new ObjectLinq { PropertyName = objectLinq.PropertyName, DBid = objectLinq.DBid });
                }
            }

            return copiedList;
        }
    }
}
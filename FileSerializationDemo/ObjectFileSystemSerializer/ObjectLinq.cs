using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileSerializationDemo
{
    /// <summary>
    /// Used to navigate objects at runtime to establish references correctly.
    /// </summary>
    public class PropertyLinq
    {
        public PropertyLinq() { }
        public string PropertyName { get; set; }

        public int DBid { get; set; }

        /// <summary>
        /// Creates a deep copy of the original list.
        /// </summary>
        /// <param name="originalList">The original list.</param>
        /// <returns>A deep copy of that list.</returns>
        public static List<PropertyLinq> CopyLinqs(List<PropertyLinq> originalList)
        {
            List<PropertyLinq> copiedList = new();
            if(originalList != null)
            {
                foreach (PropertyLinq objectLinq in originalList)
                {
                    copiedList.Add(new PropertyLinq { PropertyName = objectLinq.PropertyName, DBid = objectLinq.DBid });
                }
            }

            return copiedList;
        }

        public override bool Equals(object obj)
        {
            if ((obj == null) || !this.GetType().Equals(obj.GetType()))
            {
                return false;
            }
            else
            {
                PropertyLinq o = (PropertyLinq)obj;
                return String.Equals(PropertyName, o.PropertyName) && (DBid == o.DBid);
            }
        }

        public override int GetHashCode()
        {
            throw new NotImplementedException();
        }
    }
}
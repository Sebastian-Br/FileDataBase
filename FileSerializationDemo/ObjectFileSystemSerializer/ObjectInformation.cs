using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileSerializationDemo.ObjectFileSystemSerializer
{
    class ObjectInformation
    {
        /// <summary>
        /// The Serialize() function should use this property directly
        /// When deserializing, the to-be-deserialized object is walked with the information stored here.
        /// </summary>
        public List<PropertyLinq> ObjectLinqs { get; set; }

        /// <summary>
        /// A hash generated from an object. Used to determine if the object has changed.
        /// </summary>
        public ObjectHash ObjectHash { get; set; }
    }
}

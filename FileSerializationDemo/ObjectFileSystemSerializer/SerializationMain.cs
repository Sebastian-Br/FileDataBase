using ExtensionMethods;
using FileSerializationDemo.Classes;
using Newtonsoft.Json;
using NLog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace FileSerializationDemo.ObjectFileSystemSerializer
{
    public partial class SerializationMain
    {
        Logger logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Used to store and find objects to establish correct references.
        /// Thus only used for reference-types.
        /// </summary>
        //Dictionary<Type, Tuple<object, List<ObjectLinq>>> ObjectDictionary { get; set; }

        /// <summary>
        /// Used to store and find objects to establish correct references.
        /// Thus only used for reference-types.
        /// </summary>
        Dictionary<object, ObjectInformation> ObjectDictionary { get; set; }

        /// <summary>
        /// This List holds the folder names that, when combined, make up the serialization path of the current object.
        /// </summary>
        List<string> CurrentSerializationPath { get; set; }
        public bool SerializeRoot(object rootObject)
        {
            try
            {
                ObjectDictionary = new();
                List<PropertyInfo> properties = rootObject.GetType().GetProperties().ToList();
                foreach(PropertyInfo property in properties)
                {
                    object propertyValue = property.GetValue(rootObject);
                    Serialize(propertyValue, property);
                }
            }
            catch(Exception e)
            {
                return false;
            }
            return false;
        }

        private bool Serialize(object obj, PropertyInfo property)
        {
            try
            {
                CurrentSerializationPath.Add(property.PropertyType.Name);
                if(obj is ValueType || obj is String) // No need to check for references.
                {
                    string serializedObject = JsonConvert.SerializeObject(obj, Formatting.Indented);
                    File.WriteAllText();
                }
                else // Have to check for references.
                {

                }
            }
            catch (Exception e)
            {
            }

            CurrentSerializationPath.Pop();
            return false;
        }


    }
}

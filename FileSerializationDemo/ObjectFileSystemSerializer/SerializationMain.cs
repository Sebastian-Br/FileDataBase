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
using FileSerializationDemo;

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
                bool bSuccess = true;
                logger.Info("SerializeRoot() called on " + rootObject.GetType());
                ObjectDictionary = new();
                List<PropertyInfo> properties = rootObject.GetType().GetProperties().ToList();
                CurrentSerializationPath.Add(rootObject.GetType().Name + "Root");
                foreach(PropertyInfo property in properties)
                {
                    logger.Info("SerializeRoot() found property " + property.Name);
                    object propertyValue = property.GetValue(rootObject);
                    bSuccess &= Serialize(propertyValue, property);
                    if(!bSuccess)
                    {
                        logger.Error("SerializeRoot() Internal Error");
                        goto location_SerRoot_end;
                    }
                }
                return bSuccess;
            }
            catch(Exception e)
            {
                logger.Log(LogLevel.Error, e);
            }
            location_SerRoot_end:
            return false;
        }

        private bool Serialize(object obj, PropertyInfo property)
        {
            if (obj == null)
            {
                logger.Info("This object was null!");
                return true;
            }
            int pathsAdded = 0;
            bool bSuccess = true;
            logger.Info("Serialize() called on " + property + " of type " + obj.GetType().Name);
            logger.Info("Serialize() CurrentSerializationPath is " + GetCurrentSerializationDirectory());
            try
            {
                if(obj is ValueType || obj is String) // No need to check for references & serialize in same folder.
                {
                    string serializedObject = JsonConvert.SerializeObject(obj, Formatting.Indented);
                    bSuccess &= WinFileSystem.CreateFolderStructure(GetCurrentSerializationDirectory());
                    File.WriteAllText(GetCurrentSerializationDirectory() + GetPrimitiveJsonFileName(property), serializedObject);
                }
                else // Reference-type, check for references; serialize in sub-folder.
                {
                    logger.Info("This is a reference type");
                    if (!ObjectDictionary.ContainsKey(obj)) // an unknown object.
                    {
                        logger.Info("This object is unknown to the dictionary");
                        ObjectDictionary.Add(obj, new ObjectInformation()); // TODO
                    }
                    else
                    {
                        // write objectLinqs file.
                        logger.Info("This object is KNOWN to the dictionary");
                        ObjectInformation tmp = new();
                        ObjectDictionary.TryGetValue(obj, out tmp);
                        string serializedObjectLinq = JsonConvert.SerializeObject(tmp);
                        bSuccess &= WinFileSystem.CreateFolderStructure(GetCurrentSerializationDirectory());
                        File.WriteAllText(GetCurrentSerializationDirectory() + GetObjectLinqJsonFileName(obj), serializedObjectLinq);
                        logger.Info("Returning: " + bSuccess);
                        return bSuccess;
                    }
                    if (ReflectionX.IsObjectList(obj))
                    {
                        logger.Info("This is a List!");
                        CurrentSerializationPath.Add(property != null ? property.Name : "");
                        pathsAdded++;
                        bSuccess &= WinFileSystem.CreateFolderStructure(GetCurrentSerializationDirectory());
                        IEnumerable<object> objects = (IEnumerable<object>)obj;
                        foreach (object nestedObject in objects)
                        {
                            CurrentSerializationPath.Add(GetNextDBid(GetCurrentSerializationDirectory()).ToString());
                            bSuccess &= WinFileSystem.CreateFolderStructure(GetCurrentSerializationDirectory());
                            bSuccess &= Serialize(nestedObject, null);
                            CurrentSerializationPath.Pop();
                            if (!bSuccess)
                                goto location_end;
                        }
                    }
                    else
                    {
                        logger.Info("This is a nonlist-object!");
                        if(property != null)
                        {
                            CurrentSerializationPath.Add(property.Name);
                            pathsAdded++;
                        }
                        List<PropertyInfo> properties = obj.GetType().GetProperties().ToList();
                        foreach (PropertyInfo propertyInfo in properties)
                        {
                            logger.Info("Found property " + propertyInfo.Name);
                            object propertyValue = propertyInfo.GetValue(obj);
                            bSuccess &= Serialize(propertyValue, propertyInfo);
                            if (!bSuccess)
                                goto location_end;
                        }
                        if (property != null)
                        {
                            CurrentSerializationPath.Pop();
                            pathsAdded--;
                        }
                    }
                }

                for(int i = 0; i < pathsAdded; i++)
                {
                    CurrentSerializationPath.Pop();
                }

                logger.Info("Returning/final: " + bSuccess);
                return bSuccess;
            }
            catch (Exception e)
            {
                bSuccess = false;
                logger.Log(LogLevel.Error, e);
            }

            location_end:

            for (int i = 0; i < pathsAdded; i++)
            {
                CurrentSerializationPath.Pop();
            }

            logger.Error("Returning false");
            return false;
        }


    }
}

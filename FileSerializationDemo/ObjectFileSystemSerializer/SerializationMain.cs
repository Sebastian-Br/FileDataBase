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
using EZDB_Core.ObjectFileSystemSerializer;
using System.Text.RegularExpressions;

namespace FileSerializationDemo.ObjectFileSystemSerializer
{
    public partial class SerializationMain
    {
        Logger logger = LogManager.GetCurrentClassLogger();

        string SerializationRootDirectory { get; set; }

        /// <summary>
        /// Used to establish correct references and to track whether objects have changed.
        /// </summary>
        Dictionary<object, ObjectInformation> SerializedObjectsDictionary { get; set; }

        /// <summary>
        /// This List holds the folder names that, when combined, make up the serialization path of the current object.
        /// </summary>
        List<string> CurrentSerializationPath { get; set; }

        /// <summary>
        /// This is the root object to be stored. It is used to walk references.
        /// </summary>
        object Root { get; set; }

        /// <summary>
        /// This property is responsible for recursively walking an object and calculating its hash.
        /// </summary>
        ObjectHashMgr objectHashMgr { get; set; }

        /// <summary>
        /// This string is used to parse the current serialization path (without the root) to generate objectlinqs.
        /// This is not optimal and should be replaced in the future.
        /// </summary>
        Regex ObjectLinqsRegexParser { get; set; }

        /// <summary>
        /// Serializes an object into the Object.GetType().Name + Root\\ folder.
        /// </summary>
        /// <param name="rootObject">The object you intend to serialize.</param>
        /// <returns>True: Success. False: Failure.</returns>
        public bool SerializeRoot(object rootObject)
        {
            try
            {
                if (rootObject == null)
                {
                    logger.Error("SerializeRoot() Root object is null! Returning: false");
                    return false;
                }

                bool bSuccess = true;
                logger.Info("SerializeRoot() called on " + rootObject.GetType());
                if (!SetVisitedToFalse())
                {
                    logger.Error("SerializeRoot() SetVisitedToFalse() false");
                    return false;
                }

                List<PropertyInfo> properties = rootObject.GetType().GetProperties().ToList();
                CurrentSerializationPath = new();
                SerializationRootDirectory = rootObject.GetType().Name + "Root";
                foreach (PropertyInfo property in properties)
                {
                    logger.Info("SerializeRoot() found property " + property.Name);
                    object propertyValue = property.GetValue(rootObject);
                    bSuccess &= Serialize(propertyValue, property);

                    if (!bSuccess)
                    {
                        logger.Error("SerializeRoot() Internal Error");
                        goto location_SerRoot_end;
                    }
                }

                RemoveUnvisitedEntries();
                return bSuccess;
            }
            catch(Exception e)
            {
                logger.Log(LogLevel.Error, e);
            }
            location_SerRoot_end:
            logger.Error("SerializeRoot() Returning: false");
            return false;
        }

        private bool Serialize(object obj, PropertyInfo property)
        {
            if (obj == null)
            {
                logger.Info("Serialize() This object was null!");
                return true;
            }
            int pathsAdded = 0;
            bool bSuccess = true;
            logger.Info("Serialize() called on " + property + " of type " + obj.GetType().Name);
            if (property != null)
            {
                CurrentSerializationPath.Add(property.Name);
                pathsAdded++;
            }
            logger.Info("Serialize() CurrentSerializationPath is " + GetCurrentSerializationDirectory());
            logger.Info("Serialize() CurrentSerializationPathNonRoot is " + GetCurrentSerializationDirectoryWithoutRoot());
            try
            {
                if (obj is ValueType || obj is String) // No need to check for references & serialize in same folder.
                {
                    string serializedObject = JsonConvert.SerializeObject(obj, Formatting.Indented);
                    bSuccess &= WinFileSystem.CreateFolderStructure(GetCurrentSerializationDirectory());
                    string primitiveJsonPath = GetPrimitiveSerializationPath(property);
                    if (File.Exists(primitiveJsonPath))
                    {
                        if (File.ReadAllText(primitiveJsonPath).Equals(serializedObject))
                        {
                            logger.Info("Serialize() This value-type or string property has already been stored and has not changed.");
                        }
                        else
                        {
                            File.WriteAllText(primitiveJsonPath, serializedObject);
                        }
                    }
                    else
                    {
                        File.WriteAllText(primitiveJsonPath, serializedObject);
                    }
                }
                else // Reference-type, check for references; serialize in sub-folder.
                {
                    logger.Info("Serialize() This is a reference type");
                    if (!SerializedObjectsDictionary.ContainsKey(obj))
                    {
                        logger.Info("Serialize() This object is unknown to the dictionary");
                        ObjectInformation objectInformation = new(true); // this is a visited object.
                        SerializedObjectsDictionary.Add(obj, objectInformation);
                        objectInformation.ObjectHash = objectHashMgr.Compute(obj); // obj can not be null
                        if(objectInformation.ObjectHash == null)
                        {
                            logger.Error("Serialize() Last objectHashMgr.Compute() returned false. Returning: false");
                            goto location_end;
                        }
                        bSuccess &= objectInformation.ImportPropertyLinqsFromSerializationPath(CurrentSerializationPath, ObjectLinqsRegexParser);
                    }
                    else
                    {
                        logger.Info("Serialize() This object is KNOWN to the dictionary");
                        ObjectInformation tmp = new();
                        bSuccess &= SerializedObjectsDictionary.TryGetValue(obj, out tmp);
                        if (!bSuccess)
                        {
                            logger.Error("Serialize() ObjectDictionary.TryGetValue failed! Returning: false");
                            goto location_end;
                        }

                        if(!tmp.VisitedOnCurrentSerialization)
                        {
                            tmp.VisitedOnCurrentSerialization = true;
                            logger.Info("Serialize() Visited this obect the first time this iteration");
                            ObjectHash objectHash = objectHashMgr.Compute(obj);
                            if (objectHash == null)
                            {
                                logger.Error("Serialize() Last objectHashMgr.Compute() returned false. Returning: false");
                                goto location_end;
                            }
                            logger.Info("Serialize() Last object hash was: " + tmp.ObjectHash.ToString());
                            if (tmp.ObjectHash == objectHash)
                            {
                                logger.Info("Serialize() This object did not change. Returning: true");
                                goto location_final;
                            }
                            else
                            {
                                tmp.ObjectHash = objectHash;
                                logger.Info("Serialize() Object hash changed to: " + tmp.ObjectHash.ToString());
                            }
                        }
                        else
                        {
                            string serializedObjectLinq = JsonConvert.SerializeObject(tmp.PropertyLinqs);
                            string serializedObjectLinqPath = GetCurrentSerializationDirectory() + GetObjectLinqJsonFileName(obj);
                            bSuccess &= WinFileSystem.CreateFolderStructure(GetCurrentSerializationDirectory());
                            logger.Info("Serialize() serializedObjectLinqPath : " + serializedObjectLinqPath);
                            if (File.Exists(serializedObjectLinqPath))
                            {
                                if (File.ReadAllText(serializedObjectLinqPath).Equals(serializedObjectLinq))
                                {
                                    logger.Info("Serialize() This PropertyLinq has already been stored and has not changed.");
                                }
                                else
                                {
                                    File.WriteAllText(serializedObjectLinqPath, serializedObjectLinq);
                                }
                            }
                            else
                            {
                                File.WriteAllText(serializedObjectLinqPath, serializedObjectLinq);
                            }

                            logger.Info("Serialize() Returning: " + bSuccess);
                            goto location_final;
                        }
                    }

                    if (ReflectionX.IsObjectList(obj))
                    {
                        logger.Info("Serialize() This is a List!");
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
                        logger.Info("Serialize() This is a nonlist-object!");
                        List<PropertyInfo> properties = obj.GetType().GetProperties().ToList();
                        foreach (PropertyInfo propertyInfo in properties)
                        {
                            logger.Info("Serialize() Found property " + propertyInfo.Name);
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

                location_final:

                for(int i = 0; i < pathsAdded; i++)
                    CurrentSerializationPath.Pop();

                logger.Info("Serialize() Returning/final: " + bSuccess);
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

            logger.Error("Serialize() Returning false");
            return false;
        }
    }
}
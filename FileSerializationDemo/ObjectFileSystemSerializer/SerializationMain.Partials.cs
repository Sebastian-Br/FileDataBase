using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ExtensionMethods;
using FileSerializationDemo.Classes;
using Newtonsoft.Json;
using NLog;

namespace FileSerializationDemo.ObjectFileSystemSerializer
{
    public partial class SerializationMain
    {
        public SerializationMain()
        {
            CurrentSerializationPath = new();
            SerializedObjectsDictionary = new();
            objectHashMgr = new();
            ObjectLinqsRegexParser = new Regex("((?<propertyName>([A-Z]|[a-z]|_)+)\\\\((?<DBid>\\d+)|))|(?<DBid2>\\d+)");
        }

        private string GetCurrentSerializationDirectory()
        {
            return SerializationRootDirectory + "\\" + String.Join("\\",  CurrentSerializationPath) + "\\";
        }

        private string GetCurrentSerializationDirectoryWithoutRoot()
        {
            return String.Join("\\", CurrentSerializationPath) + "\\";
        }

        private string GetPrimitiveJsonFileName(PropertyInfo property)
        {
            return property.Name + ".Primitive.json";
        }

        private string GetObjectLinqJsonFileName(object obj)
        {
            if (!ReflectionX.IsObjectList(obj))
            {
                return obj.GetType().Name + ".ObjLinq.json";
            }
            else
            {
                return "List.ObjLinq.json";
            }
        }

        /// <summary>
        /// Gets the highest ID in Path example\path\id, where ID is int.
        /// </summary>
        /// <param name="Path">The path to serialized object(s).</param>
        /// <returns>The highest database-ID among those objects.</returns>
        private int GetNextDBid(string Path)
        {
            logger.Info("GetNextDBid() called on Path = " + Path);
            int id = 0;
            string searchPath;
            if (!Path.EndsWith('\\'))
                searchPath = Path.Substring(0, Path.LastIndexOf('\\') + 1);
            else
                searchPath = Path;

            //logger.Info("GetNextDBid() searchPath = " + searchPath);
            List<string> directoryNames = Directory.GetDirectories(searchPath, "*", new EnumerationOptions() { RecurseSubdirectories = false }).ToList();

            if (directoryNames != null)
            {
                foreach (string directory in directoryNames)
                {
                    int tmp;
                    string tmpStr = directory;
                    if (tmpStr.Contains(searchPath)) // how to search such as to exclusively get the sub-dir names??
                        tmpStr = tmpStr.Replace(searchPath, "");
                    // now tmpStr should only hold int.Parse-able strings.
                    try
                    {
                        tmp = int.Parse(tmpStr);
                    }
                    catch (Exception e)
                    {
                        logger.Info("GetNextDBid() directory = " + directory + " not Int type!");
                        tmp = -1;
                    }

                    if (tmp > id)
                        id = tmp;
                }
            }

            id++; // we're interested in the NEXT available id.
            logger.Info("GetHighestDBidInPath() Returning = " + id);
            return id;
        }

        private object WalkRoot(List<PropertyLinq> propertyLinqs)
        {
            return ReflectionX.WalkObject(propertyLinqs, Root);
        }

        private string GetPrimitiveSerializationPath(PropertyInfo property)
        {
            return GetCurrentSerializationDirectory() + GetPrimitiveJsonFileName(property);
        }

        private bool SetVisitedToFalse()
        {
            try
            {
                Dictionary<object, ObjectInformation>.ValueCollection values = SerializedObjectsDictionary.Values;
                if(values != null)
                {
                    foreach (ObjectInformation objInfo in values)
                    {
                        objInfo.VisitedOnCurrentSerialization = false;
                    }
                }

                return true;
            }
            catch (Exception e)
            {
                logger.Log(LogLevel.Error, e);
                return false;
            }
        }

        private bool RemoveUnvisitedEntries()
        {
            try
            {
                // Step 1: Define parent Linqs.
                List<List<PropertyLinq>> parentLinqsList = new();
                foreach (KeyValuePair<object, ObjectInformation> kvp in SerializedObjectsDictionary)
                {
                    if (kvp.Value.VisitedOnCurrentSerialization == true)
                    {
                        logger.Info("Setting up Linq as parent " + JsonConvert.SerializeObject(kvp.Value.PropertyLinqs));
                        parentLinqsList.Add(kvp.Value.PropertyLinqs);
                    }
                }

                // Step 2: Set child linqs visited to true.
                foreach (KeyValuePair<object, ObjectInformation> kvp in SerializedObjectsDictionary)
                {
                    if (kvp.Value.VisitedOnCurrentSerialization == false)
                    {
                        foreach(List<PropertyLinq> parentLinqs in parentLinqsList)
                        {
                            logger.Info("Checking if " + JsonConvert.SerializeObject(kvp.Value.PropertyLinqs) + " is a child of " + JsonConvert.SerializeObject(parentLinqs));
                            int index = 0;
                            if (parentLinqs.Count > kvp.Value.PropertyLinqs.Count)
                                break; // can not be child if parentLinqs are larger.
                            PropertyLinq[] childLinqArray = kvp.Value.PropertyLinqs.ToArray();
                            foreach(PropertyLinq parentLinq in parentLinqs)
                            {
                                if(! (parentLinq.PropertyName == childLinqArray[index].PropertyName && (parentLinq.DBid == childLinqArray[index].DBid || parentLinq.DBid == -1)))
                                {
                                    logger.Info("This is not a parent Linq!");
                                    break; // Lists do not match.
                                }
                                index++;
                            }

                            if(index == parentLinqs.Count) // = contains child
                            {
                                logger.Info("Setting up Linq as child: " + JsonConvert.SerializeObject(kvp.Value.PropertyLinqs) + " is a child of " + JsonConvert.SerializeObject(parentLinqs));
                                kvp.Value.VisitedOnCurrentSerialization = true;
                                break;
                            }
                        }
                    }
                }

                foreach (KeyValuePair<object, ObjectInformation> kvp in SerializedObjectsDictionary)
                {
                    if(kvp.Value.VisitedOnCurrentSerialization == false)
                    {
                        logger.Info("Object " + JsonConvert.SerializeObject(kvp.Value.PropertyLinqs) + " was not visited!");
                        if (!SerializedObjectsDictionary.Remove(kvp.Key))
                        {
                            logger.Error("Failed to remove object!");
                            return false;
                        }
                    }
                    else
                    {
                        logger.Info("Object of type " + kvp.Key.GetType().Name + "@" + JsonConvert.SerializeObject(kvp.Value.PropertyLinqs) + " WAS visited!");
                    }
                }

                return true;
            }
            catch (Exception e)
            {
                logger.Log(LogLevel.Error, e);
                return false;
            }
        }
    }
}
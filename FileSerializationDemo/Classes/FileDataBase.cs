/*
 Author:    Sebastian Brenner
 Date:      Jan. 2022
 Version:   <0.5> Pending further tests.
 License:   Noncommercial
 */

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
//using FileSerializationDemo.Classes;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using NLog;

namespace FileSerializationDemo.Classes
{
    /// <summary>
    /// Provides basic functionality to serialize classes containing primitive, string, or :FileDataBase-
    /// types to the filesystem.
    /// </summary>
    public abstract class FileDataBase
    {
        [Newtonsoft.Json.JsonIgnore]
        [System.Text.Json.Serialization.JsonIgnore]
        private Logger logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// The unique ID associated with this object.
        /// This is used to prevent excessive write-load when objects of a list are rearranged.
        /// </summary>
        public int DBid { get; set; }

        /// <summary>
        /// The FilePath where this object will be serialized.
        /// Do NOT set this manually.
        /// </summary>
        [Newtonsoft.Json.JsonIgnore]
        [System.Text.Json.Serialization.JsonIgnore]
        public string FilePath = "<id>\\";

        /// <summary>
        /// Serializes this object.
        /// Only primitive or string, non-FileDataBase types are considered.
        /// </summary>
        private string Serialization()
        {
            return SerializePrimitives();
        }

        /// <summary>
        /// Serializes only primitive or string-type properties.
        /// </summary>
        /// <returns></returns>
        private string SerializePrimitives()
        {
            var settings = new JsonSerializerSettings();
            settings.ContractResolver = new PrimitiveContractResolver();
            settings.DefaultValueHandling = DefaultValueHandling.Ignore;
            settings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
            settings.Formatting = Formatting.Indented;
            return JsonConvert.SerializeObject(this, settings);
        }

        /// <summary>
        /// Provides functionality to SerializePrimitives().
        /// See: https://stackoverflow.com/questions/15929848/serialize-only-simple-types-using-json-net
        /// </summary>
        private class PrimitiveContractResolver : DefaultContractResolver
        {
            protected override Newtonsoft.Json.Serialization.JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
            {
                var property = base.CreateProperty(member, memberSerialization);

                var propertyType = property.PropertyType;
                if (propertyType.IsPrimitive || propertyType == typeof(string))
                {
                    property.ShouldSerialize = instance => true;
                }
                else
                {
                    property.ShouldSerialize = instance => false;
                }
                return property;
            }
        }

        /// <summary>
        /// Serializes the current class.
        /// </summary>
        /// <returns>True: Success. False otherwise.</returns>
        public bool Serialize()
        {
            try
            {
                logger.Info("Serialize(): Called from " + this.GetType());
                FilePath = WinFileSystem.GetWinReadablePath(FilePath);
                if (FilePath == "<id>\\") // if this is the root of the call, create new Root folder.
                {
                    string prefix = this.GetType().Name + "Root\\";
                    FilePath = this.GetType().Name + "Root\\" + FilePath;
                    if (!Directory.Exists(prefix))
                    {
                        logger.Info("Serialize(): !Directory.Exists(prefix) = " + prefix + ". Creating...");
                        Directory.CreateDirectory(prefix);
                    }
                    else
                        logger.Info("Serialize(): Directory.Exists(prefix) = " + prefix + ".");
                }

                // the path in which to search for existing DataBaseIds.
                string searchPath = FilePath.Substring(0, FilePath.LastIndexOf("<id>\\"));
                CreateFolderStructure(searchPath);
                logger.Info("Serialize() searchPath = " + searchPath);
                if (this.DBid >= 1)
                {
                    logger.Info("Serialize() Update element!");
                    // keep DBid.
                }
                else
                {
                    logger.Info("Serialize() Creating new element! Generating DBid...");
                    // generate new DBid.
                    this.DBid = GetHighestDBidInPath(searchPath) + 1;
                    logger.Info("Serialize() Assigned DBid = " + this.DBid);
                }

                FilePath = FilePath.Replace("<id>", this.DBid.ToString());
                CreateFolderStructure(FilePath);
                // Serialize primitive/string properties here.
                File.WriteAllText(FilePath + this.GetType().Name + ".Primitives.json", this.Serialization());

                /*
                 * Nested FileDataBase Properties below.
                 */

                List<PropertyInfo> properties = this.GetType().GetProperties().ToList();
                foreach (PropertyInfo property in properties)
                {
                    logger.Info("Serialize(): Next property is " + property.Name);
                    try
                    {
                        bool isList = IsPropertyList(property);
                        logger.Info("Serialize(): property.isList = " + isList);
                        if (!isList)
                        {
                            /*MethodInfo m = property.PropertyType.GetMethod("Serialize");
                            m.Invoke(property.GetValue(this), null);*/
                            object objProperty = property.GetValue(this, null);
                            if (objProperty != null)
                            {
                                logger.Info("Serialize(): Got objProperty.GetType() = " + objProperty.GetType());
                                if (objProperty is FileDataBase @base)
                                {
                                    @base.FilePath = FilePath + property.Name + "\\" + @base.FilePath;
                                    @base.Serialize();
                                }
                            }
                            
                        }
                        else
                        {//see: https://stackoverflow.com/questions/937224/propertyinfo-getvalue-how-do-you-index-into-a-generic-parameter-using-reflec
                            Object collection = property.GetValue(this, null);
                            IEnumerable<object> iCollection = (IEnumerable<object>)collection;

                            foreach (object objProperty in iCollection)
                            {
                                logger.Info("Serialize(): Got objProperty.GetType() = " + objProperty.GetType());

                                if (objProperty is FileDataBase @base)
                                {
                                    @base.FilePath = FilePath + property.Name + "\\" + @base.FilePath;
                                    @base.Serialize();
                                }
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        logger.Info("Serialize(): Property " + property.Name + " did not contain Serialize()! " + e.Message);
                    }
                }

                logger.Info("Serialize(): Return True.");
                this.ResetFilePath(); // prevent spilling when Serialize is called on the same object of a sub-class of type FileDataBase.
                return true;
            }
            catch (Exception e)
            {
                logger.Info("Serialize(): Return False/Exception : " + e.Message);
                this.ResetFilePath();
                return false;
            }
        }

        /// <summary>
        /// Deserializes a json file to an object of type T.
        /// </summary>
        /// <typeparam name="T">The output type parameter.</typeparam>
        /// <param name="dbId"></param>
        /// <returns>A new object of Type T or default() if that fails.</returns>
        public T Deserialize<T>(int dbId)
        {
            try
            {
                logger.Info("Deserialize(): Called from " + typeof(T).Name);
                FilePath = WinFileSystem.GetWinReadablePath(FilePath);
                if (FilePath == "<id>\\")
                {
                    string prefix = this.GetType().Name + "Root\\";
                    FilePath = this.GetType().Name + "Root\\" + FilePath;
                    if (!Directory.Exists(prefix)) // Can not find deserialization root.
                    {
                        logger.Info("Deserialize(): !Directory.Exists(prefix) = " + prefix + ". Exiting...");
                        return default;
                    }
                    else
                        logger.Info("Deserialize(): Directory.Exists(prefix) = " + prefix + ".");
                }

                FilePath = FilePath.Replace("<id>", dbId.ToString());
                logger.Info("Deserialize(): FilePath = " + FilePath);
                string primitivesLocation = FilePath + typeof(T).Name + ".Primitives.json";
                string primitivesContent = File.ReadAllText(primitivesLocation);
                logger.Info("Deserialize(): primitivesContent = " + primitivesContent);
                T returnObject = System.Text.Json.JsonSerializer.Deserialize<T>(primitivesContent);
                logger.Info("Deserialize-Serialize(): returnObject = " + JsonConvert.SerializeObject(returnObject)); // status-overview after deser. of primitives/strings.

                /*
                 * Nested Properties below.
                 */

                List<PropertyInfo> properties = this.GetType().GetProperties().ToList();
                foreach (PropertyInfo property in properties)
                {
                    bool isList = IsPropertyList(property);
                    bool isDerivedFileDB = property.PropertyType.IsAssignableTo(typeof(FileDataBase)) || property.PropertyType.IsAssignableTo(typeof(List<FileDataBase>)); // the 'IsAssignableTo' part does not work as intended and is always false for derived type lists.
                    logger.Info("Deserialize(): Property = " + property.Name + " isDerivedFileDB = " + isDerivedFileDB + " isList = " + isList);
                    try
                    {
                        if (!isList && isDerivedFileDB)
                        {
                            Type nonlistType = property.PropertyType;
                            //object objProperty = property.GetValue(this, null); wrong!
                            object objProperty = Activator.CreateInstance(nonlistType); ;
                            string propLocation = FilePath + property.Name + "\\";
                            logger.Info("Deserialize(object): Type is " + nonlistType);

                            if (Directory.Exists(propLocation))
                            {
                                logger.Info("Deserialize(object): Directory.Exists " + propLocation);
                                if (objProperty is FileDataBase @base)
                                {
                                    logger.Info("Deserialize(object): Is derived from FileDataBase!");
                                    @base.FilePath = propLocation + @base.FilePath;
                                    List<string> directories = Directory.GetDirectories(propLocation, "*", new EnumerationOptions() { RecurseSubdirectories = false }).ToList();
                                    string sdirectory = directories.First();
                                    if (sdirectory.Contains(propLocation))
                                        sdirectory = sdirectory.Replace(propLocation, "");

                                    logger.Info("Deserialize(object): In Directory " + sdirectory);

                                    MethodInfo mi = nonlistType.GetMethod("Deserialize").MakeGenericMethod(new Type[] { nonlistType });
                                    List<object> param = new();
                                    param.Add(int.Parse(sdirectory));
                                    object setMe = mi.Invoke(objProperty, param.ToArray());
                                    object thisProp = property.GetValue(returnObject);
                                    property.SetValue(returnObject, setMe);
                                    logger.Info("Deserialize(object): setMe-Ser: " + JsonConvert.SerializeObject(setMe));
                                    logger.Info("Deserialize(object<>): retObj-Ser: " + JsonConvert.SerializeObject(returnObject)); // Status overview for recent obj and return-obj.
                                }
                            }
                        }
                        else if (isList)
                        {//see: https://stackoverflow.com/questions/937224/propertyinfo-getvalue-how-do-you-index-into-a-generic-parameter-using-reflec
                            List<Type> types = property.PropertyType.GenericTypeArguments.ToList();
                            Type listType = types.First();
                            logger.Info("Deserialize(List<>): Type is " + listType.Name);
                            string propLocation = FilePath + property.Name + "\\";

                            if (Directory.Exists(propLocation))
                            {
                                logger.Info("Deserialize(List<>): Directory.Exists " + propLocation);

                                if (listType.IsAssignableTo(typeof(FileDataBase))) // Todo: Apply this technique to bool isDerived... (above)
                                {
                                    logger.Info("Deserialize(List<>): listType is derived from FileDataBase!");
                                    List<string> directories = Directory.GetDirectories(propLocation, "*", new EnumerationOptions() { RecurseSubdirectories = false }).ToList();
                                    foreach (string directory in directories) // directory = 1,2,..n (should be (is not (yikes)))
                                    {
                                        string sdirectory = directory;
                                        if (directory.Contains(propLocation)) //why the F* is this always true??!?!?!? WHYYYYYYYYYYY
                                            sdirectory = directory.Replace(propLocation, "");
                                        logger.Info("Deserialize(List<>): In Directory " + sdirectory);
                                        var instance = Activator.CreateInstance(listType);
                                        if (instance is FileDataBase @base) // HUIuiuiuiui...
                                        {
                                            logger.Info("Deserialize(List<>): instance is derived from FileDataBase!");
                                            @base.FilePath = propLocation + @base.FilePath;
                                            MethodInfo deserializeMethodInfo = listType.GetMethod("Deserialize").MakeGenericMethod(new Type[] { listType });
                                            List<object> param = new();
                                            param.Add(int.Parse(sdirectory));
                                            object AddMeToList = deserializeMethodInfo.Invoke(instance, param.ToArray());
                                            object thisProp = property.GetValue(returnObject);
                                            MethodInfo propMi = property.PropertyType.GetMethod("Add");
                                            propMi.Invoke(thisProp, new object[] { AddMeToList });
                                            logger.Info("Deserialize(List<>): AddMeToList-Ser: " + JsonConvert.SerializeObject(AddMeToList));
                                            logger.Info("Deserialize(List<>): returnObj-Ser: " + JsonConvert.SerializeObject(returnObject)); // Status overview for recent object and return object.
                                        }
                                    }
                                }
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        logger.Info("Deserialize(): Property " + property.Name + " did not contain Deserialize()! " + e.Message);
                    }
                }

                logger.Info("Deserialize(): Return True.");
                return returnObject;
                //return (T)Convert.ChangeType(returnObject, typeof(T));
            }
            catch (Exception e)
            {
                logger.Info("Deserialize(List<>): Return default/Exception : " + e.Message);
                return default;
            }
        }

        /// <summary>
        /// When a class is serialized to the file system, we need to make sure that the specified paths exist.
        /// </summary>
        /// <param name="Path">If this path already exists, fine. Else, create that path.</param>
        public void CreateFolderStructure(string Path)
        {
            Logger logger = LogManager.GetCurrentClassLogger();
            string[] folderList = Path.Split('\\'); //last elem is always "". second-last is the "true" last elem.
            bool isFile = !Path.EndsWith('\\');
            logger.Info("CreateFolderStructure() called on Path=\"" + Path + "\"");
            string currentDirectory = "";

            for (int i = 0; i < folderList.Length; i++)
            {
                if (string.IsNullOrEmpty(folderList[i]))
                    break;
                logger.Info("CreateFolderStructure() STRING s = \"" + folderList[i] + "\"");
                if (i == folderList.Length - 1)
                    if (isFile)
                    {
                        if (!File.Exists(currentDirectory + folderList[i]))
                        {
                            File.Create(currentDirectory + folderList[i]);
                            logger.Info("CreateFolderStructure() !File.Exists, Creating = \"" + currentDirectory + folderList[i] + "\"");
                        }
                        else
                        {
                            logger.Info("CreateFolderStructure() File.Exists = \"" + currentDirectory + folderList[i] + "\"");
                        }

                        break;
                    }

                if (!Directory.Exists(currentDirectory + folderList[i] + "\\"))
                {
                    logger.Info("CreateFolderStructure() !Directory.Exists, Creating = \"" + currentDirectory + folderList[i] + "\\" + "\"");
                    Directory.CreateDirectory(currentDirectory + folderList[i] + "\\");
                }

                currentDirectory = string.Concat(currentDirectory, folderList[i] + "\\");
                logger.Info("CreateFolderStructure() Set currentDirectory = \"" + currentDirectory + "\"");
            }
        }

        /// <summary>
        /// Gets the highest ID in Path example\path\id, where ID is int.
        /// </summary>
        /// <param name="Path">The path to serialized object(s).</param>
        /// <returns>The highest database-ID among those objects.</returns>
        private int GetHighestDBidInPath(string Path)
        {
            Logger logger = LogManager.GetCurrentClassLogger();
            try
            {
                logger.Info("GetHighestDBidInPath() called on Path = " + Path);
                int id = 0;
                string searchPath;
                if (!Path.EndsWith('\\'))
                    searchPath = Path.Substring(0, Path.LastIndexOf('\\') + 1);
                else
                    searchPath = Path;

                logger.Info("GetHighestDBidInPath() searchPath = " + searchPath);
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
                            logger.Info("GetHighestDBidInPath() directory = " + directory + " not Int type!");
                            tmp = -1;
                        }

                        if (tmp > id)
                            id = tmp;
                    }
                }

                logger.Info("GetHighestDBidInPath() Returning = " + id);
                return id;
            }
            catch (Exception e)
            {
                logger.Info("GetHighestDBidInPath() Returning = " + int.MaxValue + ". Exception: " + e.Message);
                return int.MaxValue;
            }
        }

        /// <summary>
        /// Finds out if a property is a List<> type or not.
        /// </summary>
        /// <param name="p">PropertyInfo parameter.</param>
        /// <returns>True: Is a list. False: Is not a list.</returns>
        private bool IsPropertyList(PropertyInfo p)
        {
            try
            {
                return p.PropertyType.GetGenericTypeDefinition() == typeof(List<>);
            }
            catch (Exception e)
            {
                return false;
            }
        }

        /// <summary>
        /// Since FilePath is altered during Serialization, the same object would otherwise not be successfully serializable again.
        /// </summary>
        private void ResetFilePath()
        {
            this.FilePath = "<id>\\";
        }
    }
}
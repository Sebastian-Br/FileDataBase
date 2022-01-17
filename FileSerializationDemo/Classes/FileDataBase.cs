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
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using NLog;
using static FileSerializationDemo.Classes.FileDBEnums;


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
        /// To reuse the serialization at some point.
        /// </summary>
        [FileDataBaseIgnore]
        [Newtonsoft.Json.JsonIgnore]
        [System.Text.Json.Serialization.JsonIgnore]
        public string FirstSerializationPath { get; set; }

        /// <summary>
        /// The FilePath where this object will be serialized.
        /// The id-part will be replaced be automatically replaced by the correct ID.
        /// Do NOT set this manually.
        /// </summary>
        [Newtonsoft.Json.JsonIgnore]
        [System.Text.Json.Serialization.JsonIgnore]
        public string FilePath = "<id>\\";

        [FileDataBaseIgnore]
        [Newtonsoft.Json.JsonIgnore]
        [System.Text.Json.Serialization.JsonIgnore]
        public ObjectHash ThisObjectHash { get; set; }

        /// <summary>
        /// Serializes this object.
        /// Only primitive or string, non-FileDataBase types are considered.
        /// </summary>
        /// <returns>The Serialization of this object.</returns>
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
            settings.ContractResolver = new NewtonsoftJsonX.PrimitiveContractResolver();
            settings.DefaultValueHandling = DefaultValueHandling.Ignore;
            settings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
            settings.Formatting = Formatting.Indented;
            return JsonConvert.SerializeObject(this, settings);
        }

        /// <summary>
        /// A root directory is created only for the first object on which Serialize() is called.
        /// This object should store all the data you want to save to the database.
        /// The name of this directory will be the type name + "Root\"
        /// </summary>
        private void CreateRootDirectory()
        {
            string prefix = this.GetType().Name + "Root\\";
            FilePath = this.GetType().Name + "Root\\" + FilePath;
            if (!Directory.Exists(prefix))
            {
                logger.Info("CreateRootDirectory(): !Directory.Exists(prefix) = " + prefix + ". Creating...");
                Directory.CreateDirectory(prefix);
            }
            else
                logger.Info("CreateRootDirectory(): Directory.Exists(prefix) = " + prefix + ".");
        }

        /// <summary>
        /// Sets this object's DBid to the next available ID.
        /// </summary>
        /// <param name="searchPath">The path in which to search for assignable DBids.</param>
        private void Serialize_SetNextID(string searchPath)
        {
            if (this.DBid >= 1)
            {
                logger.Info("Serialize_SetNextID() Update element!");
                // keep DBid.
            }
            else
            {
                logger.Info("Serialize_SetNextID() Creating new element! Generating DBid...");
                // generate new DBid.
                this.DBid = GetHighestDBidInPath(searchPath) + 1;
                logger.Info("Serialize_SetNextID() Assigned DBid = " + this.DBid);
            }
            FilePath = FilePath.Replace("<id>", this.DBid.ToString());
        }

        /// <summary>
        /// Serializes a non-list property that derives from FileDataBase.
        /// </summary>
        /// <param name="property">The property.</param>
        private void SerializeNonListProperty(PropertyInfo property, SerializationType serializationType)
        {
            bool bSuccess = true; // return success/failure later.
            object objProperty = property.GetValue(this, null);
            string propertyBaseName = GetPropertyBaseName(property);
            if (objProperty != null)
            {
                logger.Info("SerializeNonListProperty(): Got Property = " + property.Name);
                if (objProperty is FileDataBase @base)
                {
                    @base.FilePath = propertyBaseName + @base.FilePath;
                    @base.Serialize(serializationType);
                }
            }
            else if (serializationType == SerializationType.ADD_DISCARDUNUSED)
            {
                logger.Info("SerializeNonListProperty(): Property is null = " + property.Name);
                WinFileSystem.TryDeleteDirectory(propertyBaseName);
            }
        }

        /// <summary>
        /// Serializes a nonlist Property that derives from FileDataBase to the file system.
        /// See: https://stackoverflow.com/questions/937224/propertyinfo-getvalue-how-do-you-index-into-a-generic-parameter-using-reflec
        /// </summary>
        /// <param name="property"></param>
        private void SerializeListProperty(PropertyInfo property, SerializationType serializationType)
        {
            object collection = property.GetValue(this, null);
            string propertyBaseName = GetPropertyBaseName(property);
            if (collection != null)
            {
                IEnumerable<object> iCollection = (IEnumerable<object>)collection;
                List<int> AddedDBids = new();
                foreach (object objProperty in iCollection)
                {
                    logger.Info("SerializeListProperty() Got Property = " + property.Name);
                    if (objProperty is FileDataBase @base)
                    {
                        @base.FilePath = propertyBaseName + @base.FilePath;
                        @base.Serialize(serializationType);
                        AddedDBids.Add(@base.DBid);
                    }
                }

                if(serializationType == SerializationType.ADD_DISCARDUNUSED)
                {
                    if(iCollection.ToList().Count > 0)
                    {
                        List<string> directories = Directory.GetDirectories(propertyBaseName, "*", new EnumerationOptions() { RecurseSubdirectories = false }).ToList();
                        if (directories != null)
                        {
                            foreach (string directory in directories)
                            {
                                try
                                {
                                    string sdirectory = directory;
                                    if (directory.Contains(propertyBaseName))
                                        sdirectory = directory.Replace(propertyBaseName, "");
                                    if (!AddedDBids.Contains(int.Parse(sdirectory)))
                                        WinFileSystem.TryDeleteDirectory(propertyBaseName + sdirectory + "\\");
                                }
                                catch (Exception e)
                                {
                                    logger.Error(e, "SerializeListProperty() Exception in directory loop.");
                                }
                            }
                        }
                    }
                    else  // in case this is a new() List<>.
                    {
                        logger.Info("SerializeListProperty(): List is empty! Trying to delete " + propertyBaseName);
                        WinFileSystem.TryDeleteDirectory(propertyBaseName);
                    }
                }
            }
            else if (serializationType == SerializationType.ADD_DISCARDUNUSED)
            {
                logger.Info("SerializeListProperty(): List<> Property is null = " + property.Name);
                WinFileSystem.TryDeleteDirectory(propertyBaseName);
            }
        }

        /// <summary>
        /// Serializes the current class.
        /// </summary>
        /// <returns>True: Success. False otherwise.</returns>
        public bool Serialize(SerializationType serializationType, int defaultRoot = 0)
        {
            try
            {
                logger.Info("Serialize(): Called from " + this.GetType());
                FilePath = WinFileSystem.GetWinReadablePath(FilePath);
                if (FilePath == "<id>\\") // if this is the root of the call, create new Root folder.
                    CreateRootDirectory();

                // the path in which to search for existing DataBaseIds.
                string searchPath = FilePath.Substring(0, FilePath.LastIndexOf("<id>\\"));
                logger.Info("Serialize() searchPath = " + searchPath);
                WinFileSystem.CreateFolderStructure(searchPath); // creates the folder with the Root name or property name.

                Serialize_SetNextID(searchPath);
                WinFileSystem.CreateFolderStructure(FilePath); // e.g. creates the .../1/ folder.
                bool bUpdatePrimitivesJson = true;
                string SerializationPath = FilePath + this.GetType().Name + ".Primitives.json";

                if (this.ThisObjectHash == null)
                {
                    logger.Info("Serialize() ObjHash is NULL!");
                    this.ThisObjectHash = ReflectionX.GetExtensivePrimitivesHash(this);
                    logger.Info("Serialize() Assigned ObjHash " + this.ThisObjectHash);
                }
                else
                {
                    logger.Info("Serialize() ObjHash is not null!");
                    if (this.ThisObjectHash == ReflectionX.GetExtensivePrimitivesHash(this))
                    {
                        bUpdatePrimitivesJson = false;
                    }
                }

                if (!File.Exists(SerializationPath))
                {
                    logger.Info("Serialize() File does not exist yet " + SerializationPath + ". Setting bUpdatePrimitivesJson to true.");
                    bUpdatePrimitivesJson = true;
                }

                logger.Info("Serialize() bUpdatePrimitivesJson = " + bUpdatePrimitivesJson + " on Type " + this.GetType().Name);
                // Serialize primitive/string properties here.
                if(bUpdatePrimitivesJson)
                {
                    if(string.IsNullOrEmpty(this.FirstSerializationPath))
                        this.FirstSerializationPath = SerializationPath;
                    File.WriteAllText(SerializationPath, this.Serialization());
                }

                /*
                 * Nested FileDataBase Properties below.
                 */

                List<PropertyInfo> properties = this.GetType().GetProperties().ToList();
                foreach (PropertyInfo property in properties)
                {
                    logger.Info("Serialize(): Next property is " + property.Name);
                    try
                    {
                        bool isList = ReflectionX.IsPropertyList(property);
                        logger.Info("Serialize(): property.isList = " + isList);
                        if (!isList)
                            SerializeNonListProperty(property, serializationType);
                        else
                            SerializeListProperty(property, serializationType);
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
                logger.Error("Serialize(): Return False/Exception : " + e.Message);
                this.ResetFilePath();
                return false;
            }
        }

        /// <summary>
        /// Checks if the root of the database is inproper, in which case
        /// deserialization can not succeed.
        /// </summary>
        /// <returns>True: This is a root and it is inproper. False otherwise.</returns>
        private bool Deserialize_CheckIsInproperRoot()
        {
            if (FilePath == "<id>\\")
            {
                string prefix = this.GetType().Name + "Root\\";
                FilePath = this.GetType().Name + "Root\\" + FilePath;
                if (!Directory.Exists(prefix)) // Can not find deserialization root.
                {
                    logger.Error("Deserialize_CheckIsInproperRoot(): !Directory.Exists(prefix) = " + prefix + ". Exiting...");
                    return true;
                }
                else
                    logger.Info("Deserialize_CheckIsInproperRoot(): Directory.Exists(prefix) = " + prefix + ". This is a proper root.");
                return false;
            }

            return false;
        }

        /// <summary>
        /// Deserializes a nonlist type that derives from FileDataBase.
        /// Sets the pertinent property of the current object to the deserialized child-object.
        /// </summary>
        /// <param name="property">The nonlist property.</param>
        /// <param name="returnObject">The current object that houses this property.</param>
        private void DeserializeNonListType(PropertyInfo property, object returnObject)
        {
            Type nonlistType = property.PropertyType;
            object objProperty = Activator.CreateInstance(nonlistType); ;
            string propLocation = FilePath + property.Name + "\\";
            logger.Info("DeserializeNonListType: Type is " + nonlistType);

            if (Directory.Exists(propLocation))
            {
                logger.Info("DeserializeNonListType: Directory.Exists " + propLocation);
                if (objProperty is FileDataBase @base)
                {
                    logger.Info("DeserializeNonListType: Is derived from FileDataBase!");
                    @base.FilePath = propLocation + @base.FilePath;
                    List<string> directories = Directory.GetDirectories(propLocation, "*", new EnumerationOptions() { RecurseSubdirectories = false }).ToList();
                    string sdirectory = directories.First();
                    if (sdirectory.Contains(propLocation))
                        sdirectory = sdirectory.Replace(propLocation, ""); // now contains that Property's DBid

                    logger.Info("DeserializeNonListType: In Directory " + sdirectory);

                    MethodInfo mi = nonlistType.GetMethod("Deserialize").MakeGenericMethod(new Type[] { nonlistType });
                    List<object> param = new();
                    param.Add(int.Parse(sdirectory));
                    object setMe = mi.Invoke(objProperty, param.ToArray());
                    property.SetValue(returnObject, setMe); // for this property, sets the property value.
                    logger.Info("DeserializeNonListType: setMe-Ser: " + JsonConvert.SerializeObject(setMe, Formatting.Indented));
                    logger.Info("DeserializeNonListType: retObj-Ser: " + JsonConvert.SerializeObject(returnObject, Formatting.Indented)); // Status overview for recent obj and return-obj.
                }
            }
        }

        /// <summary>
        /// Deserializes a property of Type List<> where the nested type is derived from FileDataBase.
        /// See: https://stackoverflow.com/questions/937224/propertyinfo-getvalue-how-do-you-index-into-a-generic-parameter-using-reflec
        /// </summary>
        /// <param name="property">The list property.</param>
        /// <param name="returnObject">The current object that has this list property.</param>
        private void DeserializeListType(PropertyInfo property, object returnObject)
        {
            List<Type> types = property.PropertyType.GenericTypeArguments.ToList();
            Type listType = types.First();
            logger.Info("DeserializeListType(): Type is " + listType.Name);
            string propLocation = FilePath + property.Name + "\\";

            if (Directory.Exists(propLocation))
            {
                logger.Info("DeserializeListType(): Directory.Exists " + propLocation);

                if (listType.IsAssignableTo(typeof(FileDataBase)))
                {
                    logger.Info("DeserializeListType(): listType is derived from FileDataBase!");
                    List<string> directories = Directory.GetDirectories(propLocation, "*", new EnumerationOptions() { RecurseSubdirectories = false }).ToList();
                    foreach (string directory in directories) // directory = 1,2,..n (should be (is not (yikes)))
                    {
                        string sdirectory = directory;
                        if (directory.Contains(propLocation)) //why the F* is this always true??!?!?!? WHYYYYYYYYYYY
                            sdirectory = directory.Replace(propLocation, "");
                        logger.Info("DeserializeListType(): In Directory " + sdirectory);
                        var instance = Activator.CreateInstance(listType);
                        if (instance is FileDataBase @base) // HUIuiuiuiui...
                        {
                            logger.Info("DeserializeListType(): instance is derived from FileDataBase!");
                            @base.FilePath = propLocation + @base.FilePath;
                            MethodInfo deserializeMethodInfo = listType.GetMethod("Deserialize").MakeGenericMethod(new Type[] { listType });
                            List<object> param = new();
                            param.Add(int.Parse(sdirectory));
                            object AddMeToList = deserializeMethodInfo.Invoke(instance, param.ToArray());
                            object thisProp = property.GetValue(returnObject);
                            MethodInfo propMi = property.PropertyType.GetMethod("Add");
                            propMi.Invoke(thisProp, new object[] { AddMeToList });
                            logger.Info("DeserializeListType(): AddMeToList-Ser: " + JsonConvert.SerializeObject(AddMeToList, Formatting.Indented));
                            logger.Info("DeserializeListType(): returnObj-Ser: " + JsonConvert.SerializeObject(returnObject, Formatting.Indented)); // Status overview for recent object and return object.
                        }
                    }
                }
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

                if (Deserialize_CheckIsInproperRoot())
                    return default;

                FilePath = FilePath.Replace("<id>", dbId.ToString());
                logger.Info("Deserialize(): FilePath = " + FilePath);

                // Deserialize primitives/strings here:

                string primitivesLocation = FilePath + typeof(T).Name + ".Primitives.json";
                string primitivesContent = File.ReadAllText(primitivesLocation);
                logger.Info("Deserialize(): primitivesContent = " + primitivesContent);
                T returnObject = System.Text.Json.JsonSerializer.Deserialize<T>(primitivesContent);
                logger.Info("Deserialize-Serialize(): returnObject = " + JsonConvert.SerializeObject(returnObject)); // status-overview after deser. of primitives/strings.
                ObjectHash oH = ReflectionX.GetExtensivePrimitivesHash(returnObject);
                PropertyInfo oHprop = returnObject.GetType().GetProperty("ThisObjectHash");
                oHprop.SetValue(returnObject, oH);
                logger.Info("Deserialize() Computed ObjectHash = " + oH);

                /*
                 * Nested :FileDB Properties below.
                 */

                List<PropertyInfo> properties = this.GetType().GetProperties().ToList();
                foreach (PropertyInfo property in properties)
                {
                    bool isList = ReflectionX.IsPropertyList(property);
                    bool isDerivedFileDB = ReflectionX.IsDerivedFileDB(property.PropertyType); // the 'IsAssignableTo' part does not work as intended and is always false for derived type lists.
                    logger.Info("Deserialize(): Property = " + property.Name + " isDerivedFileDB = " + isDerivedFileDB + " isList = " + isList);
                    try
                    {
                        if (!isList && isDerivedFileDB)
                        {
                            DeserializeNonListType(property, returnObject);
                        }
                        else if (isList)
                        {
                            DeserializeListType(property, returnObject);
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
                logger.Error("Deserialize(): Return default/Exception : " + e.Message);
                return default;
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

        private string GetPropertyBaseName(PropertyInfo property)
        {
            return FilePath + property.Name + "\\";
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
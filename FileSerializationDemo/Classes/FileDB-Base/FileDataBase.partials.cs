using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace FileSerializationDemo.Classes
{
    public partial class FileDataBase
    {
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
        /// Checks if the root of the database is improper, in which case
        /// deserialization can not succeed.
        /// </summary>
        /// <returns>True: This is a root and it is inproper. False otherwise.</returns>
        private bool Deserialize_CheckIsImproperRoot()
        {
            if (FilePath == "<id>\\")
            {
                string prefix = this.GetType().Name + "Root\\";
                FilePath = this.GetType().Name + "Root\\" + FilePath;
                if (!Directory.Exists(prefix)) // Can not find deserialization root.
                {
                    logger.Error("Deserialize_CheckIsImproperRoot(): !Directory.Exists(prefix) = " + prefix + ". Exiting...");
                    return true;
                }
                else {
                    logger.Info("Deserialize_CheckIsImproperRoot(): Directory.Exists(prefix) = " + prefix + ". This is a proper root.");
                    this.IsRoot = true;
                }
            }

            return false;
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
        /// Gets the highest ID in Path example\path\id, where ID is int.
        /// </summary>
        /// <param name="Path">The path to serialized object(s).</param>
        /// <returns>The highest database-ID among those objects.</returns>
        private int GetNextDBid(string Path)
        {
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

                id++; // we're interested in the NEXT available id.
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
        /// Sets this object's DBid to the next available ID.
        /// </summary>
        /// <param name="searchPath">The path in which to search for assignable DBids.</param>
        private void Serialize_SetNextID(string searchPath)
        {
            if (this.DBid >= 1)
            {
                logger.Info("Serialize_SetNextID() This is a known element.");
                // keep DBid.
            }
            else
            {
                logger.Info("Serialize_SetNextID() This is a new element! Generating DBid...");
                this.DBid = GetNextDBid(searchPath);
                logger.Info("Serialize_SetNextID() Assigned DBid = " + this.DBid);
            }
            FilePath = FilePath.Replace("<id>", this.DBid.ToString());
        }

        /// <summary>
        /// Returns the directory name in (/from) which a child property will be (/de)serialized.
        /// </summary>
        /// <param name="property">The property.</param>
        /// <returns>The directory.</returns>
        private string GetPropertyBaseDirectory(PropertyInfo property)
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

        private string GetObjectLinqsLocation()
        {
            return FilePath + this.GetType().Name + ".ObjLinqs.json";
        }

        private void SerializeWithAtomicReference(PropertyInfo property)
        {
            if (property.PropertyType.IsAssignableTo(typeof(FileDataBase)))
                return;
            if (property.GetCustomAttribute(typeof(PropertyAlreadySerializedAttribute)) == null)
            {
            }
        }
    }
}
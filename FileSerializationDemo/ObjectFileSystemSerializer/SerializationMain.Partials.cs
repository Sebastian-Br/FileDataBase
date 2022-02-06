using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using ExtensionMethods;

namespace FileSerializationDemo.ObjectFileSystemSerializer
{
    public partial class SerializationMain
    {
        public SerializationMain()
        {
            CurrentSerializationPath = new();
            ObjectDictionary = new();
        }

        private string GetCurrentSerializationDirectory()
        {
            return String.Join("\\", CurrentSerializationPath) + "\\";
        }

        private string GetPrimitiveJsonFileName(PropertyInfo property)
        {
            return property.Name + ".Primitive.json";
        }

        private string GetObjectLinqJsonFileName(object obj)
        {
            return obj.GetType().Name + ".ObjLinq.json";
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
    }
}
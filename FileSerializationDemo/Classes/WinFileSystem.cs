using NLog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileSerializationDemo.Classes
{
    public class WinFileSystem
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// When a class is serialized to the file system, we need to make sure that the specified paths exist.
        /// </summary>
        /// <param name="Path">If this path already exists, fine. Else, create that path.</param>
        /// <returns>True: Success/No exception. False: Exception - some operation failed, the folder structure was probably not created.</returns>
        public static bool CreateFolderStructure(string Path)
        {
            Logger logger = LogManager.GetCurrentClassLogger();

            try
            {
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

                return true;
            }
            catch (Exception e)
            {
                logger.Error(e, "CreateFolderStructure() Exception.");
                return false;
            }
        }


        /// <summary>
        /// Replaces and '/' with '\\'
        /// </summary>
        /// <param name="Path">The path.</param>
        /// <returns>Path, not containing any '/'.</returns>
        public static string GetWinReadablePath(string Path)
        {
            try
            {
                string WinReadablePath = Path.Replace('/', '\\');
                return WinReadablePath;
            }
            catch (Exception e)
            {
                throw new Exception("Path generation failed!");
            }
        }

        /// <summary>
        /// Tries to the delete the directory (and all sub-directories and files) specified @ Path.
        /// Handles exceptions inside of this function.
        /// </summary>
        /// <param name="Path">The path.</param>
        /// <returns>True: The directory was deleted (or it did not exist to begin with). False: There was an exception.</returns>
        public static bool TryDeleteDirectory(string Path)
        {
            try
            {
                if(Directory.Exists(Path))
                {
                    Directory.Delete(Path, true);
                    logger.Info("TryDeleteDirectory() Deleted " + Path);
                }
                else
                {
                    logger.Info("TryDeleteDirectory() " + Path + " does not exist.");
                }

                return true;
            }
            catch (Exception e)
            {
                logger.Error(e, "TryDeleteDirectory() Exception.");
                return false;
            }
        }
    }
}

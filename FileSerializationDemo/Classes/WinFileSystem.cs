using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileSerializationDemo.Classes
{
    public class WinFileSystem
    {
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
    }
}

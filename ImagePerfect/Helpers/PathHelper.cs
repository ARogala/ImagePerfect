using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImagePerfect.Helpers
{
    public static class PathHelper
    {
        public static string GetRegExpString(string path)
        {
            return path.Replace(@"\", @"\\\\") + @"\\\\[^\\\\]+\\\\?$";
        }

        public static string RemoveOneFolderFromPath(string path)
        {
            string[] strArray = path.Split(@"\");
            string newPath = string.Empty;
            for (int i = 0; i < strArray.Length - 1; i++)
            {
                if (i < strArray.Length - 2)
                {
                    newPath = newPath + strArray[i] + @"\";
                }
                else
                {
                    newPath = newPath + strArray[i];
                }
            }
            return newPath;
        }

        public static string RemoveTwoFoldersFromPath(string path)
        {
            string[] strArray = path.Split(@"\");
            string newPath = string.Empty;
            for (int i = 0; i < strArray.Length - 2; i++)
            {
                if (i < strArray.Length - 3)
                {
                    newPath = newPath + strArray[i] + @"\";
                }
                else
                {
                    newPath = newPath + strArray[i];
                }
            }
            return newPath;
        }

        public static string FormatPathForDbStorage(string path)
        {
            return path.Replace(@"\", @"\\");
        }
    }
}

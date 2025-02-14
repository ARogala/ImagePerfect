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
        //regular expression string used in sql with REGEXP_LIKE to get all folders in directory (NOT Their sub folders)
        public static string GetRegExpString(string path)
        {
            return path.Replace(@"\", @"\\\\") + @"\\\\[^\\\\]+\\\\?$";
        }
        //regexp string to get folders and their subfolders
        public static string GetRegExpStringForSubDirectories(string path)
        {
            return path.Replace(@"\",@"\\\\");
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

        public static string FormatPathFromFolderPicker(string path)
        {
            path = path.Replace(@"file:///", "");
            path = path.Remove(path.Length - 1);
            path = path.Replace(@"/", @"\");
            return path;
        }

        public static string FormatPathFromFilePicker(string path)
        {
            path = path.Replace(@"file:///", "");
            path = path.Replace(@"/", @"\");
            return path;
        }
    }
}

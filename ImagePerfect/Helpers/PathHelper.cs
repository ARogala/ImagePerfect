using ImagePerfect.Models;
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
        /*
            say for example you want to move C:\Users\arogala\Documents\CSharp\SamplePictures\space 
            to C:\Users\arogala\Documents\CSharp\SamplePictures\Dad
            All this function does is removes everything in the string before space and adds that onto moveToFolderPath

            C:\Users\arogala\Documents\CSharp\SamplePictures\Dad + \ + space

            it does this for each folder with in space
        
            folder list must contain sub directores if any
         */
        public static List<Folder> ModifyFolderPathsForFolderMove(List<Folder> folders, string currentFolderName, string moveToFolderPath)
        {
            for(int i = 0; i < folders.Count; i++) 
            { 
                string newPath = string.Empty;
                string newCoverImagePath = string.Empty;
                newPath = moveToFolderPath + @"\" + ReturnPartialPath(folders[i].FolderPath, currentFolderName);
                folders[i].FolderPath = newPath;
                if (folders[i].CoverImagePath != "")
                {
                    newCoverImagePath = moveToFolderPath + @"\" + ReturnPartialPath(folders[i].CoverImagePath, currentFolderName);
                    folders[i].CoverImagePath = newCoverImagePath;
                }
                
            }
            return folders;
        }
        //does basically the same thing as the folder method
        public static List<Image> ModifyImagePathsForFolderMove(List<Image> images, string currentFolderName, string moveToFolderPath) 
        {
            for (int i = 0; i < images.Count; i++) 
            {
                string newPath = string.Empty;
                string newImageFolderPath = string.Empty;
                newPath = moveToFolderPath + @"\" + ReturnPartialPath(images[i].ImagePath, currentFolderName);
                newImageFolderPath = moveToFolderPath + @"\" + ReturnPartialPath(images[i].ImageFolderPath, currentFolderName);
                images[i].ImagePath = newPath;
                images[i].ImageFolderPath = newImageFolderPath;
            }
            return images;
        }
        //returns path to end starting at the provied folderName
        //path must contain folderName
        private static string ReturnPartialPath(string path, string folderName)
        {
            int indexOfCurrentFolder = path.IndexOf(folderName);
            string pathToCurrentFolder = path.Remove(indexOfCurrentFolder);
            string partialPath = path.Replace(pathToCurrentFolder, "");
            return partialPath;
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

        public static string BuildFolderSqlForFolderMove(List<Folder> folders)
        {
            StringBuilder sb = new StringBuilder("UPDATE folders SET FolderPath = CASE ");
            foreach (Folder folder in folders) 
            {
                sb.Append($"WHEN FolderId = {folder.FolderId} THEN '{FormatPathForDbStorage(folder.FolderPath)}' ");
            }
            sb.Append("ELSE FolderPath End, CoverImagePath = CASE ");
            foreach (Folder folder in folders)
            {
                if (folder.CoverImagePath != "")
                {
                    sb.Append($"WHEN FolderId = {folder.FolderId} THEN '{FormatPathForDbStorage(folder.CoverImagePath)}' ");
                }
            }
            sb.Append("ELSE CoverImagePath END WHERE FolderId In (");
            for (int i = 0; i < folders.Count; i++)
            {
                if (i < folders.Count - 1)
                {
                    sb.Append($"{folders[i].FolderId},");
                }
                else
                {
                    sb.Append($"{folders[i].FolderId}");
                }
            }
            sb.Append(");");
            return sb.ToString();
        }
    }
}

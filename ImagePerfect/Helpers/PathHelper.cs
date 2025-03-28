using ImagePerfect.Models;
using ImagePerfect.ViewModels;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImagePerfect.Helpers
{
    //this whole class will need platform specific code to handle file paths. Linux vs Windows paths will be different
    public static class PathHelper
    {
        private static string pathSlash = string.Empty;

        private static string getPathSlash()
        {
            #if WINDOWS
            pathSlash = @"\";
            #else
            pathSlash = @"/";
            #endif
            return pathSlash;
        }
        
        public static string FormatPathForLikeOperator(string path)
        {
            #if WINDOWS
            return path.Replace(@"\", @"\\\\") + @"%";
            #else
            return path + @"%";
            #endif
        }

        //regular expression string used in sql with REGEXP_LIKE to get all folders in directory (NOT Their sub folders)
        //Only gets the folders in the path -- the folder itself or any sub directories within each folder are not returned
        public static string GetRegExpStringAllFoldersInDirectory(string path)
        {
            #if WINDOWS
            return path.Replace(@"\", @"\\\\") + @"\\\\[^\\\\]+\\\\?$";
            #else
            return path + @"/[^/]+/?$";
            #endif
        }
        //regexp string to get folder and all subfolders
        //gets the folder itself as well as all folders and subfolders within. 
        //the entire directory tree of the path
        public static string GetRegExpStringDirectoryTree(string path)
        {
            #if WINDOWS
            return path.Replace(@"\",@"\\\\");
            #else
            return path;
            #endif
        }
        public static string AddNewFolderNameToPathForDirectoryMoveFolder(string newFolderPath, string newFolderName)
        {
            #if WINDOWS
            return newFolderPath + @"\" + newFolderName;
            #else
            return newFolderPath + @"/" + newFolderName;
            #endif
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
                newPath = moveToFolderPath + getPathSlash() + ReturnPartialPath(folders[i].FolderPath, currentFolderName);
                folders[i].FolderPath = newPath;
                if (folders[i].CoverImagePath != "")
                {
                    newCoverImagePath = moveToFolderPath + getPathSlash() + ReturnPartialPath(folders[i].CoverImagePath, currentFolderName);
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
                newPath = moveToFolderPath + getPathSlash() + ReturnPartialPath(images[i].ImagePath, currentFolderName);
                newImageFolderPath = moveToFolderPath + getPathSlash() + ReturnPartialPath(images[i].ImageFolderPath, currentFolderName);
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
            string[] strArray = path.Split(getPathSlash());
            string newPath = string.Empty;
            for (int i = 0; i < strArray.Length - 1; i++)
            {
                if (i < strArray.Length - 2)
                {
                    newPath = newPath + strArray[i] + getPathSlash();
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
            string[] strArray = path.Split(getPathSlash());
            string newPath = string.Empty;
            for (int i = 0; i < strArray.Length - 2; i++)
            {
                if (i < strArray.Length - 3)
                {
                    newPath = newPath + strArray[i] + getPathSlash();
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
            #if WINDOWS
            return path.Replace(@"\", @"\\");
            #else
            return path;
            #endif
        }

        public static string FormatPathFromFolderPicker(string path)
        {
            path = path.Replace(@"file:///", "");
            path = path.Remove(path.Length - 1);
            #if WINDOWS
            path = path.Replace(@"/", @"\");
            #else
            path = getPathSlash() + path;
            #endif
            return path;
        }

        public static string FormatPathFromFilePicker(string path)
        {
            path = path.Replace(@"file:///", "");
            #if WINDOWS
            path = path.Replace(@"/", @"\");
            #else
            path = getPathSlash() + path;
            #endif
            return path;
        }

        //this needs obvious improvement -- maybe have user select path and store in db. 
        public static string GetExternalImageViewerExePath()
        {
            #if WINDOWS 
            return @"C:\Program Files\nomacs\bin\nomacs.exe";
            #else
            return @"/usr/bin/eog";
            #endif
        }

        //this needs obvious improvement -- maybe have user select path and store in db. 
        public static string GetExternalFileExplorerExePath()
        {
            #if WINDOWS
            return @"C:\Windows\explorer.exe";
            #else
            return @"/usr/bin/nautilus";
            #endif
        }

        public static string FormatImageFilePathForProcessStart(string imagePath)
        {
            //https://stackoverflow.com/questions/1857325/c-sharp-easiest-way-to-parse-filename-with-spaces-eg-c-test-file-with-space
            //escape the " to wrap the path in " like this "C:\pictures\pic with spaces.jpg"
            //this allows the app to open files that have spaces in their names.
            return "\"" + imagePath + "\"";
        }

        public static string GetTrashFolderPath(string rootFolderPath)
        {
            return rootFolderPath + getPathSlash() + "ImagePerfectTRASH";
        }
        public static string GetImageFileTrashPath(ImageViewModel imageVm, string trashFolderPath)
        {
            //add a guid to guarantee no image in trash has the same name
            Guid g = Guid.NewGuid();
            return trashFolderPath + getPathSlash() + g + imageVm.FileName;
        }

        public static string GetFolderTrashPath(FolderViewModel folderVm, string trashFolderPath)
        {
            //add a guid to guarantee no folder in trash has the same name
            Guid g = Guid.NewGuid();
            return trashFolderPath + getPathSlash() + g + folderVm.FolderName;
        }

        public static string GetNewFolderPath(string currentDirectory, string newFolderName)
        {
            return currentDirectory + getPathSlash() + newFolderName;
        }
    }
}

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
        //regular expression string used in sql with REGEXP_LIKE to get all folders in directory (NOT Their sub folders)
        public static string GetRegExpString(string path)
        {
            return path.Replace(@"\", @"\\\\") + @"\\\\[^\\\\]+\\\\?$";
        }
        //regexp string to get folder and their subfolders
        public static string GetRegExpStringForSubDirectories(string path)
        {
            return path.Replace(@"\",@"\\\\");
        }
        public static string AddNewFolderNameToPathForDirectoryMoveFolder(string newFolderPath, string newFolderName)
        {
            return newFolderPath + @"\" + newFolderName;
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

        //this needs obvious improvement -- maybe have user select path and store in db. 
        public static string GetExternalImageViewerExePath()
        {
            return @"C:\Program Files\nomacs\bin\nomacs.exe";
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
            return rootFolderPath + @"\" + "ImagePerfectTRASH";
        }
        public static string GetImageFileTrashPath(ImageViewModel imageVm, string trashFolderPath)
        {
            //add a guid to guarantee no image in trash has the same name
            Guid g = Guid.NewGuid();
            return trashFolderPath + @"\" + g + Path.GetFileName(imageVm.ImagePath);
        }

        public static string GetFolderTrashPath(FolderViewModel folderVm, string trashFolderPath)
        {
            //add a guid to guarantee no folder in trash has the same name
            Guid g = Guid.NewGuid();
            return trashFolderPath + @"\" + g + folderVm.FolderName;
        }
    }
}

using ImagePerfect.Helpers;
using ImagePerfect.Models;
using ImagePerfect.ViewModels;
using System;
using System.Threading.Tasks;


namespace ImagePerfect.ObjectMappers
{
    public static class FolderMapper
    {
        public static async Task<FolderViewModel> GetFolderVm(Folder folder)
        {
            FolderViewModel folderViewModel = new() 
            {
                FolderId = folder.FolderId,
                FolderName = folder.FolderName,
                FolderPath = folder.FolderPath,
                HasChildren = folder.HasChildren,
                CoverImageBitmap = folder.CoverImagePath == "" ? ImageHelper.LoadFromResource(new Uri("avares://ImagePerfect/Assets/computer-folder-dual-tone-icon.png")) : await ImageHelper.FormatImage(folder.CoverImagePath),
                CoverImagePath = folder.CoverImagePath,
                FolderDescription = folder.FolderDescription,
                FolderRating = folder.FolderRating,
                HasFiles = folder.HasFiles,
                IsRoot = folder.IsRoot,
                FolderContentMetaDataScanned = folder.FolderContentMetaDataScanned,
                AreImagesImported = folder.AreImagesImported,
                ShowImportImagesButton = folder.HasFiles == true && folder.AreImagesImported == false ? true : false,
            };
            return folderViewModel;
        }

        public static Folder GetFolderFromVm(FolderViewModel folderVm)
        {
            Folder folder = new()
            {
                FolderId = folderVm.FolderId,
                FolderName = folderVm.FolderName,
                FolderPath = folderVm.FolderPath,
                HasChildren = folderVm.HasChildren,
                CoverImagePath = folderVm.CoverImagePath,
                FolderDescription = folderVm.FolderDescription,
                FolderRating = folderVm.FolderRating,
                HasFiles = folderVm.HasFiles,
                IsRoot = folderVm.IsRoot,
                FolderContentMetaDataScanned= folderVm.FolderContentMetaDataScanned,
                AreImagesImported= folderVm.AreImagesImported,
            };
            return folder;
        }
    }
}

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
                CoverImagePath = folder.CoverImagePath == "" ? ImageHelper.LoadFromResource(new Uri("avares://ImagePerfect/Assets/icons8-folder-600.png")) : await ImageHelper.FormatImage(folder.CoverImagePath),
                FolderDescription = folder.FolderDescription,
                FolderTags = folder.FolderTags,
                FolderRating = folder.FolderRating,
                HasFiles = folder.HasFiles,
                IsRoot = folder.IsRoot,
                FolderContentMetaDataScanned = folder.FolderContentMetaDataScanned,
                AreImagesImported = folder.AreImagesImported,
                ShowImportImagesButton = folder.HasFiles == true && folder.AreImagesImported == false ? true : false,
            };
            return folderViewModel;
        }
    }
}

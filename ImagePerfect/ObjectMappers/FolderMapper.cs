using ImagePerfect.Helpers;
using ImagePerfect.Models;
using ImagePerfect.ViewModels;
using System;
using System.Collections.Generic;
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
                FolderTags = MapTagsListToString(folder.Tags),
                FolderRating = folder.FolderRating,
                HasFiles = folder.HasFiles,
                IsRoot = folder.IsRoot,
                FolderContentMetaDataScanned = folder.FolderContentMetaDataScanned,
                AreImagesImported = folder.AreImagesImported,
                ShowImportImagesButton = folder.HasFiles == true && folder.AreImagesImported == false ? true : false,
                Tags = folder.Tags,
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

        private static string MapTagsListToString(List<Tag> tags)
        {
            string tagString = string.Empty;
            if(tags.Count == 0)
            {
                return tagString;
            }
            for (int i = 0; i < tags.Count; i++)
            {
                if (i == 0)
                {
                    tagString = tags[i].TagName;
                }
                else
                {
                    tagString = tagString + "," + tags[i].TagName;
                }
            }
            return tagString;
        }
    }
}

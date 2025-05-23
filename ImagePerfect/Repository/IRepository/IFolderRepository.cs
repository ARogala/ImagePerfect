﻿using ImagePerfect.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ImagePerfect.Repository.IRepository
{
    public interface IFolderRepository : IRepository<Folder>
    {
        //any Folder model specific database methods here
        Task<bool> AddFolderCsv(string filePath);
        Task<Folder?> GetRootFolder();
        Task<(List<Folder> folders, List<FolderTag> tags)> GetFoldersInDirectory(string directoryPath);
        Task<Folder> GetFolderAtDirectory(string directoryPath);
        Task<(List<Folder> folders, List<FolderTag> tags)> GetAllFoldersAtRating(int rating, bool filterInCurrentDirectory, string currentDirectory);
        Task<(List<Folder> folders, List<FolderTag> tags)> GetAllFoldersWithNoImportedImages(bool filterInCurrentDirectory, string currentDirectory);
        Task<(List<Folder> folders, List<FolderTag> tags)> GetAllFoldersWithMetadataNotScanned(bool filterInCurrentDirectory, string currentDirectory);
        Task<(List<Folder> folders, List<FolderTag> tags)> GetAllFoldersWithoutCovers(bool filterInCurrentDirectory, string currentDirectory);
        Task<(List<Folder> folders, List<FolderTag> tags)> GetAllFoldersWithTag(string tag, bool filterInCurrentDirectory, string currentDirectory);
        Task<(List<Folder> folders, List<FolderTag> tags)> GetAllFoldersWithDescriptionText(string text, bool filterInCurrentDirectory, string currentDirectory);
        Task<(List<Folder> folders, List<FolderTag> tags)> GetAllFavoriteFolders();
        Task<List<Folder>> GetDirectoryTree(string directoryPath);
        Task<bool> AddCoverImage(string coverImagePath, int folderId);
        Task<bool> MoveFolder(string folderMoveSql, string imageMoveSql);
        Task<bool> UpdateFolderTags(Folder folder, string newTag);
        Task<bool> DeleteFolderTag(FolderTag tag);
        Task<bool> DeleteLibrary();
        Task SaveFolderToFavorites(int  folderId);
        Task DeleteAllFavoriteFolders();
    }
}

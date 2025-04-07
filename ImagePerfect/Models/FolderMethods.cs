using ImagePerfect.Repository.IRepository;
using ImagePerfect.ViewModels;
using System.Collections.Generic;
using System.Threading.Tasks;


namespace ImagePerfect.Models
{
    public class FolderMethods
    {
        private readonly IUnitOfWork _unitOfWork;
        
        public FolderMethods(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<List<Folder>> GetAllFolders()
        {
            return (List<Folder>)await _unitOfWork.Folder.GetAll();
        }

        public async Task<Folder?> GetRootFolder()
        {
            return await _unitOfWork.Folder.GetRootFolder();
        }

        public async Task<(List<Folder> folders, List<FolderTag> tags)> GetFoldersInDirectory(string directoryPath)
        {
            return await _unitOfWork.Folder.GetFoldersInDirectory(directoryPath);
        }

        public async Task<Folder> GetFolderAtDirectory(string directoryPath)
        {
            return await _unitOfWork.Folder.GetFolderAtDirectory(directoryPath);
        }

        public async Task<(List<Folder> folders, List<FolderTag> tags)> GetAllFoldersAtRating(int rating, bool filterInCurrentDirectory, string currentDirectory)
        {
            return await _unitOfWork.Folder.GetAllFoldersAtRating(rating, filterInCurrentDirectory, currentDirectory);
        }

        public async Task<(List<Folder> folders, List<FolderTag> tags)> GetAllFoldersWithTag(string tag, bool filterInCurrentDirectory, string currentDirectory)
        {
            return await _unitOfWork.Folder.GetAllFoldersWithTag(tag, filterInCurrentDirectory, currentDirectory);
        }

        public async Task<(List<Folder> folders, List<FolderTag> tags)> GetAllFoldersWithDescriptionText(string text, bool filterInCurrentDirectory, string currentDirectory)
        {
            return await _unitOfWork.Folder.GetAllFoldersWithDescriptionText(text, filterInCurrentDirectory, currentDirectory);
        }

        public async Task<(List<Folder> folders, List<FolderTag> tags)> GetAllFavoriteFolders()
        {
            return await _unitOfWork.Folder.GetAllFavoriteFolders();
        }

        public async Task<List<Folder>> GetDirectoryTree(string directoryPath)
        {
            return await _unitOfWork.Folder.GetDirectoryTree(directoryPath);
        }

        public async Task<bool> DeleteAllFolders()
        {
            return await _unitOfWork.Folder.DeleteLibrary();
        }

        public async Task<bool> UpdateFolder(Folder folder)
        {
            return await _unitOfWork.Folder.Update(folder);
        }

        public async Task<bool> UpdateCoverImage(string coverImagePath, int folderId)
        {
            return await _unitOfWork.Folder.AddCoverImage(coverImagePath, folderId);
        }

        public async Task<bool> MoveFolder(string folderMoveSql, string imageMoveSql)
        {
            return await _unitOfWork.Folder.MoveFolder(folderMoveSql, imageMoveSql);
        }

        public async Task<bool> DeleteFolder(int id)
        {
            return await _unitOfWork.Folder.Delete(id);
        }

        public async Task<bool> CreateNewFolder(Folder newFolder)
        {
            return await _unitOfWork.Folder.Add(newFolder);
        }

        public async Task<bool> UpdateFolderTags(Folder folder, string newTag)
        {
            return await _unitOfWork.Folder.UpdateFolderTags(folder, newTag);
        }

        public async Task<bool> DeleteFolderTag(FolderTag tag)
        {
            return await _unitOfWork.Folder.DeleteFolderTag(tag);
        }

        public async Task SaveFolderToFavorites(int folderId)
        {
            await _unitOfWork.Folder.SaveFolderToFavorites(folderId);
        }

        public async Task RemoveAllFavoriteFolders()
        {
            await _unitOfWork.Folder.DeleteAllFavoriteFolders();
        }
    }
}

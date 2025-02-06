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

        public async Task<List<Folder>> GetFoldersInDirectory(string directoryPath)
        {
            return await _unitOfWork.Folder.GetFoldersInDirectory(directoryPath);
        }

        public async Task<bool> DeleteAllFolders()
        {
            return await _unitOfWork.Folder.DeleteAll();
        }

        public async Task<bool> AddFolderDescription(Folder folder)
        {
            return await _unitOfWork.Folder.Update(folder);
        }

        public async Task<bool> AddFolderTags(Folder folder)
        {
            return await _unitOfWork.Folder.Update(folder);
        }

        public async Task<bool> AddFolderRating(Folder folder)
        {
            return await _unitOfWork.Folder.Update(folder);
        }
    }
}

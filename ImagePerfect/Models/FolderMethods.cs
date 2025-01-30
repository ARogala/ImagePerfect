using ImagePerfect.Repository.IRepository;
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

        public async Task<Folder> GetRootFolder()
        {
            return await _unitOfWork.Folder.GetRootFolder();
        }
    }
}

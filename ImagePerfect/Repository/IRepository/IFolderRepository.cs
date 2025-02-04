using ImagePerfect.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ImagePerfect.Repository.IRepository
{
    public interface IFolderRepository : IRepository<Folder>
    {
        //any Folder model specific database methods here
        Task<bool> AddFolderCsv(string filePath);
        Task<Folder?> GetRootFolder();
        Task<List<Folder>> GetFoldersInDirectory(string directoryPath);
    }
}

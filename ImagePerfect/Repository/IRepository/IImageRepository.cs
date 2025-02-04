using ImagePerfect.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ImagePerfect.Repository.IRepository
{
    public interface IImageRepository : IRepository<Image>
    {
        //any Image model sepecific database methods here
        Task<List<Image>> GetAllImagesInFolder(int folderId);
        Task<bool> AddImageCsv(string filePath, int folderId);
    }
}

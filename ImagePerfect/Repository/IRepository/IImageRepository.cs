using ImagePerfect.Models;
using System.Threading.Tasks;

namespace ImagePerfect.Repository.IRepository
{
    public interface IImageRepository : IRepository<Image>
    {
        //any Image model sepecific database methods here
        Task<bool> AddImageCsv(string filePath);
    }
}

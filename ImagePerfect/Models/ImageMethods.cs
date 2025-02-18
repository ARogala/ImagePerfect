using ImagePerfect.Repository.IRepository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImagePerfect.Models
{
    public class ImageMethods
    {
        private readonly IUnitOfWork _unitOfWork;

        public ImageMethods(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<List<Image>> GetAllImagesInFolder(int folderId)
        {
            return await _unitOfWork.Image.GetAllImagesInFolder(folderId);
        }

        public async Task<List<Image>> GetAllImagesInFolder(string folderPath)
        {
            return await _unitOfWork.Image.GetAllImagesInFolder(folderPath);
        }

        public async Task<List<Image>> GetImagesInDirectory(string directoryPath)
        {
            return await _unitOfWork.Image.GetImagesInDirectory(directoryPath);
        }

        public async Task<bool> UpdateImage(Image image)
        {
            return await _unitOfWork.Image.Update(image);
        }

        public async Task<bool> DeleteImage(int id)
        {
            return await _unitOfWork.Image.Delete(id);
        }
    }
}

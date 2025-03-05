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

        public async Task<(List<Image> images, List<ImageTag> tags)> GetAllImagesInFolder(int folderId)
        {
            return await _unitOfWork.Image.GetAllImagesInFolder(folderId);
        }

        public async Task<(List<Image> images, List<ImageTag> tags)> GetAllImagesInFolder(string folderPath)
        {
            return await _unitOfWork.Image.GetAllImagesInFolder(folderPath);
        }

        public async Task<List<Image>> GetAllImagesInDirectoryTree(string directoryPath)
        {
            return await _unitOfWork.Image.GetAllImagesInDirectoryTree(directoryPath);
        }

        public async Task<bool> UpdateImage(Image image)
        {
            return await _unitOfWork.Image.Update(image);
        }

        public async Task<bool> UpdateImageTags(Image image, string newTag)
        {
            return await _unitOfWork.Image.UpdateImageTags(image, newTag);
        }

        public async Task<bool> DeleteImageTag(ImageTag tag)
        {
            return await _unitOfWork.Image.DeleteImageTag(tag);
        }

        public async Task<List<string>> GetTagsList()
        {
            return await _unitOfWork.Image.GetTagsList();
        }

        public async Task<bool> DeleteImage(int id)
        {
            return await _unitOfWork.Image.Delete(id);
        }

        public async Task UpdateImageTagFromMetaData(ImageTag tag)
        {
            await _unitOfWork.Image.UpdateImageTagFromMetaData(tag);
        }

        public async Task<bool> UpdateImageRatingFromMetaData(string imageUpdateSql, int folderId)
        {
            return await _unitOfWork.Image.UpdateImageRatingFromMetaData(imageUpdateSql, folderId);
        }

        public async Task ClearImageTagsJoinForMetaData(Image image)
        {
            await _unitOfWork.Image.ClearImageTagsJoinForMetaData(image);
        }
    }
}

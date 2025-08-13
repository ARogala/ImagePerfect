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

        public async Task<(List<Image> images, List<ImageTag> tags)> GetAllImagesAtRating(int rating, bool filterInCurrentDirectory, string currentDirectory)
        {
            return await _unitOfWork.Image.GetAllImagesAtRating(rating, filterInCurrentDirectory, currentDirectory);
        }

        public async Task<(List<Image> images, List<ImageTag> tags)> GetAllImagesWithTag(string tag, bool filterInCurrentDirectory, string currentDirectory)
        {
            return await _unitOfWork.Image.GetAllImagesWithTag(tag, filterInCurrentDirectory, currentDirectory);
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

        public async Task<bool> AddMultipleImageTags(string sql)
        {
            return await _unitOfWork.Image.AddMultipleImageTags(sql);
        }

        public async Task<bool> DeleteImageTag(ImageTag tag)
        {
            return await _unitOfWork.Image.DeleteImageTag(tag);
        }

        public async Task<bool> DeleteSelectedImages(string sql)
        {
            return await _unitOfWork.Image.DeleteSelectedImages(sql);
        }

        public async Task<bool> MoveSelectedImageToNewFolder(string sql)
        {
            return await _unitOfWork.Image.MoveSelectedImageToNewFolder(sql);
        }

        public async Task<List<Tag>> GetTagsList()
        {
            return await _unitOfWork.Image.GetTagsList();
        }

        public async Task<bool> DeleteImage(int id)
        {
            return await _unitOfWork.Image.Delete(id);
        }

        public async Task<bool> UpdateImageTagsAndRatingFromMetaData(List<Image> imagesPlusUpdatedMetaData, int folderId)
        {
            return await _unitOfWork.Image.UpdateImageTagsAndRatingFromMetaData(imagesPlusUpdatedMetaData, folderId);
        }
        
        public async Task<bool> RemoveTagOnAllImages(Tag selectedTag)
        {
            return await _unitOfWork.Image.RemoveTagOnAllImages(selectedTag);
        }

        public async Task<int> GetTotalImages()
        {
           return await _unitOfWork.Image.GetTotalImages();
        }
    }
}

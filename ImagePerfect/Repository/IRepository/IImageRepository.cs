using ImagePerfect.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ImagePerfect.Repository.IRepository
{
    public interface IImageRepository : IRepository<Image>
    {
        //any Image model sepecific database methods here
        Task<(List<Image> images, List<ImageTag> tags)> GetAllImagesInFolder(int folderId);
        Task<(List<Image> images, List<ImageTag> tags)> GetAllImagesInFolder(string folderPath);
        Task<(List<Image> images, List<ImageTag> tags)> GetAllImagesAtRating(int rating, bool filterInCurrentDirectory, string currentDirectory);
        Task<(List<Image> images, List<ImageTag> tags)> GetAllImagesWithTag(string tag, bool filterInCurrentDirectory, string currentDirectory);
        Task<List<Image>> GetAllImagesInDirectoryTree(string directoryPath);
        Task<bool> AddImageCsv(string filePath, int folderId);
        Task<bool> UpdateImageTags(Image image, string newTag);
        Task<bool> AddMultipleImageTags(string sql);
        Task<List<Tag>> GetTagsList();
        Task<bool> UpdateImageTagFromMetaData(List<Image> imagesPlusUpdatedMetaData);
        Task<bool> UpdateImageRatingFromMetaData(string imageUpdateSql, int folderId);
        Task<bool> DeleteImageTag(ImageTag tag);
        Task<bool> DeleteSelectedImages(string sql);
        Task<bool> MoveSelectedImageToNewFolder(string sql);
        Task<int> GetTotalImages();
    }
}

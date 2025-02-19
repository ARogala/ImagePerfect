using ImagePerfect.Helpers;
using ImagePerfect.Models;
using ImagePerfect.ViewModels;
using System.Threading.Tasks;

namespace ImagePerfect.ObjectMappers
{
    public static class ImageMapper
    {
        public static async Task<ImageViewModel> GetImageVm(Image image)
        {
            ImageViewModel imageVm = new()
            {
                ImageId = image.ImageId,
                ImageBitmap = await ImageHelper.FormatImage(image.ImagePath),
                ImagePath = image.ImagePath,
                FileName = image.FileName,
                ImageTags = image.ImageTags,
                ImageRating = image.ImageRating,
                ImageFolderPath = image.ImageFolderPath,
                ImageMetaDataScanned = image.ImageMetaDataScanned,
                FolderId = image.FolderId,
            };
            return imageVm;
        }

        public static Image GetImageFromVm(ImageViewModel imageVm) 
        {
            Image image = new() 
            { 
                ImageId= imageVm.ImageId,
                ImagePath = imageVm.ImagePath,
                FileName = imageVm.FileName,
                ImageTags = imageVm.ImageTags,
                ImageRating = imageVm.ImageRating,
                ImageFolderPath = imageVm.ImageFolderPath,
                ImageMetaDataScanned = imageVm.ImageMetaDataScanned,
                FolderId = imageVm.FolderId,
            };
            return image;
        }
    }
}

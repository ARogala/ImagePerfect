using ImagePerfect.Helpers;
using ImagePerfect.Models;
using ImagePerfect.ViewModels;
using System.Collections.Generic;
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
                ImageTags = MapTagsListToString(image.Tags),
                ImageRating = image.ImageRating,
                ImageFolderPath = image.ImageFolderPath,
                ImageMetaDataScanned = image.ImageMetaDataScanned,
                FolderId = image.FolderId,
                Tags = image.Tags,
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
                ImageRating = imageVm.ImageRating,
                ImageFolderPath = imageVm.ImageFolderPath,
                ImageMetaDataScanned = imageVm.ImageMetaDataScanned,
                FolderId = imageVm.FolderId,
                Tags = imageVm.Tags,
            };
            return image;
        }

        public static Image MapTagsToImage(Image image, List<ImageTag> imageTags)
        {
            foreach (ImageTag imageTag in imageTags)
            {
                if (imageTag.ImageId == image.ImageId)
                {
                    image.Tags.Add(imageTag);
                }
            }
            return image;
        }
        private static string MapTagsListToString(List<ImageTag> tags)
        {
            string tagString = string.Empty;
            if (tags.Count == 0) 
            {
                return tagString;
            }
            for (int i = 0; i < tags.Count; i++) 
            {
                if (i == 0)
                {
                    tagString = tags[i].TagName;
                }
                else
                {
                    tagString = tagString + "," + tags[i].TagName;
                }
            }
            return tagString;
        }
    }
}

using ImagePerfect.Helpers;
using ImagePerfect.Models;
using ImagePerfect.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
                ImagePath = await ImageHelper.FormatImage(image.ImagePath),
                ImageTags = image.ImageTags,
                ImageRating = image.ImageRating,
                ImageFolderPath = image.ImageFolderPath,
                ImageMetaDataScanned = image.ImageMetaDataScanned,
                FolderId = image.FolderId,
            };
            return imageVm;
        }
    }
}

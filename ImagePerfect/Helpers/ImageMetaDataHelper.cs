using ImagePerfect.Models;
using ImageSharp = SixLabors.ImageSharp;
using System.Collections.Generic;
using System.Linq;
using ImagePerfectImage = ImagePerfect.Models.Image;
using ImagePerfect.ViewModels;
using System.Diagnostics;
using SixLabors.ImageSharp.Metadata.Profiles.Iptc;
using SixLabors.ImageSharp.Metadata.Profiles.Exif;
using System;
using System.Threading.Tasks;
using SixLabors.ImageSharp;
//https://aaronbos.dev/posts/iptc-metadata-csharp-imagesharp
namespace ImagePerfect.Helpers
{
    public static class ImageMetaDataHelper
    {
        public static async Task<List<ImagePerfectImage>> ScanImagesForMetaData(List<ImagePerfectImage> images)
        {
            foreach (var image in images)
            {
                ImageSharp.ImageInfo imageInfo = await ImageSharp.Image.IdentifyAsync(image.ImagePath);
                UpdateMetadata(imageInfo, image);   
            }
            return images;
        }

        public static async Task WriteTagToImage(ImageViewModel imageVm)
        {
            ImageSharp.Image imageSharpImage = await ImageSharp.Image.LoadAsync(imageVm.ImagePath);
            await WriteKeywordToImage(imageSharpImage, imageVm);
        }

        public static async void AddRatingToImage(ImagePerfectImage image)
        {
            ImageSharp.Image imageSharpImage = await ImageSharp.Image.LoadAsync(image.ImagePath);
            WriteRatingToImage(imageSharpImage, image);
        }

        //adds the image metadata to the ImagePerfect Image object
        private static void UpdateMetadata(ImageSharp.ImageInfo imageInfo, ImagePerfectImage image)
        {
            image.Tags.Clear();
            if (imageInfo.Metadata.IptcProfile?.Values?.Any() == true)
            {
                foreach (var prop in imageInfo.Metadata.IptcProfile.Values)
                {
                    if (prop.Tag == IptcTag.Keywords)
                    {
                        ImageTag imageTag = new() 
                        { 
                            TagName = prop.Value,
                            ImageId = image.ImageId,
                        };
                        image.Tags.Add(imageTag);
                    }
                }
            }

            //shotwell rating is in exifprofile
            if (imageInfo.Metadata.ExifProfile?.Values?.Any() == true)
            {
                foreach (var prop in imageInfo.Metadata.ExifProfile.Values)
                {
                    if (prop.Tag == ExifTag.Rating)
                    {
                        image.ImageRating = Convert.ToInt32(prop.GetValue());
                    }
                }   
            }
        }

        //clear all the current keywords add the imagePerfect ones save
        //this will keep it in sync
        private static async Task WriteKeywordToImage(ImageSharp.Image image, ImageViewModel imagePerfectImage)
        {
            if (image.Metadata.IptcProfile == null)
                image.Metadata.IptcProfile = new IptcProfile();
            if (imagePerfectImage.ImageTags != "" && imagePerfectImage.ImageTags != null)
            {
                //remove all
                image.Metadata.IptcProfile.RemoveValue(IptcTag.Keywords);

                string[] tags = imagePerfectImage.ImageTags.Split(",");
                foreach (string tag in tags)
                {
                    //re-add
                    image.Metadata.IptcProfile.SetValue(IptcTag.Keywords, tag);
                }
                await image.SaveAsync($"{imagePerfectImage.ImagePath}");
            }
            //just remove all if that is what we want -- this will be the case if the user removes the entire string in the UI
            else if (imagePerfectImage.ImageTags == "" || imagePerfectImage.ImageTags == null)
            {
                //remove all
                image.Metadata.IptcProfile.RemoveValue(IptcTag.Keywords);
                await image.SaveAsync($"{imagePerfectImage.ImagePath}");
            }
        }

        private static async void WriteRatingToImage(ImageSharp.Image image, ImagePerfectImage imagePerfectImage)
        {
            if(image.Metadata.ExifProfile == null)
                image.Metadata.ExifProfile = new ExifProfile();
            ushort newRating = Convert.ToUInt16(imagePerfectImage.ImageRating);
            image.Metadata.ExifProfile.SetValue(ExifTag.Rating, newRating);
            await image.SaveAsync($"{imagePerfectImage.ImagePath}");
        }
    }
}

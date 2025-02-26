using ImagePerfect.Models;
using ImageSharp = SixLabors.ImageSharp;
using System.Collections.Generic;
using System.Linq;
using ImagePerfectImage = ImagePerfect.Models.Image;
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

        public static async void WriteTagToImage(ImagePerfectImage image)
        {
            ImageSharp.Image imageSharpImage = await ImageSharp.Image.LoadAsync(image.ImagePath);
            WriteKeywordToImage(imageSharpImage, image);
        }

        private static void UpdateMetadata(ImageSharp.ImageInfo imageInfo, ImagePerfectImage image)
        {
          
            if (imageInfo.Metadata.IptcProfile?.Values?.Any() == true)
            {
                foreach (var prop in imageInfo.Metadata.IptcProfile.Values)
                {
                    if(prop.Tag == IptcTag.Keywords)
                    {
                        if(image.ImageTags == "")
                        {
                            image.ImageTags = $"{prop.Value}";
                        }
                        else
                        {
                            image.ImageTags = $"{image.ImageTags},{prop.Value}";
                        }
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
        private static async void WriteKeywordToImage(ImageSharp.Image image, ImagePerfectImage imagePerfectImage)
        {
            if (image.Metadata.IptcProfile == null)
                image.Metadata.IptcProfile = new IptcProfile();

            if (imagePerfectImage.ImageTags != "")
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
        }
    }
}

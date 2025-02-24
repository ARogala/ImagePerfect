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

        private static void WriteMetadata(ImageSharp.Image image, Image localImg)
        {
            if (image.Metadata.IptcProfile == null)
                image.Metadata.IptcProfile = new IptcProfile();

            image.Metadata.IptcProfile.SetValue(IptcTag.Name, "Pokemon");
            image.Metadata.IptcProfile.SetValue(IptcTag.Byline, "Thimo Pedersen");
            image.Metadata.IptcProfile.SetValue(IptcTag.Caption, "Classic Pokeball Toy on a bunch of Pokemon Cards. Zapdos, Ninetales and a Trainercard visible.");
            image.Metadata.IptcProfile.SetValue(IptcTag.Source, @"https://rb.gy/hgkqhy");
            image.Metadata.IptcProfile.SetValue(IptcTag.Keywords, "Pokemon");
            image.Metadata.IptcProfile.SetValue(IptcTag.Keywords, "Pokeball");
            image.Metadata.IptcProfile.SetValue(IptcTag.Keywords, "Cards");
            image.Metadata.IptcProfile.SetValue(IptcTag.Keywords, "Zapdos");
            image.Metadata.IptcProfile.SetValue(IptcTag.Keywords, "Ninetails");
            image.Metadata.IptcProfile.SetValue(IptcTag.CustomField1, "Ninetails");
        }
    }
}

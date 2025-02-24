using ImagePerfect.Models;
using ImageSharp = SixLabors.ImageSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Image = ImagePerfect.Models.Image;
using System.Diagnostics;
using SixLabors.ImageSharp.Metadata.Profiles.Iptc;
//https://aaronbos.dev/posts/iptc-metadata-csharp-imagesharp
namespace ImagePerfect.Helpers
{
    public static class ImageMetaDataHelper
    {
        public static async void ScanImagesForMetaData(List<Image> images)
        {
            foreach (var image in images)
            {
                using (var img = await ImageSharp.Image.LoadAsync(image.ImagePath))
                {
                    ReadMetadata(img, image);
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

        private static void ReadMetadata(ImageSharp.Image image, Image localImg)
        {
          
            if (image.Metadata.IptcProfile?.Values?.Any() == true)
            {
                foreach (var prop in image.Metadata.IptcProfile.Values)
                {
                    Debug.WriteLine($"{prop.Tag}: {prop.Value} ");
           
                }
             
                Debug.WriteLine(localImg.FileName);
                Debug.WriteLine("------------------");
            }
            else
            {
                Debug.WriteLine($" does not contain metadata");
                Debug.WriteLine("------------------");
            }

            //shotwell rating is in exifprofile
            if (image.Metadata.ExifProfile?.Values?.Any() == true)
            {
                foreach (var prop in image.Metadata.ExifProfile.Values)
                    Debug.WriteLine($"{prop.Tag}: {prop.GetValue()}");
                Debug.WriteLine(localImg.FileName);
                Debug.WriteLine("------------------------------");
            }

            if (image.Metadata.IccProfile?.Entries?.Any() == true)
            {
                foreach (var prop in image.Metadata.IccProfile.Entries)
                {
                    Debug.WriteLine($"{prop.TagSignature} : {prop.Signature}");
                }
                Debug.WriteLine("------------------------------");
            }
        }
    }
}

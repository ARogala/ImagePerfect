using ImagePerfect.Models;
using ImagePerfect.ViewModels;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Metadata.Profiles.Exif;
using SixLabors.ImageSharp.Metadata.Profiles.Iptc;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ImagePerfectImage = ImagePerfect.Models.Image;
using ImageSharp = SixLabors.ImageSharp;
//https://aaronbos.dev/posts/iptc-metadata-csharp-imagesharp
namespace ImagePerfect.Helpers
{
    public static class ImageMetaDataHelper
    {
        public static async Task<List<ImagePerfectImage>> ScanImagesForMetaData(List<ImagePerfectImage> images)
        {
            await Parallel.ForEachAsync(images, new ParallelOptions { MaxDegreeOfParallelism = 4 }, async(img, ct) => {
                ImageSharp.ImageInfo imageInfo = await ImageSharp.Image.IdentifyAsync(img.ImagePath);
                UpdateMetadata(imageInfo, img);
            });
            return images;
        }

        public static async Task WriteTagToImage(ImageViewModel imageVm)
        {
            ImageSharp.Image imageSharpImage = await ImageSharp.Image.LoadAsync(imageVm.ImagePath);
            await WriteKeywordToImage(imageSharpImage, imageVm);
        }

        public static async Task AddRatingToImage(ImagePerfectImage image)
        {
            ImageSharp.Image imageSharpImage = await ImageSharp.Image.LoadAsync(image.ImagePath);
            await WriteRatingToImage(imageSharpImage, image);
        }

        public static async Task<bool> RemoveTagFromAllImages(List<ImagePerfectImage> images, Tag selectedTag)
        {
            /*
            assuming HHD with lots of data. Images with the selected tag will likely be from many different folders
            sorting by the directory and filename will group them per folder and cut down oh physical HHD head movement

            Without sorting:
            Parallel threads request files in random order, causing the HDD head to constantly jump between distant tracks. Seek times pile up.

            With sorting:
            Because files from the same directory tend to be stored physically close to each other on disk, sorting like this means:
                
                The read/write head moves sequentially within a folder's cluster.

                Only when that folder's files are done does the head jump to another folder.

                That reduces "seek time," which is what kills HDD speed.

            Consider for future commit and doing this in SQL then i can remove the C# sort
            sql: ORDER BY images.ImageFolderPath, images.FileName

            */
            List<ImagePerfectImage> sortedImages = images
               .OrderBy(img => Path.GetDirectoryName(img.ImagePath))
               .ThenBy(img => Path.GetFileName(img.ImagePath))
               .ToList();

            int anyFail = 0;
            await Parallel.ForEachAsync(sortedImages, new ParallelOptions { MaxDegreeOfParallelism = 3 }, async (img, ct) => {
                try
                {
                    using ImageSharp.Image imageSharpImage = await ImageSharp.Image.LoadAsync(img.ImagePath, ct);
                    bool success = await RemoveTag(imageSharpImage, img, selectedTag);
                    if(!success)
                        Interlocked.Exchange(ref anyFail, 1);
                }
                catch
                {
                    Interlocked.Exchange(ref anyFail, 1);
                }
            });
            return anyFail == 0;
        }

        //adds the image metadata to the ImagePerfect Image object
        private static void UpdateMetadata(ImageSharp.ImageInfo imageInfo, ImagePerfectImage image)
        {
            //thread safe bc/ each thread will make new List
            List<ImageTag> newTags = new List<ImageTag>();
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
                        newTags.Add(imageTag);
                    }
                }
            }
            image.Tags = newTags;
            //shotwell rating is in exifprofile
            if (imageInfo.Metadata.ExifProfile?.Values?.Any() == true)
            {
                if 
                (
                    imageInfo.Metadata.ExifProfile.TryGetValue(ExifTag.DateTimeOriginal, out IExifValue<string>? dateValue) &&
                    DateTime.TryParseExact(dateValue?.Value, "yyyy:MM:dd HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime parsedDate)
                )
                {
                    image.DateTaken = parsedDate;
                }
                else
                {
                    image.DateTaken = null;
                }
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

            string originalPath = imagePerfectImage.ImagePath;
            string backupPath = Path.ChangeExtension(originalPath, ".bak" + Path.GetExtension(originalPath));
            //avoid possible corruption of original images on failed writes
            try
            {
                //Create a backup
                File.Copy(originalPath, backupPath, overwrite: true);

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
                }
                //just remove all if that is what we want -- this will be the case if the user removes the entire string in the UI
                else if (imagePerfectImage.ImageTags == "" || imagePerfectImage.ImageTags == null)
                {
                    //remove all
                    image.Metadata.IptcProfile.RemoveValue(IptcTag.Keywords);
                }
                await image.SaveAsync(originalPath);
                //delete backup
                if (File.Exists(backupPath))
                    File.Delete(backupPath);
            }
            catch (Exception ex) 
            {
                //Restore backup if save failed
                if (File.Exists(backupPath))
                {
                    File.Copy(backupPath, originalPath, overwrite: true);
                    File.Delete(backupPath);
                }
            }
           
        }

        private static async Task WriteRatingToImage(ImageSharp.Image image, ImagePerfectImage imagePerfectImage)
        {
            if(image.Metadata.ExifProfile == null)
                image.Metadata.ExifProfile = new ExifProfile();
            ushort newRating = Convert.ToUInt16(imagePerfectImage.ImageRating);
            image.Metadata.ExifProfile.SetValue(ExifTag.Rating, newRating);

            string originalPath = imagePerfectImage.ImagePath;
            string backupPath = Path.ChangeExtension(originalPath, ".bak" + Path.GetExtension(originalPath));
            //avoid possible corruption of original images on failed writes
            try
            {
                // Step 1: Create a backup
                File.Copy(originalPath, backupPath, overwrite: true);

                // Step 2: Save modified image to original path
                await image.SaveAsync(originalPath);

                // Step 3: If successful, delete backup
                if (File.Exists(backupPath))
                    File.Delete(backupPath);
            }
            catch (Exception ex)
            {
                // Step 4: Restore backup if save failed
                if (File.Exists(backupPath))
                {
                    File.Copy(backupPath, originalPath, overwrite: true);
                    File.Delete(backupPath);
                }
            }

            //option 2 use temp -- keep for now
            //string originalPath = imagePerfectImage.ImagePath;
            //string directory = Path.GetDirectoryName(originalPath)!;
            //string filenameWithoutExt = Path.GetFileNameWithoutExtension(originalPath);
            //string ext = Path.GetExtension(originalPath);
            ////imagename.temp.jpg
            //string tempPath = Path.Combine(directory, $"{filenameWithoutExt}.temp{ext}");

            //// Save to temp -- avoid possible corruption of original images on failed writes
            //await image.SaveAsync(tempPath);

            //// Replace original with temp
            //File.Delete(originalPath);
            //File.Move(tempPath, originalPath);
        }

        private static async Task<bool> RemoveTag(ImageSharp.Image image, ImagePerfectImage imagePerfectImage, Tag selectedTag)
        {
            if (image.Metadata.IptcProfile == null)
                image.Metadata.IptcProfile = new IptcProfile();
            ImageSharp.Metadata.ImageMetadata imageMetadata = image.Metadata;

            string originalPath = imagePerfectImage.ImagePath;
            string backupPath = Path.ChangeExtension(originalPath, ".bak" + Path.GetExtension(originalPath));
            //avoid possible corruption of original images on failed writes
            try
            {
                //Create a backup
                File.Copy(originalPath, backupPath, overwrite: true);
                // Remove the specific tag from the image without checking
                // images passed from db will have the tag
                imageMetadata.IptcProfile.RemoveValue(IptcTag.Keywords, selectedTag.TagName);

                //to add a check see this. will slow things down if there are lots of tags
                //if (imageMetadata.IptcProfile?.Values?.Any() == true)
                //{
                //    List<IptcValue> keywordProps = imageMetadata.IptcProfile.Values
                //        .Where(v => v.Tag == IptcTag.Keywords && string.Equals(v.ToString()?.Trim(), selectedTag.TagName.Trim(), StringComparison.Ordinal))
                //        .ToList();

                //    if (keywordProps.Any()) 
                //    {
                //        //remove the specific tag from the image
                //        image.Metadata.IptcProfile.RemoveValue(IptcTag.Keywords, selectedTag.TagName);
                //    }
                //}
                await image.SaveAsync(originalPath);
                //delete backup
                if (File.Exists(backupPath))
                    File.Delete(backupPath);
                return true;
            }
            catch (Exception ex) 
            {
                //Restore backup if save failed
                if (File.Exists(backupPath))
                {
                    File.Copy(backupPath, originalPath, overwrite: true);
                    File.Delete(backupPath);
                }
                return false;
            }

        }
    }
}

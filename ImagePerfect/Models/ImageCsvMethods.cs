using CsvHelper;
using ImagePerfect.Helpers;
using ImagePerfect.Repository.IRepository;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImagePerfect.Models
{
    public class ImageCsvMethods
    {
        private static string appDirectory = Directory.GetCurrentDirectory();
        private readonly IUnitOfWork _unitOfWork;

        public ImageCsvMethods(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<bool> AddImageCsv(int imageFolderId)
        {
            string filePath = GetCsvPath("images.csv");
            return await _unitOfWork.Image.AddImageCsv(filePath, imageFolderId);
        }
        public static async Task<bool> BuildImageCsv(string imageFolderPath, int imageFolderId)
        {
            string imageCsvPath = GetCsvPath("images.csv");

            //1st empty the csv
            List<ImageCsv> records;
            using (StreamReader reader = new StreamReader(imageCsvPath))
            using (CsvReader csv = new CsvReader(reader, CultureInfo.InvariantCulture))
            {
                records = await csv.GetRecordsAsync<ImageCsv>().ToListAsync();
                records.Clear();
            }
            using (StreamWriter writer = new StreamWriter(imageCsvPath))
            using (CsvWriter csvWriter = new CsvWriter(writer, CultureInfo.InvariantCulture))
            {
                await csvWriter.WriteRecordsAsync(records);
            }

            DirectoryInfo imageFolderInfo = new DirectoryInfo(imageFolderPath);
            IEnumerable<string> imageFolderFiles = Directory.EnumerateFiles(imageFolderPath).Where(s => s.ToLower().EndsWith(".jpeg") || s.ToLower().EndsWith(".jpg") || s.ToLower().EndsWith(".png") || s.ToLower().EndsWith(".gif"));
            List<ImageCsv> images = new List<ImageCsv>();
            foreach (string imagePath in imageFolderFiles)
            {
                images.Add(
                    new ImageCsv 
                    { 
                        ImageId = 0,
                        ImagePath = PathHelper.FormatPathForDbStorage(imagePath),
                        ImageTags = null,
                        ImageRating = 0,
                        ImageFolderPath = PathHelper.FormatPathForDbStorage(imageFolderPath),
                        ImageMetaDataScanned = 0,
                        FolderId = imageFolderId,
                    }    
                );
            }
            bool hasImages = images.Any();
            if (hasImages) 
            { 
                using(StreamWriter writer = new StreamWriter(imageCsvPath))
                using(CsvWriter csv = new CsvWriter(writer, CultureInfo.InvariantCulture)) 
                { 
                    await csv.WriteRecordsAsync(images); 
                }
                return true;
            }
            else
            {
                return false;
            }
        }
        private static string GetCsvPath(string fileName)
        {
            return Directory.GetFiles(appDirectory, $"{fileName}").First();
        }
    }
}

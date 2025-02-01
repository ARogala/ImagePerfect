using CsvHelper;
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

        public static async Task<bool> BuildImageCsv(string imageFolderPath)
        {
            imageFolderPath = imageFolderPath.Replace(@"file:///", "");
            imageFolderPath = imageFolderPath.Remove(imageFolderPath.Length - 1);
            imageFolderPath = imageFolderPath.Replace(@"/", @"\");
            Debug.WriteLine(imageFolderPath);
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
            foreach (string image in imageFolderFiles)
            {
                Debug.WriteLine(image);
            }
            return true;
        }
        private static string GetCsvPath(string fileName)
        {
            return Directory.GetFiles(appDirectory, $"{fileName}").First();
        }
    }
}

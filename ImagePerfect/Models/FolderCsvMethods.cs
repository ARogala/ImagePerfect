using Avalonia.Metadata;
using Avalonia.Rendering.Composition;
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
    public class FolderCsvMethods
    {
        private static string appDirectory = Directory.GetCurrentDirectory();
        private readonly IUnitOfWork _unitOfWork;

        public FolderCsvMethods(IUnitOfWork unitOfWork) 
        {
            _unitOfWork = unitOfWork;
        }
        public async Task<bool> AddFolderCsv()
        {
            string filePath = GetCsvPath("folders.csv");
            return await _unitOfWork.Folder.AddFolderCsv(filePath);
        }

        public static async Task<bool> BuildFolderTreeCsv(string rootFolderPath)
        {
            rootFolderPath = rootFolderPath.Replace(@"file:///", "");
            rootFolderPath = rootFolderPath.Remove(rootFolderPath.Length - 1);
            rootFolderPath = rootFolderPath.Replace(@"/", @"\");
       
            string folderCsvPath = GetCsvPath("folders.csv");

            //1st empty the csv
            List<FolderCsv> records;
            using(StreamReader reader = new StreamReader(folderCsvPath))
            using(CsvReader csv = new CsvReader(reader, CultureInfo.InvariantCulture))
            {
                records = await csv.GetRecordsAsync<FolderCsv>().ToListAsync();
                
                records.Clear();
            }
            using (StreamWriter writer = new StreamWriter(folderCsvPath))
            using (CsvWriter csvWriter = new CsvWriter(writer, CultureInfo.InvariantCulture))
            {
                await csvWriter.WriteRecordsAsync(records);
            }

            //populate folder list with root folder info
            DirectoryInfo rootInfo = new DirectoryInfo(rootFolderPath);
            
            IEnumerable<string> rootFiles = Directory.EnumerateFiles(rootFolderPath).Where(s => s.ToLower().EndsWith(".jpeg") || s.ToLower().EndsWith(".jpg") || s.ToLower().EndsWith(".png") || s.ToLower().EndsWith(".gif"));
            List<FolderCsv> folders = new List<FolderCsv>
            {
                new FolderCsv
                {
                    FolderId = 0,
                    FolderName = rootInfo.Name,
                    FolderPath = PathHelper.FormatPathForDbStorage(rootFolderPath),
                    HasChildren = Directory.GetDirectories(rootFolderPath).Any() == true ? 1 : 0,
                    CoverImagePath = null,
                    FolderDescription = null,
                    FolderTags = null,
                    FolderRating = 0,
                    HasFiles = rootFiles.Any() == true ? 1 : 0,
                    IsRoot = 1,
                    FolderContentMetaDataScanned = 0,
                    AreImagesImported = 0

                }
            };
            //populate folder list with all sub directories info
            IEnumerable<string> libraryDirectories = Directory.EnumerateDirectories(rootFolderPath, "", SearchOption.AllDirectories);
            foreach (string libraryDirectory in libraryDirectories) 
            { 
                DirectoryInfo info = new DirectoryInfo(libraryDirectory);
                IEnumerable<string> files = Directory.EnumerateFiles(libraryDirectory).Where(s => s.ToLower().EndsWith(".jpeg") || s.ToLower().EndsWith(".jpg") || s.ToLower().EndsWith(".png") || s.ToLower().EndsWith(".gif"));
                folders.Add(
                    new FolderCsv
                    {
                        FolderId = 0,
                        FolderName = info.Name,
                        FolderPath = PathHelper.FormatPathForDbStorage(libraryDirectory),
                        HasChildren = Directory.GetDirectories(libraryDirectory).Any() == true ? 1 : 0,
                        CoverImagePath = null,
                        FolderDescription = null,
                        FolderTags = null,
                        FolderRating = 0,
                        HasFiles = files.Any() == true ? 1 : 0,
                        IsRoot = 0,
                        FolderContentMetaDataScanned = 0,
                        AreImagesImported = 0
                    }
                );
            }
            //check that folders list is populated
            bool hasFolders = folders.Any();
            if (hasFolders) 
            {
                //write the folders list to the csv file
                using (StreamWriter writer = new StreamWriter(folderCsvPath))
                using (CsvWriter csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
                {
                    await csv.WriteRecordsAsync(folders);
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

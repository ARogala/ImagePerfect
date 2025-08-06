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

        public static async Task<bool> AddNewFoldersCsv(List<string> newFoldersPaths, bool isRootFolder)
        {
            //if isRootFolder is true folderPaths will be Length 1
            //format the paths from folder picker to work with the Directory class
            //create an empty list to store the FolderCsv objects
            List<string> foldersPaths = newFoldersPaths.Select(x => PathHelper.FormatPathFromFolderPicker(x)).ToList();
            List<FolderCsv> folders = new List<FolderCsv>();

            //empty the csv
            string folderCsvPath = GetCsvPath("folders.csv");
            List<FolderCsv> records;
            using (StreamReader reader = new StreamReader(folderCsvPath))
            using (CsvReader csv = new CsvReader(reader, CultureInfo.InvariantCulture))
            {
                records = await csv.GetRecordsAsync<FolderCsv>().ToListAsync();

                records.Clear();
            }
            using (StreamWriter writer = new StreamWriter(folderCsvPath))
            using (CsvWriter csvWriter = new CsvWriter(writer, CultureInfo.InvariantCulture))
            {
                await csvWriter.WriteRecordsAsync(records);
            }
            //add new folders to FolderCsv list
            foreach (string folderPath in foldersPaths) 
            { 
                DirectoryInfo folderInfo = new DirectoryInfo(folderPath);
                IEnumerable<string> folderFiles = Directory.EnumerateFiles(folderPath).Where(s => s.ToLower().EndsWith(".jpeg") || s.ToLower().EndsWith(".jpg") || s.ToLower().EndsWith(".png") || s.ToLower().EndsWith(".gif"));
                folders.Add(
                        new FolderCsv 
                        { 
                            FolderId = 0,
                            FolderName = folderInfo.Name,
                            FolderPath = PathHelper.FormatPathForDbStorage(folderPath),
                            HasChildren = Directory.GetDirectories(folderPath).Any() == true ? 1 : 0,
                            CoverImagePath = null,
                            FolderDescription = null,
                            FolderRating = 0,
                            HasFiles = folderFiles.Any() == true ? 1 : 0,
                            IsRoot = isRootFolder == true ? 1 : 0,
                            FolderContentMetaDataScanned = 0,
                            AreImagesImported = 0
                        }
                    );
                //populate folder list with all sub directories info
                IEnumerable<string> folderDirectories = Directory.EnumerateDirectories(folderPath, "", SearchOption.AllDirectories);
                foreach (string folderDirectory in folderDirectories)
                {
                    DirectoryInfo info = new DirectoryInfo(folderDirectory);
                    IEnumerable<string> files = Directory.EnumerateFiles(folderDirectory).Where(s => s.ToLower().EndsWith(".jpeg") || s.ToLower().EndsWith(".jpg") || s.ToLower().EndsWith(".png") || s.ToLower().EndsWith(".gif"));
                    folders.Add(
                        new FolderCsv
                        {
                            FolderId = 0,
                            FolderName = info.Name,
                            FolderPath = PathHelper.FormatPathForDbStorage(folderDirectory),
                            HasChildren = Directory.GetDirectories(folderDirectory).Any() == true ? 1 : 0,
                            CoverImagePath = null,
                            FolderDescription = null,
                            FolderRating = 0,
                            HasFiles = files.Any() == true ? 1 : 0,
                            IsRoot = 0,
                            FolderContentMetaDataScanned = 0,
                            AreImagesImported = 0
                        }
                    );
                }
            }
            //check that folders list is populated
            bool hasFolders = folders.Any();
            if (hasFolders)
            {
                /*
                On Ubuntu/Linux folders imported with the same name but different case will show all folders within each folder in both folders
                Easiest fix it simply -- Don't allow this.

                Find folder path duplicates differing only by case
                key will be folder path and list will be list of folders with the same case insensitive path
                */
                Dictionary<string, List<FolderCsv>> pathConflicts = folders
                    .GroupBy(f => f.FolderPath.ToLowerInvariant())
                    .Where(g => g.Count() > 1)
                    .ToDictionary(g => g.Key, g => g.ToList());

                // Remove all conflicting folders from the list
                if (pathConflicts.Any())
                {
                    
                    foreach (var conflictGroup in pathConflicts.Values)
                    {
                        foreach (var folder in conflictGroup)
                        {
                            folders.Remove(folder); // Remove from import list
                        }
                    }

                    //write a log file
                    string logPath = GetCsvPath("case_conflict_folders.txt");
                    List<string> logLines = new List<string>
                    {
                        $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] Case conflict detected:"
                    };
                    logLines.AddRange(pathConflicts
                        .SelectMany(g => g.Value)
                        .Select(f => $" - {f.FolderPath}"));

                    logLines.Add(""); // add a newline between entries

                    await File.AppendAllLinesAsync(logPath, logLines);
                    
                }

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

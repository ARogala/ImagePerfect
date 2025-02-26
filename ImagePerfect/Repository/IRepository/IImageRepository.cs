﻿using ImagePerfect.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ImagePerfect.Repository.IRepository
{
    public interface IImageRepository : IRepository<Image>
    {
        //any Image model sepecific database methods here
        Task<List<Image>> GetAllImagesInFolder(int folderId);
        Task<List<Image>> GetAllImagesInFolder(string folderPath);
        Task<List<Image>> GetAllImagesInDirectoryTree(string directoryPath);
        Task<bool> AddImageCsv(string filePath, int folderId);
        Task<bool> UpdateImageTags(Image image, string newTag);
        Task<List<string>> GetTagsList();
        Task<bool> UpdateImageMetaData(string imageUpdateSql, int folderId);
    }
}

using Dapper;
using ImagePerfect.Helpers;
using ImagePerfect.Models;
using ImagePerfect.Repository.IRepository;
using MySqlConnector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImagePerfect.Repository
{
    public class ImageRepository : Repository<Image>, IImageRepository
    {
        private readonly MySqlConnection _connection;

        public ImageRepository(MySqlConnection db) : base(db) 
        { 
            _connection = db;
        }
        //any Image model specific database methods here
        public async Task<List<Image>> GetAllImagesInFolder(int folderId)
        {
            string sql = @"SELECT * FROM images WHERE FolderId = @folderId ORDER BY FileName";
            List<Image> allImagesInFolder = (List<Image>)await _connection.QueryAsync<Image>(sql, new { folderId });
            await _connection.CloseAsync();
            return allImagesInFolder;
        }

        public async Task<List<Image>> GetAllImagesInFolder(string folderPath)
        {
            string sql = @"SELECT * FROM images WHERE ImageFolderPath = @folderPath ORDER BY FileName";
            List<Image> allImagesInFolder = (List<Image>)await _connection.QueryAsync<Image>(sql, new { folderPath });
            await _connection.CloseAsync();
            return allImagesInFolder;
        }

        public async Task<List<Image>> GetAllImagesInDirectoryTree(string directoryPath)
        {
            string regExpString = PathHelper.GetRegExpStringDirectoryTree(directoryPath);
            string sql = @"SELECT * FROM images WHERE REGEXP_LIKE(ImageFolderPath, '" + regExpString + "') ORDER BY FileName;";
            List<Image> images = (List<Image>)await _connection.QueryAsync<Image>(sql);
            await _connection.CloseAsync();
            return images;
        }

        public async Task<bool> AddImageCsv(string filePath, int folderId)
        {
            int rowsEffectedA = 0;
            int rowsEffectedB = 0;
            await _connection.OpenAsync();
            using (MySqlTransaction txn = await _connection.BeginTransactionAsync())
            {
                MySqlBulkLoader bulkLoader = new MySqlBulkLoader(_connection)
                {
                    FileName = filePath,
                    TableName = "images",
                    CharacterSet = "UTF8",
                    NumberOfLinesToSkip = 1,
                    FieldTerminator = ",",
                    FieldQuotationCharacter = '"',
                    FieldQuotationOptional = true,
                    Local = true,
                };
                rowsEffectedA = await bulkLoader.LoadAsync();
                string sql = @"UPDATE folders SET AreImagesImported = true WHERE FolderId = @folderId";
                rowsEffectedB = await _connection.ExecuteAsync(sql, new { folderId }, transaction: txn);
                if (rowsEffectedA > 0 && rowsEffectedB > 0)
                {
                    await txn.CommitAsync();
                    await _connection.CloseAsync();
                    return true;
                }
                await txn.RollbackAsync();
                await _connection.CloseAsync();
                return false;
            }
        }

        public async Task<bool> UpdateImageTags(Image image, string newTag)
        {
            int rowsEffectedA = 0;
            int rowsEffectedB = 0;
            await _connection.OpenAsync();
            MySqlTransaction txn = await _connection.BeginTransactionAsync();
            string sql1 = @"UPDATE images Set ImageTags = @tags WHERE ImageId = @id";
            string sql2 = @"INSERT IGNORE INTO tags (TagName) VALUES (@newTag)";
            rowsEffectedA = await _connection.ExecuteAsync(sql1, new { tags = image.ImageTags, id = image.ImageId }, transaction: txn);
            rowsEffectedB = await _connection.ExecuteAsync(sql2, new { newTag = newTag }, transaction: txn);
            await txn.CommitAsync();
            await _connection.CloseAsync();
            return rowsEffectedA + rowsEffectedB >= 1 ? true : false;

        }

        public async Task<List<string>> GetTagsList()
        {
            string sql = @"SELECT TagName FROM tags";
            List<string> tags = (List<string>)await _connection.QueryAsync<string>(sql);
            await _connection.CloseAsync();
            return tags;
        }
    }
}

using Dapper;
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
            string sql = @"SELECT * FROM images WHERE FolderId = @folderId";
            return (List<Image>)await _connection.QueryAsync<Image>(sql, new { folderId });
        }

        public async Task<bool> AddImageCsv(string filePath)
        {
            int rowsEffected = 0;
            var bulkLoader = new MySqlBulkLoader(_connection) 
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
            rowsEffected = await bulkLoader.LoadAsync();
            await _connection.CloseAsync();
            return rowsEffected > 0 ? true : false;
        }
    }
}

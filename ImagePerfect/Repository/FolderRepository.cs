using MySqlConnector;
using ImagePerfect.Models;
using ImagePerfect.Repository.IRepository;
using System.Threading.Tasks;
using Dapper;
using System.Collections.Generic;
using ImagePerfect.Helpers;

namespace ImagePerfect.Repository
{
    public class FolderRepository : Repository<Folder>, IFolderRepository
    {
        private readonly MySqlConnection _connection;

        public FolderRepository(MySqlConnection db) : base(db) 
        {
            _connection = db;
        }
        //any Folder model specific database methods here
        public async Task<Folder?> GetRootFolder()
        {
            string sql = @"SELECT * FROM folders WHERE IsRoot = True";
            Folder? rootFolder = await _connection.QuerySingleOrDefaultAsync<Folder>(sql);
            await _connection.CloseAsync();
            return rootFolder;
        }
        public async Task<bool> AddFolderCsv(string filePath)
        {
            int rowsEffected = 0;
            var bulkLoader = new MySqlBulkLoader(_connection)
            {
                FileName = filePath,
                TableName = "folders",
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

        //Only gets the folders in the path -- the folder itself or any sub directories within each folder are not returned
        public async Task<List<Folder>> GetFoldersInDirectory(string directoryPath)
        {
            string regExpString = PathHelper.GetRegExpStringAllFoldersInDirectory(directoryPath);
            string sql = @"SELECT * FROM folders WHERE REGEXP_LIKE(FolderPath, '" + regExpString + "');";
            List<Folder> folders = (List<Folder>)await _connection.QueryAsync<Folder>(sql);
            await _connection.CloseAsync();
            return folders;
        }

        //gets the folder itself as well as all folders and subfolders within. 
        //the entire directory tree of the path
        public async Task<List<Folder>> GetDirectoryTree(string directoryPath)
        {
            string regExpString = PathHelper.GetRegExpStringDirectoryTree(directoryPath);
            string sql = @"SELECT * FROM folders WHERE REGEXP_LIKE(FolderPath, '" + regExpString + "');";
            List<Folder> folders = (List<Folder>)await _connection.QueryAsync<Folder>(sql);
            await _connection.CloseAsync();
            return folders;
        }

        public async Task<bool> AddCoverImage(string coverImagePath, int folderId)
        {
            int rowsEffected = 0;
            string sql = @"UPDATE folders SET CoverImagePath = @coverImagePath WHERE FolderId = @folderId";
            rowsEffected = await _connection.ExecuteAsync(sql, new { coverImagePath, folderId });
            await _connection.CloseAsync();
            return rowsEffected > 0 ? true : false;
        }

        public async Task<bool> MoveFolder(string folderMoveSql, string imageMoveSql)
        {
            int rowsEffectedA = 0;
            int rowsEffectedB = 0;  
            await _connection.OpenAsync();
            MySqlTransaction txn = await _connection.BeginTransactionAsync();
            rowsEffectedA = await _connection.ExecuteAsync(folderMoveSql, transaction: txn);
            rowsEffectedB = await _connection.ExecuteAsync(imageMoveSql, transaction: txn);
            await txn.CommitAsync();
            await _connection.CloseAsync();
            return rowsEffectedA > 0 && rowsEffectedB >= 0 ? true : false;
        }
    }
}

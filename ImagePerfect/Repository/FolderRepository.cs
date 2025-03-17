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
            await _connection.OpenAsync();
            MySqlTransaction txn = await _connection.BeginTransactionAsync();
            string sql1 = @"SELECT * FROM folders WHERE IsRoot = 1";
            Folder? rootFolder = await _connection.QuerySingleOrDefaultAsync<Folder>(sql1, transaction: txn);
            string sql2 = @"SELECT tags.TagId, tags.TagName, folders.FolderId FROM folders
	                            JOIN folder_tags_join ON folder_tags_join.FolderId = folders.FolderId
	                            JOIN tags ON folder_tags_join.TagId = tags.TagId WHERE folders.IsRoot = 1;";
            if (rootFolder != null) 
            { 
                List<FolderTag> tags = (List<FolderTag>)await _connection.QueryAsync<FolderTag>(sql2, transaction: txn);
                rootFolder.Tags = tags;
            }
            await txn.CommitAsync();
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
        public async Task<(List<Folder> folders, List<FolderTag> tags)> GetFoldersInDirectory(string directoryPath)
        {
            await _connection.OpenAsync();
            MySqlTransaction txn = await _connection.BeginTransactionAsync();
            string regExpString = PathHelper.GetRegExpStringAllFoldersInDirectory(directoryPath);
            string sql1 = @"SELECT * FROM folders WHERE REGEXP_LIKE(FolderPath, '" + regExpString + "') ORDER BY FolderName;";
            List<Folder> folders = (List<Folder>)await _connection.QueryAsync<Folder>(sql1, transaction: txn);
            string sql2 = @"SELECT tags.TagId, tags.TagName, folders.FolderId FROM folders 
                            JOIN folder_tags_join ON folder_tags_join.FolderId = folders.FolderId 
                            JOIN tags ON folder_tags_join.TagId = tags.TagId WHERE REGEXP_LIKE(folders.FolderPath, '" + regExpString + "') ORDER BY folders.FolderId;";
            List<FolderTag> tags = (List<FolderTag>)await _connection.QueryAsync<FolderTag>(sql2, transaction: txn);
            await txn.CommitAsync();
            await _connection.CloseAsync();
            return (folders,tags);
        }

        public async Task<(List<Folder> folders, List<FolderTag> tags)> GetAllFoldersAtRating(int rating)
        {
            await _connection.OpenAsync();
            MySqlTransaction txn = await _connection.BeginTransactionAsync();

            string sql1 = @"SELECT * FROM folders WHERE FolderRating = @rating ORDER BY FolderName";
            List<Folder> allFoldersAtRating = (List<Folder>)await _connection.QueryAsync<Folder>(sql1, new { rating }, transaction: txn);
            string sql2 = @"SELECT tags.TagId, tags.TagName, folders.FolderId FROM folders 
                            JOIN folder_tags_join ON folder_tags_join.FolderId = folders.FolderId 
                            JOIN tags ON folder_tags_join.TagId = tags.TagId WHERE folders.FolderRating = @rating ORDER BY folders.FolderName;";
            List<FolderTag> tags = (List<FolderTag>)await _connection.QueryAsync<FolderTag>(sql2, new { rating }, transaction: txn);
            await txn.CommitAsync();
            await _connection.CloseAsync();
            return (allFoldersAtRating,tags);
        }

        //gets the folder itself as well as all folders and subfolders within. 
        //the entire directory tree of the path
        public async Task<List<Folder>> GetDirectoryTree(string directoryPath)
        {
            string regExpString = PathHelper.GetRegExpStringDirectoryTree(directoryPath);
            string sql = @"SELECT * FROM folders WHERE REGEXP_LIKE(FolderPath, '" + regExpString + "') ORDER BY FolderName;";
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

        public async Task<bool> UpdateFolderTags(Folder folder, string newTag)
        {
            int rowsEffectedA = 0;
            int rowsEffectedB = 0;
            await _connection.OpenAsync();
            MySqlTransaction txn = await _connection.BeginTransactionAsync();
            //insert newTag if its already there IGNORE
            string sql1 = @"INSERT IGNORE INTO tags (TagName) VALUES (@newTag)";
            rowsEffectedA = await _connection.ExecuteAsync(sql1, new { newTag = newTag }, transaction: txn);
            //get newTag id
            string sql2 = @"SELECT TagId FROM tags WHERE TagName = @newTag";
            int newTagId = await _connection.QuerySingleOrDefaultAsync<int>(sql2, new { newTag }, transaction: txn);
            //insert into folder_tags_join
            string sql3 = @"INSERT INTO folder_tags_join (FolderId, TagId) VALUES (@folderId, @tagId)";
            rowsEffectedB = await _connection.ExecuteAsync(sql3, new { folderId = folder.FolderId, tagId = newTagId }, transaction: txn);

            await txn.CommitAsync();
            await _connection.CloseAsync();
            return rowsEffectedA + rowsEffectedB >= 1 ? true : false;
        }
        public async Task<bool> DeleteFolderTag(FolderTag tag)
        {
            int rowsEffected = 0;
            string sql = @"DELETE FROM folder_tags_join WHERE FolderId = @folderId AND TagId = @tagId";
            rowsEffected = await _connection.ExecuteAsync(sql, new { folderId = tag.FolderId, tagId = tag.TagId });
            await _connection.CloseAsync();
            return rowsEffected > 0 ? true : false;
        }
    }
}

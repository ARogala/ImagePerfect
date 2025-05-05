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

        //get the folder at the path -- only one folder returned
        public async Task<Folder> GetFolderAtDirectory(string directoryPath)
        {
            //fomats the path correctly for this case as well. Maybe new method name.
            string path = PathHelper.GetRegExpStringDirectoryTree(directoryPath);
            string sql = @"SELECT * FROM folders WHERE FolderPath LIKE '" + path + "'";
            Folder folder = (Folder)await _connection.QuerySingleAsync<Folder>(sql);
            return folder;
        }

        public async Task<(List<Folder> folders, List<FolderTag> tags)> GetAllFoldersAtRating(int rating, bool filterInCurrentDirectory, string currentDirectory)
        {
            await _connection.OpenAsync();
            MySqlTransaction txn = await _connection.BeginTransactionAsync();
            string path = PathHelper.FormatPathForLikeOperator(currentDirectory);
            string sql1 = string.Empty;
            string sql2 = string.Empty;
            if (filterInCurrentDirectory)
            {
                sql1 = @"SELECT * FROM folders WHERE FolderRating = @rating AND FolderPath LIKE '" + path + "' ORDER BY FolderName";
                sql2 = @"SELECT tags.TagId, tags.TagName, folders.FolderId FROM folders 
                            JOIN folder_tags_join ON folder_tags_join.FolderId = folders.FolderId 
                            JOIN tags ON folder_tags_join.TagId = tags.TagId WHERE folders.FolderRating = @rating AND FolderPath LIKE '" + path + "' ORDER BY folders.FolderName;";
            }
            else
            {
                sql1 = @"SELECT * FROM folders WHERE FolderRating = @rating ORDER BY FolderName";
                sql2 = @"SELECT tags.TagId, tags.TagName, folders.FolderId FROM folders 
                            JOIN folder_tags_join ON folder_tags_join.FolderId = folders.FolderId 
                            JOIN tags ON folder_tags_join.TagId = tags.TagId WHERE folders.FolderRating = @rating ORDER BY folders.FolderName;";
            }
            
            List<Folder> allFoldersAtRating = (List<Folder>)await _connection.QueryAsync<Folder>(sql1, new { rating }, transaction: txn);
            List<FolderTag> tags = (List<FolderTag>)await _connection.QueryAsync<FolderTag>(sql2, new { rating }, transaction: txn);
            await txn.CommitAsync();
            await _connection.CloseAsync();
            return (allFoldersAtRating,tags);
        }

        public async Task<(List<Folder> folders, List<FolderTag> tags)> GetAllFoldersWithNoImportedImages(bool filterInCurrentDirectory, string currentDirectory)
        {
            await _connection.OpenAsync();
            MySqlTransaction txn = await _connection.BeginTransactionAsync();
            string path = PathHelper.FormatPathForLikeOperator(currentDirectory);
            string sql1 = string.Empty;
            string sql2 = string.Empty;
            if (filterInCurrentDirectory) 
            {
                sql1 = @"SELECT * FROM folders WHERE AreImagesImported = false AND HasFiles = true AND FolderPath LIKE '" + path + "' ORDER BY FolderName";
                sql2 = @"SELECT tags.TagId, tags.TagName, folders.FolderId FROM folders 
                            JOIN folder_tags_join ON folder_tags_join.FolderId = folders.FolderId 
                            JOIN tags ON folder_tags_join.TagId = tags.TagId WHERE folders.AreImagesImported = false AND FolderPath LIKE '" + path + "' ORDER BY folders.FolderName;";
            }
            else
            {
                sql1 = @"SELECT * FROM folders WHERE AreImagesImported = false AND HasFiles = true ORDER BY FolderName";
                sql2 = @"SELECT tags.TagId, tags.TagName, folders.FolderId FROM folders 
                            JOIN folder_tags_join ON folder_tags_join.FolderId = folders.FolderId 
                            JOIN tags ON folder_tags_join.TagId = tags.TagId WHERE folders.AreImagesImported = false ORDER BY folders.FolderName;";
            }

            List<Folder> allFoldersWithNoImportedImages = (List<Folder>)await _connection.QueryAsync<Folder>(sql1, transaction: txn);
            List<FolderTag> tags = (List<FolderTag>)await _connection.QueryAsync<FolderTag>(sql2, transaction: txn);
            await txn.CommitAsync();
            await _connection.CloseAsync();
            return (allFoldersWithNoImportedImages, tags);
        }

        public async Task<(List<Folder> folders, List<FolderTag> tags)> GetAllFoldersWithMetadataNotScanned(bool filterInCurrentDirectory, string currentDirectory)
        {
            await _connection.OpenAsync();
            MySqlTransaction txn = await _connection.BeginTransactionAsync();
            string path = PathHelper.FormatPathForLikeOperator(currentDirectory);
            string sql1 = string.Empty;
            string sql2 = string.Empty;
            if (filterInCurrentDirectory)
            {
                sql1 = @"SELECT * FROM folders WHERE FolderContentMetaDataScanned = false AND HasFiles = true AND FolderPath LIKE '" + path + "' ORDER BY FolderName";
                sql2 = @"SELECT tags.TagId, tags.TagName, folders.FolderId FROM folders 
                            JOIN folder_tags_join ON folder_tags_join.FolderId = folders.FolderId 
                            JOIN tags ON folder_tags_join.TagId = tags.TagId WHERE folders.FolderContentMetaDataScanned = false AND FolderPath LIKE '" + path + "' ORDER BY folders.FolderName;";
            }
            else
            {
                sql1 = @"SELECT * FROM folders WHERE FolderContentMetaDataScanned = false AND HasFiles = true ORDER BY FolderName";
                sql2 = @"SELECT tags.TagId, tags.TagName, folders.FolderId FROM folders 
                            JOIN folder_tags_join ON folder_tags_join.FolderId = folders.FolderId 
                            JOIN tags ON folder_tags_join.TagId = tags.TagId WHERE folders.FolderContentMetaDataScanned = false ORDER BY folders.FolderName;";
            }

            List<Folder> allFoldersWithMetadataNotScanned = (List<Folder>)await _connection.QueryAsync<Folder>(sql1, transaction: txn);
            List<FolderTag> tags = (List<FolderTag>)await _connection.QueryAsync<FolderTag>(sql2, transaction: txn);
            await txn.CommitAsync();
            await _connection.CloseAsync();
            return (allFoldersWithMetadataNotScanned, tags);
        }

        public async Task<(List<Folder> folders, List<FolderTag> tags)> GetAllFoldersWithoutCovers(bool filterInCurrentDirectory, string currentDirectory)
        {
            await _connection.OpenAsync();
            MySqlTransaction txn = await _connection.BeginTransactionAsync();
            string path = PathHelper.FormatPathForLikeOperator(currentDirectory);
            string sql1 = string.Empty;
            string sql2 = string.Empty;
            if (filterInCurrentDirectory)
            {
                sql1 = @"SELECT * FROM folders WHERE AreImagesImported = true AND HasFiles = true AND CoverImagePath = '' AND FolderPath LIKE '" + path + "' ORDER BY FolderName";
                sql2 = @"SELECT tags.TagId, tags.TagName, folders.FolderId FROM folders 
                            JOIN folder_tags_join ON folder_tags_join.FolderId = folders.FolderId 
                            JOIN tags ON folder_tags_join.TagId = tags.TagId WHERE folders.AreImagesImported = true AND folders.CoverImagePath = '' AND FolderPath LIKE '" + path + "' ORDER BY folders.FolderName;";
            }
            else
            {
                sql1 = @"SELECT * FROM folders WHERE AreImagesImported = true AND HasFiles = true AND CoverImagePath = '' ORDER BY FolderName";
                sql2 = @"SELECT tags.TagId, tags.TagName, folders.FolderId FROM folders 
                            JOIN folder_tags_join ON folder_tags_join.FolderId = folders.FolderId 
                            JOIN tags ON folder_tags_join.TagId = tags.TagId WHERE folders.AreImagesImported = true AND folders.CoverImagePath = '' ORDER BY folders.FolderName;";
            }

            List<Folder> allFoldersWithoutCovers = (List<Folder>)await _connection.QueryAsync<Folder>(sql1, transaction: txn);
            List<FolderTag> tags = (List<FolderTag>)await _connection.QueryAsync<FolderTag>(sql2, transaction: txn);
            await txn.CommitAsync();
            await _connection.CloseAsync();
            return (allFoldersWithoutCovers, tags);
        }
        public async Task<(List<Folder> folders, List<FolderTag> tags)> GetAllFoldersWithTag(string tag, bool filterInCurrentDirectory, string currentDirectory)
        {
            await _connection.OpenAsync();
            MySqlTransaction txn = await _connection.BeginTransactionAsync();
            string path = PathHelper.FormatPathForLikeOperator(currentDirectory);
            string sql1 = string.Empty;
            string sql2 = string.Empty;
            if (filterInCurrentDirectory) 
            {
                sql1 = @"SELECT * FROM folders
                            JOIN folder_tags_join ON folder_tags_join.FolderId = folders.FolderId
                            JOIN tags ON folder_tags_join.TagId = tags.TagId WHERE tags.TagName = @tag AND FolderPath LIKE '" + path + "' ORDER BY folders.FolderName;";
                sql2 = @"SELECT tags.TagId, tags.TagName, folders.FolderId FROM folders
                            JOIN folder_tags_join ON folder_tags_join.FolderId = folders.FolderId
                            JOIN tags ON folder_tags_join.TagId = tags.TagId WHERE FolderPath LIKE '" + path + "' ORDER BY folders.FolderName;";
            }
            else
            {
                sql1 = @"SELECT * FROM folders
                            JOIN folder_tags_join ON folder_tags_join.FolderId = folders.FolderId
                            JOIN tags ON folder_tags_join.TagId = tags.TagId WHERE tags.TagName = @tag ORDER BY folders.FolderName;";
                sql2 = @"SELECT tags.TagId, tags.TagName, folders.FolderId FROM folders
                            JOIN folder_tags_join ON folder_tags_join.FolderId = folders.FolderId
                            JOIN tags ON folder_tags_join.TagId = tags.TagId ORDER BY folders.FolderName;";
            }
            
            List<Folder> allFoldersWithTag = (List<Folder>)await _connection.QueryAsync<Folder>(sql1, new { tag }, transaction: txn);
            List<FolderTag> tags = (List<FolderTag>)await _connection.QueryAsync<FolderTag>(sql2, new { tag }, transaction: txn);
            await txn.CommitAsync();
            await _connection.CloseAsync();
            return (allFoldersWithTag, tags);
        }

        public async Task<(List<Folder> folders, List<FolderTag> tags)> GetAllFoldersWithDescriptionText(string text, bool filterInCurrentDirectory, string currentDirectory)
        {
            await _connection.OpenAsync();
            MySqlTransaction txn = await _connection.BeginTransactionAsync();
            string path = PathHelper.FormatPathForLikeOperator(currentDirectory);
            string sql1 = string.Empty;
            string sql2 = string.Empty;
            if (filterInCurrentDirectory)
            {
                sql1 = @"SELECT * FROM folders WHERE MATCH(FolderName, FolderPath, FolderDescription) AGAINST(@text) AND FolderPath LIKE '" + path + "' ORDER BY FolderName";
                sql2 = @"SELECT tags.TagId, tags.TagName, folders.FolderId FROM folders
                            JOIN folder_tags_join ON folder_tags_join.FolderId = folders.FolderId
                            JOIN tags ON folder_tags_join.TagId = tags.TagId WHERE FolderPath LIKE '" + path + "' ORDER BY folders.FolderName;";
            }
            else 
            {
                sql1 = @"SELECT * FROM folders WHERE MATCH(FolderName, FolderPath, FolderDescription) AGAINST(@text) ORDER BY FolderName";
                sql2 = @"SELECT tags.TagId, tags.TagName, folders.FolderId FROM folders
                            JOIN folder_tags_join ON folder_tags_join.FolderId = folders.FolderId
                            JOIN tags ON folder_tags_join.TagId = tags.TagId ORDER BY folders.FolderName;";
            }
            
            List<Folder> allFoldersWithDescriptionText = (List<Folder>)await _connection.QueryAsync<Folder>(sql1, new { text }, transaction: txn);
            List<FolderTag> tags = (List<FolderTag>)await _connection.QueryAsync<FolderTag>(sql2, transaction: txn);
            await txn.CommitAsync();
            await _connection.CloseAsync();
            return (allFoldersWithDescriptionText, tags);
        }

        public async Task<(List<Folder> folders, List<FolderTag> tags)> GetAllFavoriteFolders()
        {
            await _connection.OpenAsync();
            MySqlTransaction txn = await _connection.BeginTransactionAsync();
            string sql1 = @"SELECT * FROM folders WHERE FolderId IN (SELECT FolderId FROM folder_saved_favorites) ORDER BY FolderName";
            string sql2 = @"SELECT tags.TagId, tags.TagName, folders.FolderId FROM folders
                                JOIN folder_tags_join ON folder_tags_join.FolderId = folders.FolderId
                                JOIN tags ON folder_tags_join.TagId = tags.TagId WHERE folders.FolderId IN (SELECT folder_saved_favorites.FolderId FROM folder_saved_favorites) ORDER BY FolderName";
            List<Folder> allFavoriteFolders = (List<Folder>)await _connection.QueryAsync<Folder>(sql1, transaction: txn);
            List<FolderTag> tags = (List<FolderTag>)await _connection.QueryAsync<FolderTag>(sql2, transaction: txn);
            await txn.CommitAsync();
            await _connection.CloseAsync();
            return (allFavoriteFolders, tags);
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
            //insert into folder_tags_join if its already there IGNORE
            string sql3 = @"INSERT IGNORE INTO folder_tags_join (FolderId, TagId) VALUES (@folderId, @tagId)";
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

        public async Task<bool> DeleteLibrary()
        {
            int rowsEffected = 0;
            await _connection.OpenAsync();
            MySqlTransaction txn = await _connection.BeginTransactionAsync();
            string sql1 = @"DELETE FROM folders WHERE FolderId >= 1";
            string sql2 = @"ALTER TABLE folders AUTO_INCREMENT = 1";
            string sql3 = @"ALTER TABLE images AUTO_INCREMENT = 1";
            string sql4 = @"DELETE FROM tags WHERE TagId >= 1";
            string sql5 = @"ALTER TABLE tags AUTO_INCREMENT = 1";

            rowsEffected = await _connection.ExecuteAsync(sql1, transaction: txn);
            await _connection.ExecuteAsync(sql2, transaction: txn);
            await _connection.ExecuteAsync(sql3, transaction: txn);
            await _connection.ExecuteAsync(sql4, transaction: txn);
            await _connection.ExecuteAsync(sql5, transaction: txn);

            await txn.CommitAsync();
            await _connection.CloseAsync();
            return rowsEffected > 0 ? true : false;
        }

        public async Task SaveFolderToFavorites(int folderId)
        {
            string sql = @"INSERT IGNORE INTO folder_saved_favorites (FolderId) VALUES (@folderId)";
            await _connection.ExecuteAsync(sql, new { folderId });
        }

        public async Task DeleteAllFavoriteFolders()
        {
            string sql = @"DELETE FROM folder_saved_favorites";
            await _connection.ExecuteAsync(sql);
        }
    }
}

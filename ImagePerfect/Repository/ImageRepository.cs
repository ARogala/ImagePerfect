using Dapper;
using ImagePerfect.Helpers;
using ImagePerfect.Models;
using ImagePerfect.Repository.IRepository;
using ImagePerfect.ViewModels;
using Microsoft.Extensions.Configuration;
using MySqlConnector;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImagePerfect.Repository
{
    public class ImageRepository : Repository<Image>, IImageRepository
    {
        private readonly MySqlConnection _connection;
        private readonly string _connectionString;

        public ImageRepository(MySqlConnection db, IConfiguration config) : base(db) 
        { 
            _connection = db;
            _connectionString = config.GetConnectionString("DefaultConnection");
        }
        //any Image model specific database methods here
        public async Task<(List<Image> images, List<ImageTag> tags)> GetAllImagesInFolder(int folderId)
        {
            await using MySqlConnection conn = new MySqlConnection(_connectionString); //need a new connection pre folder as this is a parallel method
            await conn.OpenAsync();
            await using MySqlTransaction txn = await conn.BeginTransactionAsync();
            string sql1 = @"SELECT * FROM images WHERE FolderId = @folderId ORDER BY FileName";
            List<Image> allImagesInFolder = (List<Image>)await conn.QueryAsync<Image>(sql1, new { folderId }, transaction: txn);
            string sql2 = @"SELECT tags.TagId, tags.TagName, images.ImageId FROM images
                            JOIN image_tags_join ON image_tags_join.ImageId = images.ImageId
                            JOIN tags ON image_tags_join.TagId = tags.TagId WHERE images.FolderId = @folderId ORDER BY images.FileName;";
            List<ImageTag> tags = (List<ImageTag>)await conn.QueryAsync<ImageTag>(sql2, new { folderId }, transaction: txn);
            await txn.CommitAsync();
            await conn.CloseAsync();
            return (allImagesInFolder, tags);
        }

        public async Task<(List<Image> images, List<ImageTag> tags)> GetAllImagesInFolder(string folderPath)
        {
            await _connection.OpenAsync();
            MySqlTransaction txn = await _connection.BeginTransactionAsync();

            string sql1 = @"SELECT * FROM images WHERE ImageFolderPath = @folderPath ORDER BY FileName";
            List<Image> allImagesInFolder = (List<Image>)await _connection.QueryAsync<Image>(sql1, new { folderPath }, transaction: txn);
            string sql2 = @"SELECT tags.TagId, tags.TagName, images.ImageId FROM images
                            JOIN image_tags_join ON image_tags_join.ImageId = images.ImageId
                            JOIN tags ON image_tags_join.TagId = tags.TagId WHERE images.ImageFolderPath = @folderPath ORDER BY images.FileName;";
            List<ImageTag> tags = (List<ImageTag>)await _connection.QueryAsync<ImageTag>(sql2, new { folderPath }, transaction: txn);
            await txn.CommitAsync();
            await _connection.CloseAsync();
            return (allImagesInFolder, tags);
        }

        public async Task<(List<Image> images, List<ImageTag> tags)> GetAllImagesAtRating(int rating, bool filterInCurrentDirectory, string currentDirectory)
        {
            await _connection.OpenAsync();
            MySqlTransaction txn = await _connection.BeginTransactionAsync();
            string path = PathHelper.FormatPathForLikeOperator(currentDirectory);
            string sql1 = string.Empty;
            string sql2 = string.Empty;
            if (filterInCurrentDirectory) 
            {
                sql1 = @"SELECT * FROM images WHERE ImageRating = @rating AND ImageFolderPath LIKE '" + path + "' ORDER BY ImageFolderPath, FileName";
                sql2 = @"SELECT tags.TagId, tags.TagName, images.ImageId FROM images
                            JOIN image_tags_join ON image_tags_join.ImageId = images.ImageId
                            JOIN tags ON image_tags_join.TagId = tags.TagId WHERE images.ImageRating = @rating AND ImageFolderPath LIKE '" + path + "' ORDER BY images.ImageFolderPath, images.FileName;";
            }
            else
            {
                sql1 = @"SELECT * FROM images WHERE ImageRating = @rating ORDER BY ImageFolderPath, FileName";
                sql2 = @"SELECT tags.TagId, tags.TagName, images.ImageId FROM images
                            JOIN image_tags_join ON image_tags_join.ImageId = images.ImageId
                            JOIN tags ON image_tags_join.TagId = tags.TagId WHERE images.ImageRating = @rating ORDER BY images.ImageFolderPath, images.FileName;";
            }
            List<Image> allImagesAtRating = (List<Image>)await _connection.QueryAsync<Image>(sql1, new { rating }, transaction: txn);           
            List<ImageTag> tags = (List<ImageTag>)await _connection.QueryAsync<ImageTag>(sql2, new { rating }, transaction: txn);
            await txn.CommitAsync();
            await _connection.CloseAsync();
            return (allImagesAtRating, tags);
        }

        public async Task<(List<Image> images, List<ImageTag> tags)> GetAllImagesAtYear(int year, bool filterInCurrentDirectory, string currentDirectory)
        {
            await _connection.OpenAsync();
            MySqlTransaction txn = await _connection.BeginTransactionAsync();
            string path = PathHelper.FormatPathForLikeOperator(currentDirectory);
            string sql1 = string.Empty;
            string sql2 = string.Empty;
            if (filterInCurrentDirectory)
            {
                sql1 = @"SELECT * FROM images WHERE DateTakenYear = @year AND ImageFolderPath LIKE '" + path + "' ORDER BY ImageFolderPath, FileName";
                sql2 = @"SELECT tags.TagId, tags.TagName, images.ImageId FROM images
                            JOIN image_tags_join ON image_tags_join.ImageId = images.ImageId
                            JOIN tags ON image_tags_join.TagId = tags.TagId WHERE images.DateTakenYear = @year AND images.ImageFolderPath LIKE '" + path + "' ORDER BY images.ImageFolderPath, images.FileName;";
            }
            else
            {
                sql1 = @"SELECT * FROM images WHERE DateTakenYear = @year ORDER BY ImageFolderPath, FileName";
                sql2 = @"SELECT tags.TagId, tags.TagName, images.ImageId FROM images
                            JOIN image_tags_join ON image_tags_join.ImageId = images.ImageId
                            JOIN tags ON image_tags_join.TagId = tags.TagId WHERE images.DateTakenYear = @year ORDER BY images.ImageFolderPath, images.FileName;";
            }
            List<Image> allImagesAtYear = (List<Image>)await _connection.QueryAsync<Image>(sql1, new { year }, transaction: txn);
            List<ImageTag> tags = (List<ImageTag>)await _connection.QueryAsync<ImageTag>(sql2, new { year }, transaction: txn);
            await txn.CommitAsync();
            await _connection.CloseAsync();
            return (allImagesAtYear, tags);
        }

        public async Task<(List<Image> images, List<ImageTag> tags)> GetAllImagesAtYearMonth(int year, int month, bool filterInCurrentDirectory, string currentDirectory)
        {
            await _connection.OpenAsync();
            MySqlTransaction txn = await _connection.BeginTransactionAsync();
            string path = PathHelper.FormatPathForLikeOperator(currentDirectory);
            string sql1 = string.Empty;
            string sql2 = string.Empty;
            if (filterInCurrentDirectory)
            {
                sql1 = @"SELECT * FROM images WHERE DateTakenYear = @year AND DateTakenMonth = @month AND ImageFolderPath LIKE '" + path + "' ORDER BY ImageFolderPath, FileName";
                sql2 = @"SELECT tags.TagId, tags.TagName, images.ImageId FROM images
                            JOIN image_tags_join ON image_tags_join.ImageId = images.ImageId
                            JOIN tags ON image_tags_join.TagId = tags.TagId WHERE images.DateTakenYear = @year AND images.DateTakenMonth = @month AND images.ImageFolderPath LIKE '" + path + "' ORDER BY images.ImageFolderPath, images.FileName;";
            }
            else
            {
                sql1 = @"SELECT * FROM images WHERE DateTakenYear = @year AND DateTakenMonth = @month ORDER BY ImageFolderPath, FileName";
                sql2 = @"SELECT tags.TagId, tags.TagName, images.ImageId FROM images
                            JOIN image_tags_join ON image_tags_join.ImageId = images.ImageId
                            JOIN tags ON image_tags_join.TagId = tags.TagId WHERE images.DateTakenYear = @year AND images.DateTakenMonth = @month ORDER BY images.ImageFolderPath, images.FileName;";
            }
            List<Image> allImagesAtYearMonth = (List<Image>)await _connection.QueryAsync<Image>(sql1, new { year, month }, transaction: txn);
            List<ImageTag> tags = (List<ImageTag>)await _connection.QueryAsync<ImageTag>(sql2, new { year, month }, transaction: txn);
            await txn.CommitAsync();
            await _connection.CloseAsync();
            return (allImagesAtYearMonth, tags);
        }

        public async Task<(List<Image> images, List<ImageTag> tags)> GetAllImagesInDateRange(DateTimeOffset startDate, DateTimeOffset endDate, bool filterInCurrentDirectory, string currentDirectory)
        {
            await _connection.OpenAsync();
            MySqlTransaction txn = await _connection.BeginTransactionAsync();
            string path = PathHelper.FormatPathForLikeOperator(currentDirectory);
            string sql1 = string.Empty;
            string sql2 = string.Empty;
            if (filterInCurrentDirectory)
            {
                sql1 = @"SELECT * FROM images WHERE DateTaken BETWEEN @startDate AND @endDate AND ImageFolderPath LIKE '" + path + "' ORDER BY ImageFolderPath, FileName";
                sql2 = @"SELECT tags.TagId, tags.TagName, images.ImageId FROM images
                            JOIN image_tags_join ON image_tags_join.ImageId = images.ImageId
                            JOIN tags ON image_tags_join.TagId = tags.TagId WHERE images.DateTaken BETWEEN @startDate AND @endDate AND images.ImageFolderPath LIKE '" + path + "' ORDER BY images.ImageFolderPath, images.FileName;";
            }
            else
            {
                sql1 = @"SELECT * FROM images WHERE DateTaken BETWEEN @startDate AND @endDate ORDER BY ImageFolderPath, FileName";
                sql2 = @"SELECT tags.TagId, tags.TagName, images.ImageId FROM images
                            JOIN image_tags_join ON image_tags_join.ImageId = images.ImageId
                            JOIN tags ON image_tags_join.TagId = tags.TagId WHERE images.DateTaken BETWEEN @startDate AND @endDate ORDER BY images.ImageFolderPath, images.FileName;";
            }
            List<Image> allImagesInDateRange = (List<Image>)await _connection.QueryAsync<Image>(sql1, new { startDate = startDate.Date, endDate = endDate.Date }, transaction: txn);
            List<ImageTag> tags = (List<ImageTag>)await _connection.QueryAsync<ImageTag>(sql2, new { startDate = startDate.Date, endDate = endDate.Date }, transaction: txn);
            await txn.CommitAsync();
            await _connection.CloseAsync();
            return (allImagesInDateRange, tags);
        }

        public async Task<(List<Image> images, List<ImageTag> tags)> GetAllImagesWithTag(string tag, bool filterInCurrentDirectory, string currentDirectory)
        {
            await _connection.OpenAsync();
            MySqlTransaction txn = await _connection.BeginTransactionAsync();
            string path = PathHelper.FormatPathForLikeOperator(currentDirectory);
            string sql1 = string.Empty;
            string sql2 = string.Empty;
            if (filterInCurrentDirectory) 
            {
                sql1 = @"SELECT * FROM images 
                            JOIN image_tags_join ON image_tags_join.ImageId = images.ImageId
                            JOIN tags ON image_tags_join.TagId = tags.TagId WHERE tags.TagName = @tag AND ImageFolderPath LIKE '" + path + "' ORDER BY images.ImageFolderPath, images.FileName;";
                sql2 = @"SELECT tags.TagId, tags.TagName, images.ImageId FROM images 
                            JOIN image_tags_join ON image_tags_join.ImageId = images.ImageId
                            JOIN tags ON image_tags_join.TagId = tags.TagId WHERE ImageFolderPath LIKE '" + path + "' ORDER BY images.ImageFolderPath, images.FileName;";
            }
            else
            {
                sql1 = @"SELECT * FROM images 
                            JOIN image_tags_join ON image_tags_join.ImageId = images.ImageId
                            JOIN tags ON image_tags_join.TagId = tags.TagId WHERE tags.TagName = @tag ORDER BY images.ImageFolderPath, images.FileName;";
                sql2 = @"SELECT tags.TagId, tags.TagName, images.ImageId FROM images 
                            JOIN image_tags_join ON image_tags_join.ImageId = images.ImageId
                            JOIN tags ON image_tags_join.TagId = tags.TagId ORDER BY images.ImageFolderPath, images.FileName;";
            }

            List<Image> allImagesWithTag = (List<Image>)await _connection.QueryAsync<Image>(sql1, new { tag }, transaction: txn);
            List<ImageTag> tags = (List<ImageTag>)await _connection.QueryAsync<ImageTag>(sql2, new { tag }, transaction: txn);
            await txn.CommitAsync();
            await _connection.CloseAsync();
            return (allImagesWithTag, tags);
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
            //insert newTag if its already there IGNORE
            string sql1 = @"INSERT IGNORE INTO tags (TagName) VALUES (@newTag)";
            rowsEffectedA = await _connection.ExecuteAsync(sql1, new { newTag = newTag }, transaction: txn);
            //get newTag id
            string sql2 = @"SELECT TagId FROM tags WHERE TagName = @newTag";
            int newTagId = await _connection.QuerySingleOrDefaultAsync<int>(sql2, new { newTag }, transaction: txn);
            //insert into image_tags_join if its already there IGNORE
            string sql3 = @"INSERT IGNORE INTO image_tags_join (ImageId, TagId) VALUES (@imageId, @tagId)";
            rowsEffectedB = await _connection.ExecuteAsync(sql3, new { imageId = image.ImageId, tagId = newTagId }, transaction: txn);

            await txn.CommitAsync();
            await _connection.CloseAsync();
            return rowsEffectedA + rowsEffectedB >= 1 ? true : false;

        }

        public async Task<bool> AddMultipleImageTags(string sql)
        {
            int rowsEffected = await _connection.ExecuteAsync(sql);
            await _connection.CloseAsync();
            return rowsEffected > 0 ? true : false;
        }

        public async Task<bool> DeleteImageTag(ImageTag tag)
        {
            int rowsEffected = 0;
            string sql = @"DELETE FROM image_tags_join WHERE ImageId = @imageId AND TagId = @tagId";
            rowsEffected = await _connection.ExecuteAsync(sql, new { imageId = tag.ImageId, tagId = tag.TagId });
            await _connection.CloseAsync();
            return rowsEffected > 0 ? true : false;
        }

        public async Task<bool> DeleteSelectedImages(string sql)
        {
            int rowsEffected = await _connection.ExecuteAsync(sql);
            await _connection.CloseAsync();
            return rowsEffected > 0 ? true : false;
        }

        public async Task<bool> MoveSelectedImageToNewFolder(string sql)
        {
            int rowsEffected = await _connection.ExecuteAsync(sql);
            await _connection.CloseAsync();
            return rowsEffected > 0 ? true : false;
        }

        public async Task<List<Tag>> GetTagsList()
        {
            string sql = @"SELECT * FROM tags";
            List<Tag> tags = (List<Tag>)await _connection.QueryAsync<Tag>(sql);
            await _connection.CloseAsync();
            return tags;
        }

        public async Task<bool> UpdateImageTagsAndRatingFromMetaData(List<Image> imagesPlusUpdatedMetaData, int folderId)
        {
            await using MySqlConnection conn = new MySqlConnection(_connectionString); //need a new connection pre folder as this is a parallel method
            await conn.OpenAsync();
            await using MySqlTransaction txn = await conn.BeginTransactionAsync();

            string bulkImageRating = SqlStringBuilder.BuildSqlForBulkInsertImageRating(imagesPlusUpdatedMetaData);
            string updateFolderContentMetaDataScanned = @"UPDATE folders set FolderContentMetaDataScanned = 1 WHERE FolderId = @folderId";

            try
            {
                await conn.ExecuteAsync(bulkImageRating, transaction: txn);
                await conn.ExecuteAsync(updateFolderContentMetaDataScanned, new { folderId }, transaction: txn);

                //get a list of distinct tags
                List<string> allTags = imagesPlusUpdatedMetaData.SelectMany(img => img.Tags).Select(tag => tag.TagName).Distinct().ToList();
                //no tags to insert -- commit Ratings close connection and return tags will not run and no commits will be made
                if (allTags.Count == 0)
                {
                    await txn.CommitAsync();
                    await conn.CloseAsync();
                    return true;
                }

                //bulk insert all distinct tags into tags table -- IGNORE duplicates
                string bulkInsertTags = SqlStringBuilder.BuildSqlForBulkInsertImageTags(allTags);
                await conn.ExecuteAsync(bulkInsertTags, transaction: txn);

                //get all tag id's 
                string getAllTagIds = @"SELECT TagId, TagName FROM tags WHERE TagName IN @tagNames";
                List<Tag> allTagsWithIds = (List<Tag>)await conn.QueryAsync<Tag>(getAllTagIds, new { tagNames = allTags }, transaction: txn);

                //build image tags join
                List<(int imageId, int tagId)> imageTagsJoin = new List<(int imageId, int tagId)>();
                foreach (Image image in imagesPlusUpdatedMetaData)
                {
                    foreach (ImageTag tag in image.Tags)
                    {
                        Tag? tagWithId = allTagsWithIds.Find(x => x.TagName == tag.TagName);
                        if (tagWithId != null)
                        {
                            imageTagsJoin.Add((tag.ImageId, tagWithId.TagId));
                        }
                    }
                }

                //clear all tag joins in one shot -- no duplicates when rescan
                string deleteTagJoins = @"DELETE FROM image_tags_join WHERE ImageId IN @imageIds";
                await conn.ExecuteAsync(deleteTagJoins, new { imageIds = imagesPlusUpdatedMetaData.Select(i => i.ImageId) }, transaction: txn);

                //bulk insert all tag joins 
                string bulkInsertTagJoin = SqlStringBuilder.BuildSqlForBulkInsertImageTagsJoin(imageTagsJoin);
                await conn.ExecuteAsync(bulkInsertTagJoin, transaction: txn);
                await txn.CommitAsync();
                await conn.CloseAsync();
                return true;
            }
            catch (Exception ex)
            {
                await txn.RollbackAsync();
                await conn.CloseAsync();
                return false;
            }   
        }

        public async Task<bool> RemoveTagOnAllImages(Tag selectedTag)
        {
            int rowsEffectedA = 0;
            int rowsEffectedB = 0;
            await _connection.OpenAsync();
            MySqlTransaction txn = await _connection.BeginTransactionAsync();

            //remove from image_tags_join 
            string removeImageTagsJoin = @"DELETE FROM image_tags_join WHERE TagId = @tagId";
            rowsEffectedA = await _connection.ExecuteAsync(removeImageTagsJoin, new { tagId = selectedTag.TagId }, transaction: txn);

            //check for tag in folder_tags_join
            string tagInFolderTagsJoin = @"SELECT COUNT(*) FROM folder_Tags_join WHERE TagId = @tagId";
            int numTagInFolderTagsJoin = await _connection.QuerySingleAsync<int>(tagInFolderTagsJoin, new { tagId = selectedTag.TagId }, transaction: txn);

            //if tag not present in folder_tags_join
            //  --remove from tags table
            if(numTagInFolderTagsJoin == 0)
            {
                string removeFromTagsTable = @"DELETE FROM tags WHERE TagId = @tagId";
                rowsEffectedB = await _connection.ExecuteAsync(removeFromTagsTable, new { tagId = selectedTag.TagId }, transaction: txn);
            }
            await txn.CommitAsync();
            await _connection.CloseAsync();
            return rowsEffectedA + rowsEffectedB >= 1 ? true : false;
        }
        public async Task<int> GetTotalImages()
        {
            string sql = @"SELECT COUNT(*) FROM images";
            return await _connection.QuerySingleAsync<int>(sql);
        }

        public async Task UpdateImageDates()
        {
            await _connection.OpenAsync();
            MySqlTransaction txn = await _connection.BeginTransactionAsync();
            //clear existing dates
            string sql1 = @"TRUNCATE TABLE image_dates;";
            //populate image_dates from images
            string sql2 = @"INSERT INTO image_dates (DateTaken, Year, Month, Day)
                                SELECT DISTINCT 
                                       DateTaken, 
                                       DateTakenYear, 
                                       DateTakenMonth, 
                                       DateTakenDay
                                FROM images
                                WHERE DateTaken IS NOT NULL;";

            await _connection.ExecuteAsync(sql1, transaction: txn);
            await _connection.ExecuteAsync(sql2 , transaction: txn);

            await txn.CommitAsync();
            await _connection.CloseAsync();
        }

        public async Task<ImageDatesViewModel> GetImageDates()
        {
            ImageDatesViewModel viewModel = new ImageDatesViewModel();

            string sql = @"
                SELECT DISTINCT Year FROM image_dates ORDER BY Year DESC;
                SELECT DISTINCT YearMonth FROM image_dates ORDER BY YearMonth DESC;
                SELECT MIN(DateTaken) FROM image_dates;
                SELECT MAX(DateTaken) FROM image_dates;
            ";

            using (var multi = await _connection.QueryMultipleAsync(sql))
            {
                IEnumerable<int> years = (await multi.ReadAsync<int>()).ToList();
                IEnumerable<string> yearMonths = (await multi.ReadAsync<string>()).ToList();
                DateTime? minDateRaw = await multi.ReadFirstOrDefaultAsync<DateTime?>();
                DateTime? maxDateRaw = await multi.ReadFirstOrDefaultAsync<DateTime?>();

                DateTimeOffset? minDate = minDateRaw.HasValue ? new DateTimeOffset(minDateRaw.Value) : null;
                DateTimeOffset? maxDate = maxDateRaw.HasValue ? new DateTimeOffset(maxDateRaw.Value) : null;

                viewModel.Years = new ObservableCollection<int>(years);
                viewModel.YearMonths = new ObservableCollection<string>(yearMonths);
                viewModel.StartDate = minDate;
                viewModel.EndDate = maxDate;
            }

            await _connection.CloseAsync();
            return viewModel;
        }
    }
}

using ImagePerfect.Models;
using ImagePerfect.ViewModels;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;


namespace ImagePerfect.Helpers
{
    public static class SqlStringBuilder
    {
        public static string BuildSqlForMoveImagesToTrash(List<ImageViewModel> images)
        {
            StringBuilder sb = new StringBuilder("DELETE FROM images WHERE ImageId IN (");
            for (int i = 0; i < images.Count; i++) 
            { 
                if(i <  images.Count - 1)
                {
                    sb.Append($"{images[i].ImageId},");
                }
                else
                {
                    sb.Append($"{images[i].ImageId}");
                }
            }
            sb.Append(");");
            return sb.ToString();
        }

        public static string BuildSqlForMoveImagesToNewFolder(List<ImageViewModel> imagesToMove)
        {
            StringBuilder sb = new StringBuilder("UPDATE images SET ImagePath = CASE ");
            foreach (ImageViewModel image in imagesToMove) 
            {
                sb.Append($"WHEN ImageId = {image.ImageId} THEN '{PathHelper.FormatPathForDbStorage(image.ImagePath)}' ");
            }
            sb.Append("ELSE ImagePath END, ImageFolderPath = CASE ");
            foreach(ImageViewModel image in imagesToMove)
            {
                sb.Append($"WHEN ImageId = {image.ImageId} THEN '{PathHelper.FormatPathForDbStorage(image.ImageFolderPath)}' ");
            }
            sb.Append("ELSE ImageFolderPath END, FolderId = CASE ");
            foreach(ImageViewModel image in imagesToMove)
            {
                sb.Append($"WHEN ImageId = {image.ImageId} THEN {image.FolderId} ");
            }
            sb.Append($"ELSE FolderId END WHERE ImageId IN (");
            for(int i = 0; i < imagesToMove.Count; i++)
            {
                if(i < imagesToMove.Count - 1)
                {
                    sb.Append($"{imagesToMove[i].ImageId},");
                }
                else
                {
                    sb.Append($"{imagesToMove[i].ImageId}");
                }
            }
            sb.Append(");");
            return sb.ToString();
        }
        public static string BuildFolderSqlForFolderMove(List<Folder> folders)
        {
            //need two sql string one that updates both folder path and coverimage path
            //and one just for folder path where there are no cover images
            int coverImageCount = 0;
            foreach(Folder folder in folders)
            {
                if(folder.CoverImagePath != "")
                {
                    coverImageCount = coverImageCount + 1;
                }
            }

            StringBuilder sb = new StringBuilder("UPDATE folders SET FolderPath = CASE ");
            foreach (Folder folder in folders)
            {
                sb.Append($"WHEN FolderId = {folder.FolderId} THEN '{PathHelper.FormatPathForDbStorage(folder.FolderPath)}' ");
            }
            if (coverImageCount > 0)
            {
                sb.Append("ELSE FolderPath End, CoverImagePath = CASE ");
                foreach (Folder folder in folders)
                {
                    if (folder.CoverImagePath != "")
                    {
                        sb.Append($"WHEN FolderId = {folder.FolderId} THEN '{PathHelper.FormatPathForDbStorage(folder.CoverImagePath)}' ");
                    }
                }
                sb.Append("ELSE CoverImagePath END WHERE FolderId IN (");
                for (int i = 0; i < folders.Count; i++)
                {
                    if (i < folders.Count - 1)
                    {
                        sb.Append($"{folders[i].FolderId},");
                    }
                    else
                    {
                        sb.Append($"{folders[i].FolderId}");
                    }
                }
                sb.Append(");");
            }
            else
            {
                sb.Append("ELSE FolderPath END WHERE FolderId IN (");
                for (int i = 0; i < folders.Count; i++)
                {
                    if (i < folders.Count - 1)
                    {
                        sb.Append($"{folders[i].FolderId},");
                    }
                    else
                    {
                        sb.Append($"{folders[i].FolderId}");
                    }
                }
                sb.Append(");");
            }
            
            return sb.ToString();
        }

        public static string BuildImageSqlForFolderMove(List<Image> images)
        {
            StringBuilder sb = new StringBuilder("UPDATE images SET ImagePath = CASE ");
            foreach (Image image in images) 
            {
                sb.Append($"WHEN ImageId = {image.ImageId} THEN '{PathHelper.FormatPathForDbStorage(image.ImagePath)}' ");
            }
            sb.Append("ELSE ImagePath END, ImageFolderPath = CASE ");
            foreach (Image image in images)
            {
                sb.Append($"WHEN ImageId = {image.ImageId} THEN '{PathHelper.FormatPathForDbStorage(image.ImageFolderPath)}' ");
            }
            sb.Append("ELSE ImageFolderPath END WHERE ImageId IN (");
            for(int i = 0; i < images.Count; i++)
            {
                if(i < images.Count - 1)
                {
                    sb.Append($"{images[i].ImageId},");
                }
                else
                {
                    sb.Append($"{images[i].ImageId}");
                }
            }
            sb.Append(");");
            return sb.ToString();
        }

        public static string BuildImageSqlForScanMetadata(List<Image> images)
        {
            StringBuilder sb = new StringBuilder("UPDATE images SET ImageRating = CASE ");
            foreach (Image image in images) 
            {
                sb.Append($"WHEN ImageId = {image.ImageId} THEN {image.ImageRating} ");
            }
            sb.Append("ELSE ImageRating END, ImageMetaDataScanned = CASE ");
            foreach (Image image in images)
            {
                sb.Append($"WHEN ImageId = {image.ImageId} THEN 1 ");
            }
            sb.Append("ELSE ImageMetaDataScanned END WHERE ImageId IN (");
            for (int i = 0; i < images.Count; i++) 
            {
                if (i < images.Count - 1)
                {
                    sb.Append($"{images[i].ImageId},");
                }
                else
                {
                    sb.Append($"{images[i].ImageId}");
                }
            }
            sb.Append(");");
            return sb.ToString();
        }

        public static string BuildSqlForBulkInsertImageTags(List<string> allTags)
        {
            StringBuilder sb = new StringBuilder("INSERT IGNORE INTO tags (TagName) VALUES ");
            for(int i = 0; i < allTags.Count; i++)
            {
                if(i < allTags.Count - 1)
                {
                    sb.Append($"('{allTags[i]}'),");
                }
                else
                {
                    sb.Append($"('{allTags[i]}');");
                }
            }
            return sb.ToString();
        }

        public static string BuildSqlForBulkInsertImageTagsJoin(List<(int imageId, int tagId)> imageTagJoins)
        {
            StringBuilder sb = new StringBuilder("INSERT INTO image_tags_join (ImageId, TagId) VALUES ");
            for(int i = 0; i < imageTagJoins.Count; i++)
            {
                if(i < imageTagJoins.Count - 1)
                {
                    sb.Append($"({imageTagJoins[i].imageId}, {imageTagJoins[i].tagId}),");
                }
                else
                {
                    sb.Append($"({imageTagJoins[i].imageId}, {imageTagJoins[i].tagId});");
                }

            }
            return sb.ToString();
        }
        public static string BuildSqlForAddMultipleImageTags(List<Tag> tagsToAdd, ImageViewModel imageVm)
        {
            StringBuilder sb = new StringBuilder("INSERT INTO image_tags_join (ImageId, TagId) VALUES ");
            for(int i = 0; i < tagsToAdd.Count; i++)
            {
                if(i < tagsToAdd.Count - 1)
                {
                    sb.Append($"({imageVm.ImageId}, {tagsToAdd[i].TagId}),");
                }
                else
                {
                    sb.Append($"({imageVm.ImageId}, {tagsToAdd[i].TagId});");
                }
            }
            
            return sb.ToString();
        }
    }
}

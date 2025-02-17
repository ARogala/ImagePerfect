using ImagePerfect.Models;
using System.Collections.Generic;
using System.Text;


namespace ImagePerfect.Helpers
{
    public static class SqlStringBuilder
    {
        public static string BuildFolderSqlForFolderMove(List<Folder> folders)
        {
            StringBuilder sb = new StringBuilder("UPDATE folders SET FolderPath = CASE ");
            foreach (Folder folder in folders)
            {
                sb.Append($"WHEN FolderId = {folder.FolderId} THEN '{PathHelper.FormatPathForDbStorage(folder.FolderPath)}' ");
            }
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
    }
}

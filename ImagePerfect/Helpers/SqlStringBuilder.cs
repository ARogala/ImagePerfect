using ImagePerfect.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            sb.Append("ELSE CoverImagePath END WHERE FolderId In (");
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
    }
}

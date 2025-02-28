using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ImagePerfect.Models
{
    [Table("folders")]
    public class Folder
    {
        [Key]
        [Column("FolderId")]
        public int FolderId { get; set; }

        [Column("FolderName")]
        public string FolderName { get; set; } = string.Empty;

        [Column("FolderPath")]
        public string FolderPath { get; set; } = string.Empty;

        [Column("HasChildren")]
        public bool HasChildren { get; set; }

        [Column("CoverImagePath")]
        public string? CoverImagePath { get; set; }

        [Column("FolderDescription")]
        public string? FolderDescription { get; set; }

        [Column("FolderRating")]
        public int FolderRating { get; set; }

        [Column("HasFiles")]
        public bool HasFiles { get; set; }

        [Column("IsRoot")]
        public bool IsRoot { get; set; }

        [Column("FolderContentMetaDataScanned")]
        public bool FolderContentMetaDataScanned { get; set; }

        [Column("AreImagesImported")]
        public bool AreImagesImported { get; set; }
    }
}

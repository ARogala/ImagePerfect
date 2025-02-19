using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ImagePerfect.Models
{
    [Table("images")]
    public class Image
    {
        [Key]
        [Column("ImageId")]
        public int ImageId { get; set; }

        [Column("ImagePath")]
        public string ImagePath { get; set; }

        [Column("FileName")]
        public string FileName { get; set; }

        [Column("ImageTags")]
        public string? ImageTags { get; set; }

        [Column("ImageRating")]
        public int ImageRating { get; set; }

        [Column("ImageFolderPath")]
        public string ImageFolderPath { get; set; } = string.Empty;

        [Column("ImageMetaDataScanned")]
        public bool ImageMetaDataScanned { get; set; }

        [ForeignKey("FolderId")]
        [Column("FolderId")]
        public int FolderId { get; set; }

    }
}

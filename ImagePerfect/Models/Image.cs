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

        [Column("ImageTags")]
        public string ImageTags { get; set; } = string.Empty;

        [Column("ImageRating")]
        public int ImageRating { get; set; }

        [Column("ImageFolderPath")]
        public string ImageFolderPath { get; set; }

        [Column("ImageMetaDataScanned")]
        public bool ImageMetaDataScanned { get; set; }

        [ForeignKey("FolderId")]
        [Column("FolderId")]
        public int FolderId { get; set; }

    }
}

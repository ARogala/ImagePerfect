using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ImagePerfect.Models
{
    [Table("saved_directory")]
    public class SaveDirectory
    {
        [Key]
        [Column("SavedDirectoryId")]
        public int SavedDirectoryId { get; set; }

        [Column("SavedDirectory")]
        public string SavedDirectory { get; set; }

        [Column("SavedFolderPage")]
        public int SavedFolderPage { get; set; }

        [Column("SavedTotalFolderPages")]
        public int SavedTotalFolderPages { get; set; }

        [Column("SavedImagePage")]
        public int SavedImagePage { get; set; }

        [Column("SavedTotalImagePages")]
        public int SavedTotalImagePages { get; set; }

        [Column("XVector")]
        public double XVector {  get; set; }

        [Column("YVector")]
        public double YVector { get; set; }
    }
}

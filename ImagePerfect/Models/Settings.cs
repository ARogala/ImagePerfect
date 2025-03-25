using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ImagePerfect.Models
{
    [Table("settings")]
    public class Settings
    {
        [Key]
        [Column("SettingsId")]
        public int SettingsId { get; set; }

        [Column("MaxImageWidth")]
        public int MaxImageWidth { get; set; }
        
        [Column("FolderPageSize")]
        public int FolderPageSize { get; set; }

        [Column("ImagePageSize")]
        public int ImagePageSize { get; set; }
    }
}

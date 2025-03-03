using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace ImagePerfect.Models
{
    [Table("tags")]
    public class Tag
    {
        [Key]
        [Column("TagId")]
        public int TagId { get; set; }

        [Column("TagName")]
        public string TagName { get; set; } = string.Empty;

        [Column("FolderId")]
        public int FolderId { get; set; }  
    }
}

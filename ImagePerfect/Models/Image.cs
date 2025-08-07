using System;
using System.Collections.Generic;
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

        [Column("ImageRating")]
        public int ImageRating { get; set; }

        [Column("ImageFolderPath")]
        public string ImageFolderPath { get; set; } = string.Empty;

        [Column("ImageMetaDataScanned")]
        public bool ImageMetaDataScanned { get; set; }

        [ForeignKey("FolderId")]
        [Column("FolderId")]
        public int FolderId { get; set; }

        [Column("DateTaken")]
        public DateTime? DateTaken { get; set; }

        [Column("DateTakenYear")]
        public int? DateTakenYear { get; set; }

        [Column("DateTakenMonth")]
        public int? DateTakenMonth { get; set; }

        [Column("DateTakenDay")]
        public int? DateTakenDay { get; set; }

        //for many to many relationship image_tags_join
        [NotMapped]
        public List<ImageTag> Tags { get; set; } = new List<ImageTag>();  
    }
}

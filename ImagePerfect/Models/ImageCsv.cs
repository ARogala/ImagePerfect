namespace ImagePerfect.Models
{
    public class ImageCsv
    {
        public int ImageId { get; set; }
        public string ImagePath {  get; set; }
        public string FileName { get; set; }
        public int  ImageRating {  get; set; }
        public string ImageFolderPath { get; set; }
        public int ImageMetaDataScanned { get; set; }
        public int FolderId { get; set; }

    }
}



namespace ImagePerfect.Models
{
    public class FolderCsv
    {
        public int FolderId { get; set; }
        public string FolderName { get; set; } = string.Empty;
        public string FolderPath { get; set; } = string.Empty;
        public int HasChildren { get; set; }
        public string? CoverImagePath { get; set; }
        public string? FolderDescription { get; set; }
        public int FolderRating { get; set; }
        public int HasFiles { get; set; }
        public int IsRoot { get; set; }
        public int FolderContentMetaDataScanned { get; set; }
        public int AreImagesImported { get; set; }
    }
}

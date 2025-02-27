using Avalonia.Media.Imaging;
using ReactiveUI;

namespace ImagePerfect.ViewModels
{
	public class FolderViewModel : ViewModelBase
	{
		private int _folderId;
		private string _folderName;
		private string _folderPath;
		private bool _hasChildren;
		private Bitmap? _coverImageBitmap;
		private string? _coverImagePath;
		private string? _folderDescription;
		private string? _folderTags;
        private string _newTag;
        private int _folderRating;
		private bool _hasFiles;
		private bool _isRoot;
		private bool _folderContentMetaDataScanned;
		private bool _areImagesImported;
		private bool _showImportImagesButton;

		public int FolderId 
		{
			get => _folderId;
			set => this.RaiseAndSetIfChanged(ref _folderId, value);
		}

		public string FolderName
		{
			get => _folderName;
			set => this.RaiseAndSetIfChanged(ref _folderName, value);
		}
		public string FolderPath
		{
			get => _folderPath;
			set => this.RaiseAndSetIfChanged(ref _folderPath, value);
		}
		public bool HasChildren
		{
			get => _hasChildren;
			set => this.RaiseAndSetIfChanged(ref _hasChildren, value);
		}
		public Bitmap? CoverImageBitmap
		{
			get => _coverImageBitmap;
			set => this.RaiseAndSetIfChanged(ref _coverImageBitmap, value);	
		}
		public string? CoverImagePath
		{
			get => _coverImagePath;
			set => this.RaiseAndSetIfChanged(ref _coverImagePath, value);
		}
		public string? FolderDescription
		{
			get => _folderDescription;
			set => this.RaiseAndSetIfChanged(ref _folderDescription, value);
		}
		public string? FolderTags
		{
			get => _folderTags;
			set => this.RaiseAndSetIfChanged(ref _folderTags, value);
		}
        public string NewTag
        {
            get => _newTag;
            set => this.RaiseAndSetIfChanged(ref _newTag, value);
        }
        public int FolderRating
		{
			get => _folderRating;
			set => this.RaiseAndSetIfChanged(ref _folderRating, value);
		}
		public bool HasFiles 
		{
			get => _hasFiles;
			set => this.RaiseAndSetIfChanged(ref _hasFiles, value);
		}
		public bool IsRoot
		{
			get => _isRoot;
			set => this.RaiseAndSetIfChanged(ref _isRoot, value);
		}
		public bool FolderContentMetaDataScanned
		{
			get => _folderContentMetaDataScanned;
			set => this.RaiseAndSetIfChanged(ref _folderContentMetaDataScanned, value);
		}

		public bool AreImagesImported
		{
			get => _areImagesImported;
			set => this.RaiseAndSetIfChanged(ref _areImagesImported, value);
		}
		public bool ShowImportImagesButton
		{
			get => _showImportImagesButton;
			set => this.RaiseAndSetIfChanged(ref _showImportImagesButton, value);
		}
	}
}
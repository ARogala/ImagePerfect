using Avalonia.Media.Imaging;
using ImagePerfect.Models;
using ReactiveUI;
using System.Collections.Generic;

namespace ImagePerfect.ViewModels
{
	public class ImageViewModel : ViewModelBase
	{
		private int _imageId;
		private Bitmap _imageBitmap;
		private string _imagePath;
		private string _fileName;
		private string? _imageTags;
		private string? _newTag;
		private int _imageRating;
		private string _imageFolderPath;
		private bool _imageMetaDataScanned;
		private int _folderId;
		private bool _isSelected = false;
        private bool _showAddMultipleImageTags = false;

        public ImageViewModel() { }
		//copy constructor so i can make a deep copy of this object
        public ImageViewModel(ImageViewModel imageVm)
        {
			ImageId = imageVm.ImageId;
			ImageBitmap = imageVm.ImageBitmap;
			ImagePath = imageVm.ImagePath;
			FileName = imageVm.FileName;
			ImageTags = imageVm.ImageTags;	
			NewTag = imageVm.NewTag;
			ImageRating = imageVm.ImageRating;
			ImageFolderPath = imageVm.ImageFolderPath;
			ImageMetaDataScanned = imageVm.ImageMetaDataScanned;
			FolderId = imageVm.FolderId;
			IsSelected = imageVm.IsSelected;
			ShowAddMultipleImageTags = imageVm.ShowAddMultipleImageTags;
			Tags = imageVm.Tags;
        }
        public int ImageId
		{
			get => _imageId;
			set => this.RaiseAndSetIfChanged(ref _imageId, value);
		}
		public Bitmap ImageBitmap
		{
			get => _imageBitmap;
			set => this.RaiseAndSetIfChanged(ref _imageBitmap, value);
		}
		public string ImagePath
		{
			get => _imagePath;
			set => this.RaiseAndSetIfChanged(ref _imagePath, value);
		}
		public string FileName
		{
			get => _fileName;
			set => this.RaiseAndSetIfChanged(ref _fileName, value);
		}
        //used to display the csv string on the Remove Tag TextBox
        public string? ImageTags
		{
			get => _imageTags;
			set => this.RaiseAndSetIfChanged(ref _imageTags, value);
		}
        public string? NewTag
		{
			get => _newTag;
            set
            {
                this.RaiseAndSetIfChanged(ref _newTag, value);
                if (value != null)
                {
					if(value.Contains(" "))
					{
                        _newTag = _newTag.Trim();
                    }
                }
            }
        }
		public int ImageRating
		{
			get => _imageRating;
			set => this.RaiseAndSetIfChanged(ref _imageRating, value);
		}
		public string ImageFolderPath
		{
			get => _imageFolderPath;
			set => this.RaiseAndSetIfChanged(ref _imageFolderPath, value);
		}
		public bool ImageMetaDataScanned
		{
			get => _imageMetaDataScanned;
			set => this.RaiseAndSetIfChanged(ref _imageMetaDataScanned, value);
		}
		public int FolderId
		{
			get => _folderId;
			set => this.RaiseAndSetIfChanged(ref _folderId, value);
		}
		public bool IsSelected
		{
			get => _isSelected;
			set => this.RaiseAndSetIfChanged(ref _isSelected, value);
		}
        public bool ShowAddMultipleImageTags
        {
            get => _showAddMultipleImageTags;
            set => this.RaiseAndSetIfChanged(ref _showAddMultipleImageTags, value);
        }
        //for many to many relationship folder_tags_join
        public List<ImageTag> Tags { get; set; } = new List<ImageTag>();
    }
}
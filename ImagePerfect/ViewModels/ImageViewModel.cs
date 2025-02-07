using Avalonia.Media.Imaging;
using ReactiveUI;

namespace ImagePerfect.ViewModels
{
	public class ImageViewModel : ViewModelBase
	{
		private int _imageId;
		private Bitmap _imageBitmap;
		private string _imagePath;
		private string? _imageTags;
		private int _imageRating;
		private string _imageFolderPath;
		private bool _imageMetaDataScanned;
		private int _folderId;

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
		public string? ImageTags
		{
			get => _imageTags;
			set => this.RaiseAndSetIfChanged(ref _imageTags, value);
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

	}
}
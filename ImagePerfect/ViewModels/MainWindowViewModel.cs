using Avalonia.Media.Imaging;
using ImagePerfect.Helpers;
using ImagePerfect.Models;
using ImagePerfect.Repository.IRepository;
using MsBox.Avalonia.Enums;
using MsBox.Avalonia;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Reactive;
using ImagePerfect.ObjectMappers;
using System.Linq;
using DynamicData;
using System.IO;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Avalonia.Controls;
using Image = ImagePerfect.Models.Image;

namespace ImagePerfect.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly FolderMethods _folderMethods;
        private readonly ImageCsvMethods _imageCsvMethods;
        private readonly ImageMethods _imageMethods;
        private readonly SettingsMethods _settingsMethods;
        private bool _showLoading;
        private bool _showFilters = false;
        private bool _showSettings = false;
        private bool _showManageImages = false;
        private bool _showCreateNewFolder = false;
        private bool _showTotalImages = false;
        private bool _showImportAndScan = false;
        private bool _showAllTags = false;
        private int _totalImages = 0;
        private string _currentDirectory;
        private string _savedDirectory;
        private string _selectedImagesNewDirectory = string.Empty;
        private bool _filterInCurrentDirectory = false;
        private string _rootFolderLocation;
        private string _newFolderName;
        private bool _isNewFolderEnabled;
        private List<string> _tagsList = new List<string>();

        private List<Folder> displayFolders = new List<Folder>();
        private List<FolderTag> displayFolderTags = new List<FolderTag>();
        private List<Image> displayImages = new List<Image>();
        private List<ImageTag> displayImageTags = new List<ImageTag>();
        //pagination
        private int _folderPageSize = 20;
        private int _totalFolderPages = 1;
        private int _currentFolderPage = 1;
        private int _savedTotalFolderPages = 1;
        private int _savedFolderPage = 1;

        private int _imagePageSize = 20;
        private int _totalImagePages = 1;
        private int _currentImagePage = 1;
        private int _savedTotalImagePages = 1;
        private int _savedImagePage = 1;
        //max value between TotalFolderPages or TotalImagePages
        private int _maxPage = 1;
        //max value between CurrentFolderPage or CurrentImagePage
        private int _maxCurrentPage = 1;

        //Filters
        private enum filters
        {
            None,
            ImageRatingFilter,
            FolderRatingFilter,
            ImageTagFilter,
            FolderTagFilter,
            FolderDescriptionFilter,
            AllFavoriteFolders,
            AllFoldersWithNoImportedImages,
            AllFoldersWithMetadataNotScanned
        }
        private filters currentFilter = filters.None;
        private int selectedRatingForFilter = 0;
        private string tagForFilter = string.Empty;
        private string textForFilter = string.Empty;

        //max of 600 min of 300
        private int _maxImageWidth = 600;

        public MainWindowViewModel() { }
        public MainWindowViewModel(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _folderMethods = new FolderMethods(_unitOfWork);
            _imageCsvMethods = new ImageCsvMethods(_unitOfWork);
            _imageMethods = new ImageMethods(_unitOfWork);
            _settingsMethods = new SettingsMethods(_unitOfWork);
            _showLoading = false;

            NextFolderCommand = ReactiveCommand.Create((FolderViewModel currentFolder) => {
                NextFolder(currentFolder);
            });
            BackFolderCommand = ReactiveCommand.Create((FolderViewModel currentFolder) => {
                BackFolder(currentFolder);
            });
            BackFolderFromImageCommand = ReactiveCommand.Create((ImageViewModel imageVm) => {
                BackFolderFromImage(imageVm);
            });
            ImportImagesCommand = ReactiveCommand.Create(async (FolderViewModel imageFolder) => {
                await ImportImages(imageFolder);
            });
            AddFolderDescriptionCommand = ReactiveCommand.Create((FolderViewModel folderVm) => {
                UpdateFolder(folderVm, "Description");
            });
            AddFolderTagsCommand = ReactiveCommand.Create((FolderViewModel folderVm) => {
                AddFolderTag(folderVm);
            });
            EditFolderTagsCommand = ReactiveCommand.Create((FolderViewModel folderVm) => {
                EditFolderTag(folderVm);
            });
            AddFolderRatingCommand = ReactiveCommand.Create((FolderViewModel folderVm) => {
                UpdateFolder(folderVm, "Rating");
            });
            AddImageTagsCommand = ReactiveCommand.Create((ImageViewModel imageVm) => {
                AddImageTag(imageVm);
            });
            EditImageTagsCommand = ReactiveCommand.Create((ImageViewModel imageVm) => {
                EditImageTag(imageVm);
            });
            AddImageRatingCommand = ReactiveCommand.Create((ImageViewModel imageVm) => {
                UpdateImage(imageVm, "Rating");
            });
            DeleteLibraryCommand = ReactiveCommand.Create(() => {
                DeleteLibrary();
            });
            OpenImageInExternalViewerCommand = ReactiveCommand.Create((ImageViewModel imageVm) => {
                OpenImageInExternalViewer(imageVm);
            });
            OpenCurrentDirectoryWithExplorerCommand = ReactiveCommand.Create(() => {
                OpenCurrentDirectoryWithExplorer();
            });
            MoveImageToTrashCommand = ReactiveCommand.Create(async (ImageViewModel imageVm) => {
                await MoveImageToTrash(imageVm);
            });
            MoveFolderToTrashCommand = ReactiveCommand.Create((FolderViewModel folderVm) => {
                MoveFolderToTrash(folderVm);
            });
            ScanFolderImagesForMetaDataCommand = ReactiveCommand.Create(async (FolderViewModel folderVm) => {
                await ScanFolderImagesForMetaData(folderVm);
            });
            NextPageCommand = ReactiveCommand.Create(() => {
                NextPage();
            });
            PreviousPageCommand = ReactiveCommand.Create(() => {
                PreviousPage();
            });
            GoToPageCommand = ReactiveCommand.Create(async (decimal pageNumber) => {
                await GoToPage(Decimal.ToInt32(pageNumber));
            });
            ToggleSettingsCommand = ReactiveCommand.Create(() => {
                ToggleSettings();
            });
            ToggleManageImagesCommand = ReactiveCommand.Create(() => {
                ToggleManageImages();
            });
            ToggleFiltersCommand = ReactiveCommand.Create(() => {
                ToggleFilters();
            });
            ToggleCreateNewFolderCommand = ReactiveCommand.Create(() => {
                ToggleCreateNewFolder();
            });
            ToggleGetTotalImagesCommand = ReactiveCommand.Create(() => { 
                ToggleGetTotalImages();
            });
            ToggleImportAndScanCommand = ReactiveCommand.Create(() => {
                ToggleImportAndScan();
            });
            ToggleListAllTagsCommand = ReactiveCommand.Create(() => {
                ToggleListAllTags();
            });
            FilterImagesOnRatingCommand = ReactiveCommand.Create(async (decimal rating) => {
                ResetPagination();
                selectedRatingForFilter = Decimal.ToInt32(rating);
                currentFilter = filters.ImageRatingFilter;
                await RefreshImages();
            });
            FilterFoldersOnRatingCommand = ReactiveCommand.Create(async (decimal rating) => {
                ResetPagination();
                selectedRatingForFilter = Decimal.ToInt32(rating);
                currentFilter = filters.FolderRatingFilter;
                await RefreshFolders();
            });
            FilterImagesOnTagCommand = ReactiveCommand.Create(async (string tag) => {
                ResetPagination();
                tagForFilter = tag;
                currentFilter = filters.ImageTagFilter;
                await RefreshImages();
            });
            FilterFoldersOnTagCommand = ReactiveCommand.Create(async (string tag) => {
                ResetPagination();
                tagForFilter = tag;
                currentFilter = filters.FolderTagFilter;
                await RefreshFolders();
            });
            FilterFoldersOnDescriptionCommand = ReactiveCommand.Create(async (string text) => {
                ResetPagination();
                textForFilter = text;
                currentFilter = filters.FolderDescriptionFilter;
                await RefreshFolders();
            });
            GetAllFoldersWithNoImportedImagesCommand = ReactiveCommand.Create(async () => {
                ResetPagination();
                currentFilter = filters.AllFoldersWithNoImportedImages;
                await RefreshFolders();
            });
            GetAllFoldersWithMetadataNotScannedCommand = ReactiveCommand.Create(async () => {
                ResetPagination();
                currentFilter = filters.AllFoldersWithMetadataNotScanned;
                await RefreshFolders();
            });
            LoadCurrentDirectoryCommand = ReactiveCommand.Create(async () => {
                await LoadCurrentDirectory();
            });
            PickImageWidthCommand = ReactiveCommand.Create(async (string size) => {
                await PickImageWidth(size);
            });
            PickFolderPageSizeCommand = ReactiveCommand.Create(async (string size) => {
                await PickFolderPageSize(size);
            });
            PickImagePageSizeCommand = ReactiveCommand.Create(async (string size) => {
                await PickImagePageSize(size);
            });
            SaveDirectoryCommand = ReactiveCommand.Create(() => {
                SaveDirectory();
            });
            LoadSavedDirectoryCommand = ReactiveCommand.Create(async () => {
                await LoadSavedDirectory();
            });
            SaveFolderAsFavoriteCommand = ReactiveCommand.Create(async (FolderViewModel folderVm) => {
                await SaveFolderAsFavorite(folderVm);
            });
            GetAllFavoriteFoldersCommand = ReactiveCommand.Create(async () => {
                ResetPagination();
                currentFilter = filters.AllFavoriteFolders;
                await RefreshFolders();
            });
            RemoveAllFavoriteFoldersCommand = ReactiveCommand.Create(async () =>
            {
                await RemoveAllFavoriteFolders();
            });
            MoveSelectedImagesToTrashCommand = ReactiveCommand.Create(async (ItemsControl imagesItemsControl) =>
            {
                await MoveSelectedImagesToTrash(imagesItemsControl);
            });
            SelectAllImagesCommand = ReactiveCommand.Create((ItemsControl imagesItemsControl) => {
                SelectAllImages(imagesItemsControl);
            });
            MoveSelectedImagesToNewFolderCommand = ReactiveCommand.Create(async (ItemsControl imagesItemsControl) => { 
                await MoveSelectedImagesToNewFolder(imagesItemsControl);
            });
            ImportAllFoldersOnCurrentPageCommand = ReactiveCommand.Create(async (ItemsControl foldersItemsControl) => { 
                await ImportAllFoldersOnCurrentPage(foldersItemsControl);
            });
            ScanAllFoldersOnCurrentPageCommand = ReactiveCommand.Create(async (ItemsControl foldersItemsControl) => {
                await ScanAllFoldersOnCurrentPage(foldersItemsControl);
            });
            CreateNewFolderCommand = ReactiveCommand.Create(() => { CreateNewFolder(); });
            Initialize();
        }

        public int TotalImages
        {
            get => _totalImages;
            set => this.RaiseAndSetIfChanged(ref _totalImages, value);  
        }
        public bool ShowTotalImages
        {
            get => _showTotalImages;
            set => this.RaiseAndSetIfChanged(ref _showTotalImages, value);
        }

        public bool ShowAllTags
        {
            get => _showAllTags;
            set => this.RaiseAndSetIfChanged(ref _showAllTags, value);
        }

        public bool ShowImportAndScan
        {
            get => _showImportAndScan;
            set => this.RaiseAndSetIfChanged(ref _showImportAndScan, value);    
        }

        public bool ShowManageImages
        {
            get => _showManageImages;
            set => this.RaiseAndSetIfChanged(ref _showManageImages, value);
        }

        public bool ShowSettings
        {
            get => _showSettings;
            set => this.RaiseAndSetIfChanged(ref _showSettings, value);  
        }
        public bool ShowFilters
        {
            get => _showFilters;
            set => this.RaiseAndSetIfChanged(ref _showFilters, value);  
        }
        public int MaxCurrentPage
        {
            get => _maxCurrentPage;
            set => this.RaiseAndSetIfChanged(ref _maxCurrentPage, value);
        }
        public int MaxPage
        {
            get => _maxPage;
            set => this.RaiseAndSetIfChanged(ref _maxPage, value);  
        }
        public int TotalImagePages
        {
            get => _totalImagePages;
            set => this.RaiseAndSetIfChanged(ref _totalImagePages, value);
        }

        public int CurrentImagePage
        {
            get => _currentImagePage;
            set => this.RaiseAndSetIfChanged(ref _currentImagePage, value);
        }
        public int SavedTotalImagePages
        {
            get => _savedTotalImagePages;
            set => _savedTotalImagePages = value;
        }
        public int SavedImagePage
        {
            get => _savedImagePage;
            set => _savedImagePage = value;
        }

        public int ImagePageSize
        {
            get => _imagePageSize;
            set => this.RaiseAndSetIfChanged(ref _imagePageSize, value);
        }

        public int FolderPageSize
        {
            get => _folderPageSize; 
            set => this.RaiseAndSetIfChanged(ref _folderPageSize, value);
        }
        public int TotalFolderPages
        {
            get => _totalFolderPages;
            set => this.RaiseAndSetIfChanged(ref _totalFolderPages, value);
        }
        public int SavedTotalFolderPages
        {
            get => _savedTotalFolderPages;
            set => _savedTotalFolderPages = value;
        }
        public int SavedFolderPage
        {
            get => _savedFolderPage;
            set => _savedFolderPage = value;
        }
        public int CurrentFolderPage
        {
            get => _currentFolderPage;
            set => this.RaiseAndSetIfChanged(ref _currentFolderPage, value);
        }
        public List<string> TagsList
        {
            get => _tagsList;
            set => this.RaiseAndSetIfChanged(ref _tagsList, value); 
        }
        public bool ShowLoading
        {
            get => _showLoading;
            set => this.RaiseAndSetIfChanged(ref _showLoading, value);
        }

        public bool ShowCreateNewFolder 
        { 
            get => _showCreateNewFolder;
            set => this.RaiseAndSetIfChanged(ref _showCreateNewFolder, value);
        }


        public bool IsNewFolderEnabled
        {
            get => _isNewFolderEnabled;
            set => this.RaiseAndSetIfChanged(ref _isNewFolderEnabled, value);
        }

        public string NewFolderName
        {
            get => _newFolderName;
            set 
            {
                this.RaiseAndSetIfChanged(ref _newFolderName, value);
                if (value == "" || CurrentDirectory == RootFolderLocation || currentFilter != filters.None) 
                { 
                    IsNewFolderEnabled = false;
                }
                else
                {
                    IsNewFolderEnabled = true;
                }
            }
        }

        public string CurrentDirectory
        {
            get => _currentDirectory;
            set => this.RaiseAndSetIfChanged(ref _currentDirectory, value);
        }

        public string SelectedImagesNewDirectory
        {
            get => _selectedImagesNewDirectory;
            set => this.RaiseAndSetIfChanged(ref _selectedImagesNewDirectory, value);
        }
        public string SavedDirectory
        {
            get => _savedDirectory;
            set => _savedDirectory = value;
        }

        public bool FilterInCurrentDirectory
        {
            get => _filterInCurrentDirectory;
            set => this.RaiseAndSetIfChanged(ref _filterInCurrentDirectory, value);
        }

        public int MaxImageWidth
        {
            get => _maxImageWidth;
            set => this.RaiseAndSetIfChanged(ref _maxImageWidth, value);
        }

        public string RootFolderLocation
        {
            get => _rootFolderLocation;
            set => _rootFolderLocation = value;
        }

        //pass in this MainWindowViewModel so we can refresh UI
        public PickRootFolderViewModel PickRootFolder { get => new PickRootFolderViewModel(_unitOfWork, this); }

        public PickNewFoldersViewModel PickNewFolders { get => new PickNewFoldersViewModel(_unitOfWork, this); }

        public PickMoveToFolderViewModel PickMoveToFolder { get => new PickMoveToFolderViewModel(_unitOfWork, this); }

        public PickImageMoveToFolderViewModel PickImageMoveToFolder { get => new PickImageMoveToFolderViewModel(_unitOfWork, this); }

        public PickFolderCoverImageViewModel PickCoverImage { get => new PickFolderCoverImageViewModel(_unitOfWork, this); }

        public ObservableCollection<FolderViewModel> LibraryFolders { get; } = new ObservableCollection<FolderViewModel>();

        public ObservableCollection<ImageViewModel> Images { get; } = new ObservableCollection<ImageViewModel>();

        public ReactiveCommand<FolderViewModel, Unit> NextFolderCommand { get; }

        public ReactiveCommand<FolderViewModel,Unit> BackFolderCommand { get; }

        public ReactiveCommand<ImageViewModel, Unit> BackFolderFromImageCommand { get; }

        public ReactiveCommand<FolderViewModel, Task> ImportImagesCommand { get; }

        public ReactiveCommand<FolderViewModel, Unit> AddFolderDescriptionCommand { get; }

        public ReactiveCommand<FolderViewModel, Unit> AddFolderTagsCommand { get; }

        public ReactiveCommand<FolderViewModel, Unit> EditFolderTagsCommand { get; }

        public ReactiveCommand<FolderViewModel, Unit> AddFolderRatingCommand { get; }

        public ReactiveCommand<ImageViewModel, Unit> AddImageTagsCommand { get; }

        public ReactiveCommand<ImageViewModel, Unit> EditImageTagsCommand { get; }

        public ReactiveCommand<ImageViewModel, Unit> AddImageRatingCommand { get; }

        public ReactiveCommand<Unit, Unit> DeleteLibraryCommand { get; }

        public ReactiveCommand<ImageViewModel, Unit> OpenImageInExternalViewerCommand { get; }

        public ReactiveCommand<Unit, Unit> OpenCurrentDirectoryWithExplorerCommand { get; }

        public ReactiveCommand<ImageViewModel, Task> MoveImageToTrashCommand { get; }

        public ReactiveCommand<FolderViewModel, Unit> MoveFolderToTrashCommand { get; }

        public ReactiveCommand<FolderViewModel, Task> ScanFolderImagesForMetaDataCommand { get; }

        public ReactiveCommand<Unit, Unit> NextPageCommand { get; }

        public ReactiveCommand<Unit, Unit> PreviousPageCommand { get; }

        public ReactiveCommand<decimal, Task> GoToPageCommand { get; }

        public ReactiveCommand<Unit, Unit> ToggleSettingsCommand { get; }

        public ReactiveCommand<Unit, Unit> ToggleFiltersCommand { get; }

        public ReactiveCommand<Unit, Unit> ToggleManageImagesCommand { get; }

        public ReactiveCommand<Unit, Unit> ToggleCreateNewFolderCommand { get; }

        public ReactiveCommand<Unit, Unit> ToggleGetTotalImagesCommand { get; }

        public ReactiveCommand<Unit, Unit> ToggleImportAndScanCommand { get; }

        public ReactiveCommand<Unit, Unit> ToggleListAllTagsCommand { get; }

        public ReactiveCommand<decimal, Task> FilterImagesOnRatingCommand { get; }

        public ReactiveCommand<decimal, Task> FilterFoldersOnRatingCommand { get; }

        public ReactiveCommand<string, Task> FilterImagesOnTagCommand { get; }

        public ReactiveCommand<string, Task> FilterFoldersOnTagCommand  { get; }

        public ReactiveCommand<string, Task> FilterFoldersOnDescriptionCommand { get; }

        public ReactiveCommand<Unit, Task> GetAllFoldersWithNoImportedImagesCommand { get; }

        public ReactiveCommand<Unit, Task> GetAllFoldersWithMetadataNotScannedCommand { get; }
        public ReactiveCommand<Unit, Task> LoadCurrentDirectoryCommand { get; }

        public ReactiveCommand<string, Task> PickImageWidthCommand { get; }

        public ReactiveCommand<string, Task> PickFolderPageSizeCommand { get; }

        public ReactiveCommand<string, Task> PickImagePageSizeCommand { get; }

        public ReactiveCommand<Unit, Unit> SaveDirectoryCommand { get; }

        public ReactiveCommand<Unit, Task> LoadSavedDirectoryCommand { get; }

        public ReactiveCommand<FolderViewModel, Task> SaveFolderAsFavoriteCommand { get; }

        public ReactiveCommand<Unit, Task> GetAllFavoriteFoldersCommand {  get; } 

        public ReactiveCommand<Unit, Task> RemoveAllFavoriteFoldersCommand { get; }

        public ReactiveCommand<ItemsControl, Task> MoveSelectedImagesToTrashCommand { get; }

        public ReactiveCommand<ItemsControl, Unit> SelectAllImagesCommand { get; }

        public ReactiveCommand<ItemsControl, Task> MoveSelectedImagesToNewFolderCommand { get; }

        public ReactiveCommand<ItemsControl, Task> ImportAllFoldersOnCurrentPageCommand { get; }

        public ReactiveCommand<ItemsControl, Task> ScanAllFoldersOnCurrentPageCommand { get; }

        public ReactiveCommand<Unit, Unit> CreateNewFolderCommand { get; }

        private async Task SaveFolderAsFavorite(FolderViewModel folderVm)
        {
            await _folderMethods.SaveFolderToFavorites(folderVm.FolderId);
        }

        private async Task RemoveAllFavoriteFolders()
        {
            await _folderMethods.RemoveAllFavoriteFolders();
        }
        private void SaveDirectory()
        {
            SavedDirectory = CurrentDirectory;
            SavedFolderPage = CurrentFolderPage;
            SavedTotalFolderPages = TotalFolderPages;
            SavedImagePage = CurrentImagePage;
            SavedTotalImagePages = TotalImagePages;
        }

        private async Task LoadSavedDirectory()
        {
            CurrentDirectory = SavedDirectory;
            CurrentFolderPage = SavedFolderPage;
            TotalFolderPages = SavedTotalFolderPages;
            CurrentImagePage = SavedImagePage;
            TotalImagePages = SavedTotalImagePages;
            MaxPage = Math.Max(TotalImagePages, TotalFolderPages);
            MaxCurrentPage = Math.Max(CurrentImagePage, CurrentFolderPage);
            await LoadCurrentDirectory();
        }
        private async Task UpdateSettings()
        {
            //update database
            Settings settings = new()
            {
                SettingsId = 1,
                MaxImageWidth = MaxImageWidth,
                FolderPageSize = FolderPageSize,
                ImagePageSize = ImagePageSize,
            };
            await _settingsMethods.UpdateSettings(settings);
        }

        private async Task PickImagePageSize(string size)
        {
            switch (size)
            {
                case "20":
                    ImagePageSize = 20;
                    break;
                case "40":
                    ImagePageSize = 40;
                    break;
                case "60":
                    ImagePageSize = 60;
                    break;
                case "80":
                    ImagePageSize = 80;
                    break;
                case "100":
                    ImagePageSize = 100;
                    break;
                case "125":
                    ImagePageSize = 125;
                    break;
                case "150":
                    ImagePageSize = 150;
                    break;
                case "175":
                    ImagePageSize = 175;
                    break;
                case "200":
                    ImagePageSize = 200;
                    break;
            }
            await UpdateSettings();
            ResetPagination();
            await RefreshFolders();
            await RefreshImages(CurrentDirectory);
        }
        private async Task PickFolderPageSize(string size)
        {
            switch (size) 
            {
                case "20":
                    FolderPageSize = 20;
                    break;
                case "40":
                    FolderPageSize = 40;
                    break;
                case "60":
                    FolderPageSize = 60;
                    break;
                case "80":
                    FolderPageSize = 80;
                    break;
                case "100":
                    FolderPageSize = 100;
                    break;
            }
            await UpdateSettings();
            ResetPagination();
            await RefreshFolders();
            await RefreshImages(CurrentDirectory);
        }
        private async Task PickImageWidth(string size)
        {           
            switch (size)
            {
                case "Small":
                    MaxImageWidth = 300;
                    break;
                case "Medium":
                    MaxImageWidth = 400;
                    break;
                case "Large":
                    MaxImageWidth = 500;
                    break;
                case "XLarge":
                    MaxImageWidth = 550;
                    break;
                case "XXLarge":
                    MaxImageWidth = 600;
                    break;
            }
            await UpdateSettings();
        }
        private async Task LoadCurrentDirectory()
        {
            currentFilter = filters.None;
            await RefreshFolders(CurrentDirectory);
            await RefreshImages(CurrentDirectory);
        }

        private void ToggleListAllTags()
        {
            if (ShowAllTags)
            {
                ShowAllTags = false;
            }
            else
            {
                ShowAllTags = true;
            }
        }
        private void ToggleImportAndScan()
        {
            if (ShowImportAndScan) 
            { 
                ShowImportAndScan = false;
            }
            else
            {
                ShowImportAndScan = true;
            }
        }
        private async void ToggleGetTotalImages()
        {
            if (ShowTotalImages)
            {
                ShowTotalImages = false;
            }
            else
            {
                ShowTotalImages = true;
                TotalImages = await _imageMethods.GetTotalImages();
            }
        }
        private void ToggleManageImages()
        {
            if (ShowManageImages)
            {
                ShowManageImages = false;
            }
            else
            {
                ShowManageImages = true;
            }
        }
        private void ToggleSettings()
        {
            if (ShowSettings)
            {
                ShowSettings = false;
            }
            else
            {
                ShowSettings = true;
            }
        }
        private void ToggleFilters()
        {
            if (ShowFilters)
            {
                ShowFilters = false;
            }
            else
            {
                ShowFilters = true;
            }
        }

        private void ToggleCreateNewFolder()
        {
            if (ShowCreateNewFolder)
            {
                ShowCreateNewFolder = false;
            }
            else
            {
                ShowCreateNewFolder = true;
            }
        }
        private List<Image> ImagePagination()
        {
            //same as FolderPagination
            int offset = ImagePageSize * (CurrentImagePage -1);
            int totalImageCount = displayImages.Count;
            if(totalImageCount == 0 || totalImageCount <= ImagePageSize)
                return displayImages;
            TotalImagePages = (int)Math.Ceiling(totalImageCount / (double)ImagePageSize);
            List<Image> displayImagesTemp;
            if(CurrentImagePage == TotalImagePages)
            {
                displayImagesTemp = displayImages.GetRange(offset, (totalImageCount - (TotalImagePages - 1)*ImagePageSize));
            }
            else
            {
                displayImagesTemp = displayImages.GetRange(offset, ImagePageSize);
            }
            MaxPage = Math.Max(TotalImagePages, TotalFolderPages);
            MaxCurrentPage = Math.Max(CurrentImagePage, CurrentFolderPage);
            return displayImagesTemp;
        }
        private List<Folder> FolderPagination()
        {
            /* Example
             * FolderPageSize = 10
             * offest = 10*1 for page = 2
             * totalFolderCount = 14
             * TotalFolderPages = 2
             */
            int offest = FolderPageSize * (CurrentFolderPage - 1);
            int totalFolderCount = displayFolders.Count;
            if (totalFolderCount == 0 || totalFolderCount <= FolderPageSize) 
                return displayFolders; 
            TotalFolderPages = (int)Math.Ceiling(totalFolderCount / (double)FolderPageSize);
            List<Folder> displayFoldersTemp;
            if (CurrentFolderPage == TotalFolderPages)
            {
                //on last page GetRange count CANNOT be FolderPageSize or index out of range 
                //thus following logical example above in a array of 14 elements the range count on the last page is 14 - 10
                //formul used: totalFolderCount - ((TotalFolderPages - 1)*FolderPageSize)
                //folderCount minus total folders on all but last page
                //14 - 10
                displayFoldersTemp = displayFolders.GetRange(offest, (totalFolderCount - (TotalFolderPages - 1)*FolderPageSize));
            }
            else
            {
                displayFoldersTemp = displayFolders.GetRange(offest, FolderPageSize);
            }
            MaxPage = Math.Max(TotalImagePages, TotalFolderPages);
            MaxCurrentPage = Math.Max(CurrentImagePage, CurrentFolderPage);
            return displayFoldersTemp;
        }

        private void ResetPagination()
        {
            CurrentFolderPage = 1;
            TotalFolderPages = 1;
            CurrentImagePage = 1;
            TotalImagePages = 1;
            MaxCurrentPage = 1;
            MaxPage = 1;
        }

        private async Task MapTagsToFoldersAddToObservable()
        {
            try
            {
                for (int i = 0; i < displayFolders.Count; i++)
                {
                    //need to map tags to folders 
                    displayFolders[i] = FolderMapper.MapTagsToFolder(displayFolders[i], displayFolderTags);
                    FolderViewModel folderViewModel = await FolderMapper.GetFolderVm(displayFolders[i]);
                    LibraryFolders.Add(folderViewModel);
                }
            }
            catch (Exception ex) 
            {
                var box = MessageBoxManager.GetMessageBoxStandard("Error", $"Something went wrong click ok to reload current directory. {ex}", ButtonEnum.Ok);
                await box.ShowAsync();
                await LoadCurrentDirectory();
            }
            
        }

        //public so we can call from other view models
        public async Task RefreshFolders(string path = "")
        {
            ShowLoading = true;
            switch (currentFilter)
            {
                case filters.None:
                    (List<Folder> folders, List<FolderTag> tags) folderResult;
                    if (String.IsNullOrEmpty(path))
                    {
                        folderResult = await _folderMethods.GetFoldersInDirectory(CurrentDirectory);
                    }
                    else
                    {
                        folderResult = await _folderMethods.GetFoldersInDirectory(path);
                    }
                    displayFolders = folderResult.folders;
                    displayFolderTags = folderResult.tags;
                    LibraryFolders.Clear();
                    displayFolders = FolderPagination();
                    await MapTagsToFoldersAddToObservable();
                    break;
                case filters.FolderRatingFilter:
                    (List<Folder> folders, List<FolderTag> tags) folderRatingResult = await _folderMethods.GetAllFoldersAtRating(selectedRatingForFilter, FilterInCurrentDirectory, CurrentDirectory);
                    displayFolders = folderRatingResult.folders;
                    displayFolderTags = folderRatingResult.tags;

                    Images.Clear();
                    LibraryFolders.Clear();
                    displayFolders = FolderPagination();
                    await MapTagsToFoldersAddToObservable();
                    break;
                case filters.FolderTagFilter:
                    (List<Folder> folders, List<FolderTag> tags) folderTagResult = await _folderMethods.GetAllFoldersWithTag(tagForFilter, FilterInCurrentDirectory, CurrentDirectory);
                    displayFolders = folderTagResult.folders;
                    displayFolderTags = folderTagResult.tags;

                    Images.Clear();
                    LibraryFolders.Clear();
                    displayFolders = FolderPagination();
                    await MapTagsToFoldersAddToObservable();
                    break;
                case filters.FolderDescriptionFilter:
                    (List<Folder> folders, List<FolderTag> tags) folderDescriptionResult = await _folderMethods.GetAllFoldersWithDescriptionText(textForFilter, FilterInCurrentDirectory, CurrentDirectory);
                    displayFolders = folderDescriptionResult.folders;
                    displayFolderTags = folderDescriptionResult.tags;

                    Images.Clear();
                    LibraryFolders.Clear();
                    displayFolders = FolderPagination();
                    await MapTagsToFoldersAddToObservable();
                    break;
                case filters.AllFavoriteFolders:
                    (List<Folder> folders, List<FolderTag> tags) allFavoriteFoldersResult = await _folderMethods.GetAllFavoriteFolders();
                    displayFolders = allFavoriteFoldersResult.folders;
                    displayFolderTags = allFavoriteFoldersResult.tags;

                    Images.Clear();
                    LibraryFolders.Clear();
                    displayFolders = FolderPagination();
                    await MapTagsToFoldersAddToObservable();
                    break;
                case filters.AllFoldersWithNoImportedImages:
                    (List<Folder> folders, List<FolderTag> tags) allFoldersWithNoImportedImagesResult = await _folderMethods.GetAllFoldersWithNoImportedImages(FilterInCurrentDirectory, CurrentDirectory);
                    displayFolders = allFoldersWithNoImportedImagesResult.folders;
                    displayFolderTags = allFoldersWithNoImportedImagesResult.tags;

                    Images.Clear();
                    LibraryFolders.Clear();
                    displayFolders = FolderPagination();
                    await MapTagsToFoldersAddToObservable();
                    break;
                case filters.AllFoldersWithMetadataNotScanned:
                    (List<Folder> folders, List<FolderTag> tags) allFoldersWithMetadataNotScannedResult = await _folderMethods.GetAllFoldersWithMetadataNotScanned(FilterInCurrentDirectory, CurrentDirectory);
                    displayFolders = allFoldersWithMetadataNotScannedResult.folders;
                    displayFolderTags = allFoldersWithMetadataNotScannedResult.tags;

                    Images.Clear();
                    LibraryFolders.Clear();
                    displayFolders = FolderPagination();
                    await MapTagsToFoldersAddToObservable();
                    break;
            }
            ShowLoading = false;
        }
        private async Task MapTagsToSingleFolderUpdateObservable(FolderViewModel folderVm)
        {
            try
            {
                for (int i = 0; i < displayFolders.Count; i++)
                {
                    //only map the one that is being updated
                    if (displayFolders[i].FolderId == folderVm.FolderId)
                    {
                        //need to map tags to folders 
                        displayFolders[i] = FolderMapper.MapTagsToFolder(displayFolders[i], displayFolderTags);
                        FolderViewModel folderViewModel = await FolderMapper.GetFolderVm(displayFolders[i]);
                        //will be in the same order unless delete/move or next back folder
                        //Any non destructive operation that does not affect the number or order of items returned from
                        //the sql query will be in the same order so just modify props for a much cleaner UI refresh
                        LibraryFolders[i] = folderViewModel;
                        return;
                    }
                }
            }
            catch (Exception ex)
            {
                var box = MessageBoxManager.GetMessageBoxStandard("Error", $"Something went wrong click ok to reload current directory. {ex}", ButtonEnum.Ok);
                await box.ShowAsync();
                await LoadCurrentDirectory();
            }
        }

        //public so we can call from other view models
        public async Task RefreshFolderProps(string path, FolderViewModel folderVm)
        {
            ShowLoading = true;
            switch (currentFilter)
            {
                case filters.None:
                    (List<Folder> folders, List<FolderTag> tags) folderResult = await _folderMethods.GetFoldersInDirectory(path);
                    displayFolders = folderResult.folders;
                    displayFolderTags = folderResult.tags;
                    displayFolders = FolderPagination();
                    await MapTagsToSingleFolderUpdateObservable(folderVm);
                    break;
                case filters.FolderRatingFilter:
                    (List<Folder> folders, List<FolderTag> tags) folderRatingResult = await _folderMethods.GetAllFoldersAtRating(selectedRatingForFilter, FilterInCurrentDirectory, CurrentDirectory);
                    displayFolders = folderRatingResult.folders;
                    displayFolderTags = folderRatingResult.tags;
                    displayFolders = FolderPagination();
                    await MapTagsToSingleFolderUpdateObservable(folderVm);
                    break;
                case filters.FolderTagFilter:
                    (List<Folder> folders, List<FolderTag> tags) folderTagResult = await _folderMethods.GetAllFoldersWithTag(tagForFilter, FilterInCurrentDirectory, CurrentDirectory);
                    displayFolders = folderTagResult.folders;
                    displayFolderTags = folderTagResult.tags;
                    displayFolders = FolderPagination();
                    await MapTagsToSingleFolderUpdateObservable(folderVm);
                    break;
                case filters.FolderDescriptionFilter:
                    (List<Folder> folders, List<FolderTag> tags) folderDescriptionResult = await _folderMethods.GetAllFoldersWithDescriptionText(textForFilter, FilterInCurrentDirectory, CurrentDirectory);
                    displayFolders = folderDescriptionResult.folders;
                    displayFolderTags = folderDescriptionResult.tags;
                    displayFolders = FolderPagination();
                    await MapTagsToSingleFolderUpdateObservable(folderVm);
                    break;
                case filters.AllFavoriteFolders:
                    (List<Folder> folders, List<FolderTag> tags) allFavoriteFoldersResult = await _folderMethods.GetAllFavoriteFolders();
                    displayFolders = allFavoriteFoldersResult.folders;
                    displayFolderTags = allFavoriteFoldersResult.tags;
                    displayFolders = FolderPagination();
                    await MapTagsToSingleFolderUpdateObservable(folderVm);
                    break;
                case filters.AllFoldersWithNoImportedImages:
                    (List<Folder> folders, List<FolderTag> tags) allFoldersWithNoImportedImagesResult = await _folderMethods.GetAllFoldersWithNoImportedImages(FilterInCurrentDirectory, CurrentDirectory);
                    displayFolders = allFoldersWithNoImportedImagesResult.folders;
                    displayFolderTags = allFoldersWithNoImportedImagesResult.tags;
                    displayFolders = FolderPagination();
                    await MapTagsToSingleFolderUpdateObservable(folderVm);
                    break;
                case filters.AllFoldersWithMetadataNotScanned:
                    (List<Folder> folders, List<FolderTag> tags) allFoldersWithMetadataNotScannedResult = await _folderMethods.GetAllFoldersWithMetadataNotScanned(FilterInCurrentDirectory, CurrentDirectory);
                    displayFolders = allFoldersWithMetadataNotScannedResult.folders;
                    displayFolderTags = allFoldersWithMetadataNotScannedResult.tags;
                    displayFolders = FolderPagination();
                    await MapTagsToSingleFolderUpdateObservable(folderVm);
                    break;
            }
            ShowLoading = false;
        }

        private async Task MapTagsToImagesAddToObservable()
        {
            try
            {
                for (int i = 0; i < displayImages.Count; i++)
                {
                    //need to map tags to images
                    displayImages[i] = ImageMapper.MapTagsToImage(displayImages[i], displayImageTags);
                    ImageViewModel imageViewModel = await ImageMapper.GetImageVm(displayImages[i]);
                    Images.Add(imageViewModel);
                }
            }
            catch (Exception ex) 
            {
                var box = MessageBoxManager.GetMessageBoxStandard("Error", $"Something went wrong click ok to reload current directory. {ex}", ButtonEnum.Ok);
                await box.ShowAsync();
                await LoadCurrentDirectory();
            }
        }
        private async Task RefreshImages(string path = "", int folderId = 0)
        {
            ShowLoading = true;
            switch (currentFilter)
            {
                case filters.None:
                    (List<Image> images, List<ImageTag> tags) imageResult;
                    if (string.IsNullOrEmpty(path))
                    {
                        imageResult = await _imageMethods.GetAllImagesInFolder(folderId);
                    }
                    else
                    {
                        imageResult = await _imageMethods.GetAllImagesInFolder(path);
                    }
                    displayImages = imageResult.images;
                    displayImageTags = imageResult.tags;

                    Images.Clear();
                    displayImages = ImagePagination();
                    await MapTagsToImagesAddToObservable();
                    break;
                case filters.ImageRatingFilter:
                    (List<Image> images, List<ImageTag> tags) imageRatingResult = await _imageMethods.GetAllImagesAtRating(selectedRatingForFilter, FilterInCurrentDirectory, CurrentDirectory);
                    displayImages = imageRatingResult.images;
                    displayImageTags = imageRatingResult.tags;

                    Images.Clear();
                    LibraryFolders.Clear();
                    displayImages = ImagePagination();
                    await MapTagsToImagesAddToObservable();
                    break;
                case filters.ImageTagFilter:
                    (List<Image> images, List<ImageTag> tags) imageTagResult = await _imageMethods.GetAllImagesWithTag(tagForFilter, FilterInCurrentDirectory, CurrentDirectory);
                    displayImages = imageTagResult.images;
                    displayImageTags = imageTagResult.tags;

                    Images.Clear();
                    LibraryFolders.Clear();
                    displayImages = ImagePagination();
                    await MapTagsToImagesAddToObservable();
                    break;
            }
            ShowLoading = false;
        }
        private async Task MapTagsToSingleImageUpdateObservable(ImageViewModel imageVm)
        {
            try
            {
                for (int i = 0; i < displayImages.Count; i++)
                {
                    //only map the one that is being updated
                    if (displayImages[i].ImageId == imageVm.ImageId)
                    {
                        //need to map tags to image
                        displayImages[i] = ImageMapper.MapTagsToImage(displayImages[i], displayImageTags);
                        ImageViewModel imageViewModel = await ImageMapper.GetImageVm(displayImages[i]);
                        Images[i] = imageViewModel;
                        return;
                    }
                }
            }
            catch (Exception ex)
            {
                var box = MessageBoxManager.GetMessageBoxStandard("Error", $"Something went wrong click ok to reload current directory. {ex}", ButtonEnum.Ok);
                await box.ShowAsync();
                await LoadCurrentDirectory();
            }
        }
        private async Task RefreshImageProps(ImageViewModel imageVm, string path = "", int folderId = 0)
        {
            ShowLoading = true;
            switch (currentFilter)
            {
                case filters.None:
                    (List<Image> images, List<ImageTag> tags) imageResult;
                    if (string.IsNullOrEmpty(path))
                    {
                        imageResult = await _imageMethods.GetAllImagesInFolder(folderId);
                    }
                    else
                    {
                        imageResult = await _imageMethods.GetAllImagesInFolder(path);
                    }
                    displayImages = imageResult.images;
                    displayImageTags = imageResult.tags;
                    displayImages = ImagePagination();
                    await MapTagsToSingleImageUpdateObservable(imageVm);
                    break;
                case filters.ImageRatingFilter:
                    (List<Image> images, List<ImageTag> tags) imageRatingResult = await _imageMethods.GetAllImagesAtRating(selectedRatingForFilter, FilterInCurrentDirectory, CurrentDirectory);
                    displayImages = imageRatingResult.images;
                    displayImageTags = imageRatingResult.tags;
                    displayImages = ImagePagination();
                    await MapTagsToSingleImageUpdateObservable(imageVm);
                    break;
                case filters.ImageTagFilter:
                    (List<Image> images, List<ImageTag> tags) imageTagResult = await _imageMethods.GetAllImagesWithTag(tagForFilter, FilterInCurrentDirectory, CurrentDirectory);
                    displayImages = imageTagResult.images;
                    displayImageTags = imageTagResult.tags;
                    displayImages = ImagePagination();
                    await MapTagsToSingleImageUpdateObservable(imageVm);
                    break;
            }
            ShowLoading = false;
        }
        private async void Initialize()
        {
            await GetRootFolder();
            await GetTagsList();
            await GetSettings();
            //initially set SavedDirectory to CurrentDirectory so method wont fail if btn clicked before saving a directory
            SavedDirectory = CurrentDirectory;
        }

        private async Task GetSettings()
        {
            Settings settings = await _settingsMethods.GetSettings();
            MaxImageWidth = settings.MaxImageWidth;
            FolderPageSize = settings.FolderPageSize;
            ImagePageSize = settings.ImagePageSize;
        }
        private async Task GetRootFolder()
        {
            Folder? rootFolder = await _folderMethods.GetRootFolder();
            if (rootFolder != null) 
            {
                FolderViewModel rootFolderVm = await FolderMapper.GetFolderVm(rootFolder);
                LibraryFolders.Add(rootFolderVm);
                RootFolderLocation = PathHelper.RemoveOneFolderFromPath(rootFolder.FolderPath);
                CurrentDirectory = RootFolderLocation;
            }
        }

        //should technically have its own repo but only plan on having only this one method just keeping it in images repo.
        private async Task GetTagsList()
        {
            TagsList = await _imageMethods.GetTagsList();
        }

        private async Task ImportImages(FolderViewModel imageFolder)
        {
            string newPath = string.Empty;
            string imageFolderPath = imageFolder.FolderPath;
            int imageFolderId = imageFolder.FolderId;
            ShowLoading = true;
            //build csv
            bool csvIsSet = await ImageCsvMethods.BuildImageCsv(imageFolderPath, imageFolderId);
            //write csv to database and load folders and images at the location again
            //load again so the import button will go away
            if (csvIsSet) 
            {
                await _imageCsvMethods.AddImageCsv(imageFolderId);
                //remove one folder from path
                newPath = PathHelper.RemoveOneFolderFromPath(imageFolderPath);
                //refresh UI
                if(currentFilter == filters.AllFoldersWithMetadataNotScanned || currentFilter == filters.AllFoldersWithNoImportedImages)
                {
                    //have to call hard refresh for these two cases as they will not be returned from the query to update props
                    await RefreshFolders();
                }
                else
                {
                    await RefreshFolderProps(newPath, imageFolder);
                }
            }
            ShowLoading = false;
        }

        //opens the previous directory location -- from image button
        private async void BackFolderFromImage(ImageViewModel imageVm)
        {
            //not ideal but keeps pagination to the folder your in. When you go back or next start from page 1
            ResetPagination();
            /*
                Similar to Back folders except these buttons are on the image and we only need to remove one folder
                Not every folder has a folder so this is the quickest way for now to back out of a folder that only has images
             */
            string newPath = PathHelper.RemoveOneFolderFromPath(imageVm.ImageFolderPath);
            //set the current directory -- used to add new folder to location
            CurrentDirectory = newPath;
            //refresh UI
            currentFilter = filters.None;
            await RefreshFolders();
            await RefreshImages(newPath);
        }

        //opens the previous directory location
        private async void BackFolder(FolderViewModel currentFolder)
        {
            ResetPagination();
            /*
                tough to see but basically you need to remove two folders to build the regexp string
                example if you are in /pictures/hiking/bearmountian and bearmountain folder has another folder saturday_2025_05_25
                you will be clicking on the back button of folder /pictures/hiking/bearmountian/saturday_2025_05_25 -- that wil be the FolderPath
                but you want to go back to hiking so you must remove two folders to get /pictures/hiking/
             */
            string newPath = PathHelper.RemoveTwoFoldersFromPath(currentFolder.FolderPath);
            //set the current directory -- used to add new folder to location
            CurrentDirectory = newPath;
            //refresh UI
            currentFilter = filters.None;
            await RefreshFolders();
            await RefreshImages(newPath);
        }

        //loads the previous X elements in CurrentDirectory
        private async void PreviousPage()
        {
            if (CurrentFolderPage > 1)
            {
                CurrentFolderPage = CurrentFolderPage - 1;
                await RefreshFolders();
            }
            if (CurrentImagePage > 1)
            {
                CurrentImagePage = CurrentImagePage - 1;
                await RefreshImages(CurrentDirectory);
            }
        }

        //opens the next directory locaion
        private async void NextFolder(FolderViewModel currentFolder)
        {
            ResetPagination();
            bool hasChildren = currentFolder.HasChildren;
            bool hasFiles = currentFolder.HasFiles;
            //set the current directory -- used to add new folder to location
            CurrentDirectory = currentFolder.FolderPath;
            //two boolean varibale 4 combos TF TT FT and FF
            if(hasChildren == false && hasFiles == false)
            {
                var box = MessageBoxManager.GetMessageBoxStandard("Empty Folder", "There are no Images in this folder.", ButtonEnum.Ok);
                await box.ShowAsync();
                return;
            }
            else
            {
                //refresh UI
                currentFilter = filters.None;
                await RefreshFolders();
                await RefreshImages("", currentFolder.FolderId);
            }  
        }

        //loads the next X elements in CurrentDirectory
        private async void NextPage()
        {
            if (CurrentFolderPage < TotalFolderPages)
            {
                CurrentFolderPage = CurrentFolderPage + 1;
                await RefreshFolders();
            }
            if (CurrentImagePage < TotalImagePages)
            {
                CurrentImagePage = CurrentImagePage + 1;
                await RefreshImages(CurrentDirectory);
            }
        }

        private async Task GoToPage(int pageNumber)
        {
            if (pageNumber <= TotalFolderPages)
            {
                CurrentFolderPage = pageNumber;
                await RefreshFolders();
            }
            if (pageNumber <= TotalImagePages)
            {
                CurrentImagePage = pageNumber;
                await RefreshImages(CurrentDirectory);
            }
        }
        private async void DeleteLibrary()
        {
            var box = MessageBoxManager.GetMessageBoxStandard("Delete Library", "Are you sure you want to delete your library? The images on the file system will remain.", ButtonEnum.YesNo);
            var result = await box.ShowAsync();

            if (result == ButtonResult.Yes)
            {
                //remove all folders -- this will drop images as well. 
                bool success = await _folderMethods.DeleteAllFolders();
                if (success) 
                {
                    //refresh UI
                    LibraryFolders.Clear();
                    Images.Clear();
                }
            }
            else 
            {
                return;
            }
        }

        private async void UpdateFolder(FolderViewModel folderVm, string fieldUpdated)
        {
            Folder folder = FolderMapper.GetFolderFromVm(folderVm);
            bool success = await _folderMethods.UpdateFolder(folder);
            if (!success)
            {
                var box = MessageBoxManager.GetMessageBoxStandard($"Add {fieldUpdated}", $"Folder {fieldUpdated} update error. Try again.", ButtonEnum.Ok);
                await box.ShowAsync();
                return;
            }
        }

        private async void EditFolderTag(FolderViewModel folderVm)
        {
            if(folderVm.FolderTags == null || folderVm.FolderTags == "") 
            {
                if(folderVm.Tags.Count == 1)
                {
                    await _folderMethods.DeleteFolderTag(folderVm.Tags[0]);
                }
                else if (folderVm.Tags.Count == 0)
                {
                    return;
                }
            }
            List<string> folderTags = folderVm.FolderTags.Split(",").ToList();
            FolderTag? tagToRemove = null;
            foreach(FolderTag tag in folderVm.Tags)
            {
                if (!folderTags.Contains(tag.TagName))
                {
                    tagToRemove = tag;
                }
            }
            if (tagToRemove != null) 
            {
                await _folderMethods.DeleteFolderTag(tagToRemove);
            }
        }
        private async void AddFolderTag(FolderViewModel folderVm)
        {
            //click submit with empty input just return
            if (folderVm.NewTag == "" || folderVm.NewTag == null)
            {
                return;
            }
            Folder folder = FolderMapper.GetFolderFromVm(folderVm);
            //update folder table and tags table in db -- success will be false if you try to input a duplicate tag
            bool success = await _folderMethods.UpdateFolderTags(folder, folderVm.NewTag);
            if (success)
            {
                //Update TagsList to show in UI AutoCompleteBox clear NewTag in box as well and refresh folders to show new tag
                await GetTagsList();
                folderVm.NewTag = "";
                //refresh UI
                await RefreshFolderProps(CurrentDirectory, folderVm);
            }
            else
            {
                folderVm.NewTag = "";
            }
        }

        //update image sql and metadata only. 
        private async void UpdateImage(ImageViewModel imageVm, string fieldUpdated)
        {
            Image image = ImageMapper.GetImageFromVm(imageVm);
            bool success = await _imageMethods.UpdateImage(image);
            if (!success)
            {
                var box = MessageBoxManager.GetMessageBoxStandard($"Add {fieldUpdated}", $"Image {fieldUpdated} update error. Try again.", ButtonEnum.Ok);
                await box.ShowAsync();
                return;     
            }
            //write rating to image metadata
            if (fieldUpdated == "Rating")
            {
                ImageMetaDataHelper.AddRatingToImage(image);
            }
           
        }

        //remove the tag from the image_tag_join table 
        //Also need to remove imageMetaData
        private async void EditImageTag(ImageViewModel imageVm)
        {
            if(imageVm.ImageTags == null || imageVm.ImageTags == "")
            {
                if(imageVm.Tags.Count == 1)
                {
                    await _imageMethods.DeleteImageTag(imageVm.Tags[0]);
                    //remove tag from image metadata
                    await ImageMetaDataHelper.WriteTagToImage(imageVm);
                }
                else if(imageVm.Tags.Count == 0)
                {
                    return;
                }
            }
            List<string> imageTags = imageVm.ImageTags.Split(",").ToList();
            ImageTag tagToRemove = null;
            foreach(ImageTag tag in imageVm.Tags)
            {
                if (!imageTags.Contains(tag.TagName))
                {
                    tagToRemove = tag;
                }
            }
            if (tagToRemove != null) 
            { 
                await _imageMethods.DeleteImageTag(tagToRemove);
                //remove tag from image metadata
                await ImageMetaDataHelper.WriteTagToImage(imageVm);
            }
        }
        //update ImageTags in db, and update image metadata
        private async void AddImageTag(ImageViewModel imageVm)
        {
            //click submit with empty input just return
            if(imageVm.NewTag == "" || imageVm.NewTag == null)
            {
                return;
            }
            //add NewTag to ImageTags -- KEEP!! THIS IS NEEDED TO WRITE METADATA
            if (string.IsNullOrEmpty(imageVm.ImageTags))
            {
                imageVm.ImageTags = imageVm.NewTag;
            }
            else
            {
                imageVm.ImageTags = imageVm.ImageTags + "," + imageVm.NewTag;
            }
            Image image = ImageMapper.GetImageFromVm(imageVm);
            //update image table and tags table in db -- success will be false if you try to input a duplicate tag
            bool success = await _imageMethods.UpdateImageTags(image, imageVm.NewTag);
            if (success) 
            {
                //write new tag to image metadata
                await ImageMetaDataHelper.WriteTagToImage(imageVm);
                //Update TagsList to show in UI AutoCompleteBox clear NewTag in box as well
                await GetTagsList();
                imageVm.NewTag = "";
                await RefreshImageProps(imageVm, CurrentDirectory);   
            }
            else
            {
                //remove the NewTag from the Tags list in the UI (New tag was duplicate and not added in this case)
                int tagsMaxIndex = imageVm.ImageTags.Length - 1;
                int newTagTotalCharsToRemove = imageVm.NewTag.Length; //total chars to remove
                int removeStartAtIndex = tagsMaxIndex - newTagTotalCharsToRemove;
                imageVm.ImageTags = imageVm.ImageTags.Remove(removeStartAtIndex);
                //clear NewTag in box if try to input duplicate tag
                imageVm.NewTag = "";
            }
        }
       
        private async void OpenImageInExternalViewer(ImageViewModel imageVm)
        {
            string externalImageViewerExePath = PathHelper.GetExternalImageViewerExePath();
            string imagePathForProcessStart = PathHelper.FormatImageFilePathForProcessStart(imageVm.ImagePath);
            if (File.Exists(imageVm.ImagePath) && File.Exists(externalImageViewerExePath)) 
            {
                Process.Start(externalImageViewerExePath, imagePathForProcessStart);
            }
            else
            {
                var box = MessageBoxManager.GetMessageBoxStandard("Open Image", "You need to install nomacs.", ButtonEnum.Ok);
                await box.ShowAsync();
                return;
            }
        }

        private async void OpenCurrentDirectoryWithExplorer()
        {
            string externalFileExplorerExePath = PathHelper.GetExternalFileExplorerExePath();
            string folderPathForProcessStart = PathHelper.FormatImageFilePathForProcessStart(CurrentDirectory); //not an image path but all this did was wrap it in quotes
            if (File.Exists(externalFileExplorerExePath) && Directory.Exists(CurrentDirectory))
            {
                Process.Start(externalFileExplorerExePath, folderPathForProcessStart);
            }
            else
            {
                var box = MessageBoxManager.GetMessageBoxStandard("Open Folder", "Sorry something went wrong.", ButtonEnum.Ok);
                await box.ShowAsync();
                return;
            }
        }

        private async Task MoveSelectedImagesToNewFolder(ItemsControl imagesItemsControl)
        {
            List<ImageViewModel> allImages = imagesItemsControl.Items.OfType<ImageViewModel>().ToList();
            List<ImageViewModel> imagesToMove = new List<ImageViewModel>();
            Folder imagesCurrentFolder = await _folderMethods.GetFolderAtDirectory(allImages[0].ImageFolderPath);

            if (SelectedImagesNewDirectory == null || SelectedImagesNewDirectory == "")
            {
                var box = MessageBoxManager.GetMessageBoxStandard("Move Images", "You need to select the move to folder first.", ButtonEnum.Ok);
                await box.ShowAsync();
                return;
            }
            //get folder at SelectedImagesNewDirectory
            Folder imagesNewFolder = await _folderMethods.GetFolderAtDirectory(SelectedImagesNewDirectory);
            if(imagesNewFolder.FolderPath == imagesCurrentFolder.FolderPath)
            {
                var box = MessageBoxManager.GetMessageBoxStandard("Move Images", "New folder path cannot be the same as the current folder path.", ButtonEnum.Ok);
                await box.ShowAsync();
                return;
            }
            //prevent a double import and only allow move to folders that are already imported
            if(imagesNewFolder.HasFiles == true && imagesNewFolder.AreImagesImported == false)
            {
                var box = MessageBoxManager.GetMessageBoxStandard("Move Images", "The move to folder has to have its current images imported first.", ButtonEnum.Ok);
                await box.ShowAsync();
                return;
            }
           
            foreach (ImageViewModel image in allImages) 
            {
                if (image.IsSelected && File.Exists(image.ImagePath))
                {
                    imagesToMove.Add(image);
                }
            }
            if(imagesToMove.Count == 0)
            {
                var box = MessageBoxManager.GetMessageBoxStandard("Move Images", "You need to select images to move.", ButtonEnum.Ok);
                await box.ShowAsync();
                return;
            }
            var boxYesNo = MessageBoxManager.GetMessageBoxStandard("Move Images", $"Are you sure you want to move these images to {SelectedImagesNewDirectory}?", ButtonEnum.YesNo);
            var boxResult = await boxYesNo.ShowAsync();
            if (boxResult == ButtonResult.Yes)
            {
                ShowLoading = true;
                //modify ImagePath, ImageFolderPath and FolderId for each image in imagesToMove 
                List<ImageViewModel> imagesToMoveModifiedPaths = PathHelper.ModifyImagePathsForMoveImagesToNewFolder(imagesToMove, imagesNewFolder);
                for (int i = 0; i < imagesToMoveModifiedPaths.Count; i++)
                {
                    if (File.Exists(imagesToMoveModifiedPaths[i].ImagePath))
                    {
                        var box = MessageBoxManager.GetMessageBoxStandard("Move Images", "One or more images in the destination has the same file name. Pick a different folder", ButtonEnum.Ok);
                        await box.ShowAsync();
                        ShowLoading = false;
                        //reset SelectedImageNewDirectory
                        SelectedImagesNewDirectory = string.Empty;
                        return;
                    }
                }
                //get image move sql
                string imageMoveSql = SqlStringBuilder.BuildSqlForMoveImagesToNewFolder(imagesToMoveModifiedPaths);
                //move images in db
                bool success = await _imageMethods.MoveSelectedImageToNewFolder(imageMoveSql);
                //move images on disk
                if (success) 
                {
                    for (int i = 0; i < imagesToMoveModifiedPaths.Count; i++) 
                    {
                        File.Move(imagesToMove[i].ImagePath, imagesToMoveModifiedPaths[i].ImagePath);
                    }
                    //after adding new images to a folder make sure the user is alerted to rescann them for metadata 
                    imagesNewFolder.FolderContentMetaDataScanned = false;
                    await _folderMethods.UpdateFolder(imagesNewFolder);
                    //if new folder did not have images before it does now so set to true
                    if (imagesNewFolder.HasFiles == false)
                    {
                        imagesNewFolder.HasFiles = true;
                        imagesNewFolder.AreImagesImported = true;
                        await _folderMethods.UpdateFolder(imagesNewFolder);
                    }
                    //update imagesCurrentFolder HasFiles, AreImagesImported, and FolderContentMetaDataScanned
                    //set all back to false if moved all images to new folder
                    IEnumerable<string> folderFiles = Directory.EnumerateFiles(imagesCurrentFolder.FolderPath).Where(s => s.ToLower().EndsWith(".jpeg") || s.ToLower().EndsWith(".jpg") || s.ToLower().EndsWith(".png") || s.ToLower().EndsWith(".gif"));
                    if (!folderFiles.Any())
                    {
                        imagesCurrentFolder.HasFiles = false;
                        imagesCurrentFolder.AreImagesImported = false;
                        imagesCurrentFolder.FolderContentMetaDataScanned = false;
                        await _folderMethods.UpdateFolder(imagesCurrentFolder);
                    }
                    //reset SelectedImageNewDirectory
                    SelectedImagesNewDirectory = string.Empty;
                    //refresh UI
                    await RefreshImages("", allImages[0].FolderId);
                    ShowLoading = false;
                }
                ShowLoading = false;
            }
        }

        private void SelectAllImages(ItemsControl imagesItemsControl)
        {
            for(int i =0; i < Images.Count; i++)
            {
                if (Images[i].IsSelected == true)
                {
                    Images[i].IsSelected = false;
                }
                else
                {
                    Images[i].IsSelected = true;
                }
            }
        }
        private async Task MoveSelectedImagesToTrash(ItemsControl imagesItemsControl)
        {
            List<ImageViewModel> allImages = imagesItemsControl.Items.OfType<ImageViewModel>().ToList();
            List<ImageViewModel> imagesToDelete = new List<ImageViewModel>();
            foreach (ImageViewModel image in allImages)
            {
                if (image.IsSelected && File.Exists(image.ImagePath))
                {
                    imagesToDelete.Add(image);
                }
            }
            if(imagesToDelete.Count == 0)
            {
                var box = MessageBoxManager.GetMessageBoxStandard("Delete Images", "You need to select images to delete.", ButtonEnum.Ok);
                await box.ShowAsync();
                return;
            }
            var boxYesNo = MessageBoxManager.GetMessageBoxStandard("Delete Images", "Are you sure you want to delete these images?", ButtonEnum.YesNo);
            var boxResult = await boxYesNo.ShowAsync();
            if (boxResult == ButtonResult.Yes) 
            {
                ShowLoading = true;
                Folder imagesFolder = await _folderMethods.GetFolderAtDirectory(allImages[0].ImageFolderPath);
                Folder? rootFolder = await _folderMethods.GetRootFolder();
                string trashFolderPath = PathHelper.GetTrashFolderPath(rootFolder.FolderPath);

                //create ImagePerfectTRASH if it doesnt exist
                if (!Directory.Exists(trashFolderPath))
                {
                    Directory.CreateDirectory(trashFolderPath);
                }
                
                string sql = SqlStringBuilder.BuildSqlForMoveImagesToTrash(imagesToDelete);
                bool success = await _imageMethods.DeleteSelectedImages(sql);
                if (success) 
                {
                    foreach (ImageViewModel image in imagesToDelete) 
                    {
                        //move file to trash folder
                        string newImagePath = PathHelper.GetImageFileTrashPath(image, trashFolderPath);
                        File.Move(image.ImagePath, newImagePath);
                    }
                    //update imagesFolder HasFiles, AreImagesImported, and FolderContentMetaDataScanned
                    //set all back to false if moved all images to trash
                    IEnumerable<string> folderFiles = Directory.EnumerateFiles(imagesFolder.FolderPath).Where(s => s.ToLower().EndsWith(".jpeg") || s.ToLower().EndsWith(".jpg") || s.ToLower().EndsWith(".png") || s.ToLower().EndsWith(".gif"));
                    if (!folderFiles.Any()) 
                    {
                        imagesFolder.HasFiles = false;
                        imagesFolder.AreImagesImported = false;
                        imagesFolder.FolderContentMetaDataScanned = false;
                        await _folderMethods.UpdateFolder(imagesFolder);
                    }
                    //refresh UI
                    await RefreshImages("", allImages[0].FolderId);
                    ShowLoading = false;
                }
                ShowLoading = false;
            }
        }

        private async Task MoveImageToTrash(ImageViewModel imageVm)
        {
           
            var boxYesNo = MessageBoxManager.GetMessageBoxStandard("Delete Image", "Are you sure you want to delete your image?", ButtonEnum.YesNo);
            var boxResult = await boxYesNo.ShowAsync();
            if (boxResult == ButtonResult.Yes) 
            {
                ShowLoading = true;
                (List<Folder> folders, List<FolderTag> tags) folderResult = await _folderMethods.GetFoldersInDirectory(imageVm.ImageFolderPath);
                displayFolders = folderResult.folders;
                (List<Image> images, List<ImageTag> tags) imageResultA = await _imageMethods.GetAllImagesInFolder(imageVm.FolderId);
                displayImages = imageResultA.images;
                if (displayImages.Count == 1 && displayFolders.Count == 0)
                {
                    var box = MessageBoxManager.GetMessageBoxStandard("Delete Image", "This is the last image in the folder go back and delete the folder", ButtonEnum.Ok);
                    await box.ShowAsync();
                    return;
                }
                Folder? rootFolder = await _folderMethods.GetRootFolder();
                string trashFolderPath = PathHelper.GetTrashFolderPath(rootFolder.FolderPath);

                //create ImagePerfectTRASH if it doesnt exist
                if (!Directory.Exists(trashFolderPath))
                {
                    Directory.CreateDirectory(trashFolderPath);
                }
                if (File.Exists(imageVm.ImagePath))
                {
                    //delete image from db
                    bool success = await _imageMethods.DeleteImage(imageVm.ImageId);

                    if (success)
                    {
                        //move file to trash folder
                        string newImagePath = PathHelper.GetImageFileTrashPath(imageVm, trashFolderPath);
                        File.Move(imageVm.ImagePath, newImagePath);

                        //refresh UI
                        await RefreshImages("",imageVm.FolderId);
                        ShowLoading = false;
                    }
                }
                ShowLoading = false;
            }
        }
        
        private async void MoveFolderToTrash(FolderViewModel folderVm)
        {
            //only allow delete if folder does not contain children/sub directories
            List<Folder> folderAndSubFolders = await _folderMethods.GetDirectoryTree(folderVm.FolderPath);
            if (folderAndSubFolders.Count > 1) 
            {
                var box = MessageBoxManager.GetMessageBoxStandard("Delete Folder", "This folder contains sub folders clean those up first.", ButtonEnum.Ok);
                await box.ShowAsync();
                return;
            }
            var boxYesNo = MessageBoxManager.GetMessageBoxStandard("Delete Folder", "Are you sure you want to delete your folder?", ButtonEnum.YesNo);
            var boxResult = await boxYesNo.ShowAsync();
            if (boxResult == ButtonResult.Yes) 
            {
                ShowLoading = true;
                //the folders parent
                string pathThatContainsFolder = PathHelper.RemoveOneFolderFromPath(folderVm.FolderPath);                
                Folder? rootFolder = await _folderMethods.GetRootFolder();
                string trashFolderPath = PathHelper.GetTrashFolderPath(rootFolder.FolderPath);

                //create ImagePerfectTRASH if it doesnt exist
                if (!Directory.Exists(trashFolderPath))
                {
                    Directory.CreateDirectory(trashFolderPath);
                }
                if (Directory.Exists(folderVm.FolderPath))
                {
                    //delete folder from db -- does not delete sub folders.
                    //images table child of folders ON DELETE CASCADE is applied on sql to delete all images if a folder is deleted
                    bool success = await _folderMethods.DeleteFolder(folderVm.FolderId);
                    if (success) 
                    {
                        //move folder to trash folder
                        string newFolderPath = PathHelper.GetFolderTrashPath(folderVm, trashFolderPath);
                        Directory.Move(folderVm.FolderPath, newFolderPath);
                        //update the parent folder HasChildren prop
                        List<Folder> parentFolderDirTree = await _folderMethods.GetDirectoryTree(pathThatContainsFolder);
                        Folder parentFolder = await _folderMethods.GetFolderAtDirectory(pathThatContainsFolder);
                        if (parentFolderDirTree.Count > 1)
                        {
                            parentFolder.HasChildren = true;
                            await _folderMethods.UpdateFolder(parentFolder);
                        }
                        else
                        {
                            parentFolder.HasChildren = false;
                            await _folderMethods.UpdateFolder(parentFolder);
                        }
                        //refresh UI
                        await RefreshFolders(pathThatContainsFolder);
                        ShowLoading = false;
                    }
                }
            }
        }

        private async Task ImportAllFoldersOnCurrentPage(ItemsControl foldersItemsControl)
        {
            var boxYesNo = MessageBoxManager.GetMessageBoxStandard("Import All Folders", "CAUTION this could take a long time are you sure?", ButtonEnum.YesNo);
            var boxResult = await boxYesNo.ShowAsync();
            if (boxResult == ButtonResult.Yes)
            {
                List<FolderViewModel> allFolders = foldersItemsControl.Items.OfType<FolderViewModel>().ToList();
                foreach (FolderViewModel folder in allFolders) 
                { 
                    if(folder.HasFiles == true && folder.AreImagesImported == false)
                    {
                        await ImportImages(folder);
                    }
                }
            }
        }

        private async Task ScanAllFoldersOnCurrentPage(ItemsControl foldersItemsControl)
        {
            var boxYesNo = MessageBoxManager.GetMessageBoxStandard("Scan All Folders", "CAUTION this could take a long time are you sure? Make sure to import images first.", ButtonEnum.YesNo);
            var boxResult = await boxYesNo.ShowAsync();
            if (boxResult == ButtonResult.Yes)
            {
                List<FolderViewModel> allFolders = foldersItemsControl.Items.OfType<FolderViewModel>().ToList();
                foreach (FolderViewModel folder in allFolders)
                {
                    if (folder.HasFiles == true && folder.AreImagesImported == true && folder.FolderContentMetaDataScanned == false)
                    {
                        await ScanFolderImagesForMetaData(folder);
                    }
                }
            }
        }
        /*
         * complicated because tags are in image_tags_join table also the tags on image metadata may or may not be in the tags table in database
         * goal is to take metadata from image and write to database. The two should be identical after this point. 
         * With image metadata taking more importance because the app also writes tags and rating to image metadata -- so count that as the master record
         * 
         * Because ImageRating is on the images table and tags are on image_tags_join it is easy to update the ImageRating 
         * in one database trip but the tags are much more complicated because the tag metadata from the image itself will not have
         * the tagId needed for the database also these metadata tags may or may not be in the tags table.
         * 
         * thus for now the most efficient thing i could think to do was to update the ratings in one shot
         * then since not every image will even have a tag only update the ones that have tags -- least amout of db round trips
         * Also for the images that do have tags clear the image_tag_join table 1st so we dont double up on tags in the db. 
         * 
         * perfect heck no... But it works fine for a few hundred or maybe thousand images. 
         * Really how many images are going to be on one folder? I am assuming at most maybe a few thousand
         * 
         */
        private async Task ScanFolderImagesForMetaData(FolderViewModel folderVm)
        {
            ShowLoading = true;
            //get all images at folder id
            (List<Image> images, List<ImageTag> tags) imageResultA = await _imageMethods.GetAllImagesInFolder(folderVm.FolderId);
            List<Image> images = imageResultA.images;
            //scan images for metadata
            List<Image> imagesPlusUpdatedMetaData = await ImageMetaDataHelper.ScanImagesForMetaData(images);
            string imageUpdateSql = SqlStringBuilder.BuildImageSqlForScanMetadata(imagesPlusUpdatedMetaData);
            bool success = await _imageMethods.UpdateImageRatingFromMetaData(imageUpdateSql, folderVm.FolderId);
            foreach (Image image in imagesPlusUpdatedMetaData) 
            {
                if (image.Tags.Count > 0)
                {
                    //avoid duplicates
                    await _imageMethods.ClearImageTagsJoinForMetaData(image);
                    foreach (ImageTag tag in image.Tags)
                    {
                        await _imageMethods.UpdateImageTagFromMetaData(tag);
                    }
                }
            }
            //show data scanned success
            if (success)
            {
                //Update TagsList to show in UI AutoCompleteBox
                await GetTagsList();
                //refresh UI
                if (currentFilter == filters.AllFoldersWithMetadataNotScanned || currentFilter == filters.AllFoldersWithNoImportedImages)
                {
                    //have to call hard refresh for these two cases as they will not be returned from the query to update props
                    await RefreshFolders();
                }
                else
                {
                    await RefreshFolderProps(CurrentDirectory, folderVm);
                }
            }
            ShowLoading = false;
        }

        private async void CreateNewFolder()
        {
            //first check if directory exists
            string newFolderPath = PathHelper.GetNewFolderPath(CurrentDirectory, NewFolderName);
            if (Directory.Exists(newFolderPath))
            {
                var box = MessageBoxManager.GetMessageBoxStandard("New Folder", "A folder with this name already exists.", ButtonEnum.Ok);
                await box.ShowAsync();
                return;
            }
            //add dir to database -- also need to update parent folders HasChildren bool value
            Folder newFolder = new Folder
            {
                FolderName = NewFolderName,
                FolderPath = newFolderPath,
                HasChildren = false,
                CoverImagePath = "",
                FolderDescription = "",
                FolderRating = 0,
                HasFiles = false,
                IsRoot = false,
                FolderContentMetaDataScanned = false,
                AreImagesImported = false,
            };
            bool success = await _folderMethods.CreateNewFolder(newFolder);

            //create on disk
            if (success)
            {
                try
                {
                    Directory.CreateDirectory(newFolderPath);
                    //refresh UI
                    currentFilter = filters.None;
                    await RefreshFolders();
                }
                catch (Exception e)
                {
                    var box = MessageBoxManager.GetMessageBoxStandard("New Folder", $"Error {e}.", ButtonEnum.Ok);
                    await box.ShowAsync();
                    return;
                }
            }
        }


        private async void GetAllFolders()
        {
            List<Folder> allFolders = await _folderMethods.GetAllFolders();
            foreach (Folder folder in allFolders) 
            {
                FolderViewModel folderViewModel = await FolderMapper.GetFolderVm(folder);
                LibraryFolders.Add(folderViewModel);
            }
        }
    }
}

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
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;

namespace ImagePerfect.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly FolderMethods _folderMethods;
        private readonly ImageCsvMethods _imageCsvMethods;
        private readonly ImageMethods _imageMethods;
        private bool _showLoading;
        private int _totalImages = 0;
        private string _currentDirectory = string.Empty;
        private string _savedDirectory = string.Empty;
        private string _selectedImagesNewDirectory = string.Empty;
        private bool _filterInCurrentDirectory = false;
        private string _rootFolderLocation = string.Empty;
        private List<Tag> _tagsList = new List<Tag>();

        public List<Folder> displayFolders = new List<Folder>();
        private List<FolderTag> displayFolderTags = new List<FolderTag>();
        public List<Image> displayImages = new List<Image>();
        private List<ImageTag> displayImageTags = new List<ImageTag>();
        //pagination
        //see FolderPageSize in SettingsVm
        private int _totalFolderPages = 1;
        private int _currentFolderPage = 1;
        private int _savedTotalFolderPages = 1;
        private int _savedFolderPage = 1;

        //used to save scrollviewer offset
        private Vector _savedOffsetVector = new Vector();

        //see ImagePageSize in SettingsVm
        private int _totalImagePages = 1;
        private int _currentImagePage = 1;
        private int _savedTotalImagePages = 1;
        private int _savedImagePage = 1;
        //max value between TotalFolderPages or TotalImagePages
        private int _maxPage = 1;
        //max value between CurrentFolderPage or CurrentImagePage
        private int _maxCurrentPage = 1;

        //Filters
        public enum Filters
        {
            None,
            ImageRatingFilter,
            FolderRatingFilter,
            ImageTagFilter,
            FolderTagFilter,
            FolderDescriptionFilter,
            AllFavoriteFolders,
            AllFoldersWithNoImportedImages,
            AllFoldersWithMetadataNotScanned,
            AllFoldersWithoutCovers
        }
        public Filters currentFilter = Filters.None;
        private int selectedRatingForFilter = 0;
        private string tagForFilter = string.Empty;
        private string textForFilter = string.Empty;

        public MainWindowViewModel() { }
        public MainWindowViewModel(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _folderMethods = new FolderMethods(_unitOfWork);
            _imageCsvMethods = new ImageCsvMethods(_unitOfWork);
            _imageMethods = new ImageMethods(_unitOfWork);
            _showLoading = false;

            InitializeVm = new InitializeViewModel(_unitOfWork, this);
            SavedDirectoryVm = new SavedDirectoryViewModel(_unitOfWork, this);
            FavoriteFoldersVm = new FavoriteFoldersViewModel(_unitOfWork);
            SettingsVm = new SettingsViewModel(_unitOfWork, this);
            MoveImages = new MoveImagesViewModel(_unitOfWork, this);
            MoveFolderToTrash = new MoveFolderToTrashViewModel(_unitOfWork, this);
            CreateNewFolder = new CreateNewFolderViewModel(_unitOfWork, this);

            NextFolderCommand = ReactiveCommand.Create((FolderViewModel currentFolder) => {
                NextFolder(currentFolder);
            });
            BackFolderCommand = ReactiveCommand.Create((FolderViewModel currentFolder) => {
                BackFolder(currentFolder);
            });
            BackFolderFromImageCommand = ReactiveCommand.Create((ImageViewModel imageVm) => {
                BackFolderFromImage(imageVm);
            });
            BackFolderFromDirectoryOptionsPanelCommand = ReactiveCommand.Create(() => {
                BackFolderFromDirectoryOptionsPanel();
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
            AddMultipleImageTagsCommand = ReactiveCommand.Create((ListBox selectedTagsListBox) => {
                AddMultipleImageTags(selectedTagsListBox);
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
                await MoveImages.MoveImageToTrash(imageVm);
            });
            MoveFolderToTrashCommand = ReactiveCommand.Create(async (FolderViewModel folderVm) => {
                await MoveFolderToTrash.MoveFolderToTrash(folderVm);
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
                ToggleUI.ToggleSettings();
            });
            ToggleManageImagesCommand = ReactiveCommand.Create(() => {
                ToggleUI.ToggleManageImages();
            });
            ToggleFiltersCommand = ReactiveCommand.Create((string showFilter) => {
                ToggleUI.ToggleFilters(showFilter);
            });
            ToggleCreateNewFolderCommand = ReactiveCommand.Create(() => {
                ToggleUI.ToggleCreateNewFolder();
            });
            ToggleGetTotalImagesCommand = ReactiveCommand.Create(async () => {
                ToggleUI.ToggleGetTotalImages();
                if (ToggleUI.ShowTotalImages == true)
                {
                    TotalImages = await _imageMethods.GetTotalImages();
                }
            });
            ToggleImportAndScanCommand = ReactiveCommand.Create(() => {
                ToggleUI.ToggleImportAndScan();
            });
            ToggleListAllTagsCommand = ReactiveCommand.Create(() => {
                ToggleUI.ToggleListAllTags();
            });
            FilterImagesOnRatingCommand = ReactiveCommand.Create(async (decimal rating) => {
                ResetPagination();
                selectedRatingForFilter = Decimal.ToInt32(rating);
                currentFilter = Filters.ImageRatingFilter;
                await RefreshImages();
            });
            FilterFoldersOnRatingCommand = ReactiveCommand.Create(async (decimal rating) => {
                ResetPagination();
                selectedRatingForFilter = Decimal.ToInt32(rating);
                currentFilter = Filters.FolderRatingFilter;
                await RefreshFolders();
            });
            FilterImagesOnTagCommand = ReactiveCommand.Create(async (string tag) => {
                ResetPagination();
                tagForFilter = tag;
                currentFilter = Filters.ImageTagFilter;
                await RefreshImages();
            });
            FilterFoldersOnTagCommand = ReactiveCommand.Create(async (string tag) => {
                ResetPagination();
                tagForFilter = tag;
                currentFilter = Filters.FolderTagFilter;
                await RefreshFolders();
            });
            FilterFoldersOnDescriptionCommand = ReactiveCommand.Create(async (string text) => {
                ResetPagination();
                textForFilter = text;
                currentFilter = Filters.FolderDescriptionFilter;
                await RefreshFolders();
            });
            GetAllFoldersWithNoImportedImagesCommand = ReactiveCommand.Create(async () => {
                ResetPagination();
                currentFilter = Filters.AllFoldersWithNoImportedImages;
                await RefreshFolders();
            });
            GetAllFoldersWithMetadataNotScannedCommand = ReactiveCommand.Create(async () => {
                ResetPagination();
                currentFilter = Filters.AllFoldersWithMetadataNotScanned;
                await RefreshFolders();
            });
            GetAllFoldersWithoutCoversCommand = ReactiveCommand.Create(async () => {
                ResetPagination();
                currentFilter = Filters.AllFoldersWithoutCovers;
                await RefreshFolders();
            });
            LoadCurrentDirectoryCommand = ReactiveCommand.Create(async () => {
                await LoadCurrentDirectory();
            });
            PickImageWidthCommand = ReactiveCommand.Create(async (string size) => {
                await SettingsVm.PickImageWidth(size);
            });
            SelectImageWidthCommand = ReactiveCommand.Create(async (decimal size) => { 
                await SettingsVm.SelectImageWidth(size);
            });
            PickFolderPageSizeCommand = ReactiveCommand.Create(async (string size) => {
                await SettingsVm.PickFolderPageSize(size);
            });
            PickImagePageSizeCommand = ReactiveCommand.Create(async (string size) => {
                await SettingsVm.PickImagePageSize(size);
            });
            SaveDirectoryCommand = ReactiveCommand.Create(async (ScrollViewer scrollViewer) => {
                await SavedDirectoryVm.SaveDirectory(scrollViewer);
            });
            LoadSavedDirectoryCommand = ReactiveCommand.Create(async (ScrollViewer scrollViewer) => {
                await SavedDirectoryVm.LoadSavedDirectory(scrollViewer);
            });
            SaveFolderAsFavoriteCommand = ReactiveCommand.Create(async (FolderViewModel folderVm) => {
                await FavoriteFoldersVm.SaveFolderAsFavorite(folderVm);
            });
            GetAllFavoriteFoldersCommand = ReactiveCommand.Create(async () => {
                ResetPagination();
                currentFilter = Filters.AllFavoriteFolders;
                await RefreshFolders();
            });
            RemoveAllFavoriteFoldersCommand = ReactiveCommand.Create(async () => {
                await FavoriteFoldersVm.RemoveAllFavoriteFolders();
            });
            MoveSelectedImagesToTrashCommand = ReactiveCommand.Create(async (ItemsControl imagesItemsControl) =>
            {
                await MoveImages.MoveSelectedImagesToTrash(imagesItemsControl);
            });
            SelectAllImagesCommand = ReactiveCommand.Create(() => {
                MoveImages.SelectAllImages();
            });
            MoveSelectedImagesToNewFolderCommand = ReactiveCommand.Create(async (ItemsControl imagesItemsControl) => {
                await MoveImages.MoveSelectedImagesToNewFolder(imagesItemsControl);
            });
            ImportAllFoldersOnCurrentPageCommand = ReactiveCommand.Create(async (ItemsControl foldersItemsControl) => { 
                await ImportAllFoldersOnCurrentPage(foldersItemsControl);
            });
            AddCoverImageOnCurrentPageCommand = ReactiveCommand.Create(async (ItemsControl folderItemsControl) => { 
                await AddCoverImageOnCurrentPage(folderItemsControl);
            });
            ScanAllFoldersOnCurrentPageCommand = ReactiveCommand.Create(async (ItemsControl foldersItemsControl) => {
                await ScanAllFoldersOnCurrentPage(foldersItemsControl);
            });
            CopyCoverImageToContainingFolderCommand = ReactiveCommand.Create(async (FolderViewModel folderVm) => { 
                await CopyCoverImageToContainingFolder(folderVm);
            });
            CreateNewFolderCommand = ReactiveCommand.Create(async () => {
                await CreateNewFolder.CreateNewFolder();
            });
            ExitAppCommand = ReactiveCommand.Create(() => { 
                ExitApp();
            });
            InitializeVm.Initialize();
        }

        public int TotalImages
        {
            get => _totalImages;
            set => this.RaiseAndSetIfChanged(ref _totalImages, value);  
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

        public Vector SavedOffsetVector
        {
            get => _savedOffsetVector;
            set => _savedOffsetVector = value;
        }
        public int CurrentFolderPage
        {
            get => _currentFolderPage;
            set => this.RaiseAndSetIfChanged(ref _currentFolderPage, value);
        }
        public List<Tag> TagsList
        {
            get => _tagsList;
            set => this.RaiseAndSetIfChanged(ref _tagsList, value); 
        }
        public bool ShowLoading
        {
            get => _showLoading;
            set => this.RaiseAndSetIfChanged(ref _showLoading, value);
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
        public string RootFolderLocation
        {
            get => _rootFolderLocation;
            set => _rootFolderLocation = value;
        }

        public InitializeViewModel InitializeVm { get; }
        public SavedDirectoryViewModel SavedDirectoryVm { get; }
        public FavoriteFoldersViewModel FavoriteFoldersVm { get; }
        public SettingsViewModel SettingsVm { get; }
        public MoveImagesViewModel MoveImages { get; }
        public MoveFolderToTrashViewModel MoveFolderToTrash { get; }
        public CreateNewFolderViewModel CreateNewFolder { get; }
        public ToggleUIViewModel ToggleUI { get; } = new ToggleUIViewModel();

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

        public ReactiveCommand<Unit, Unit> BackFolderFromDirectoryOptionsPanelCommand { get; }

        public ReactiveCommand<FolderViewModel, Task> ImportImagesCommand { get; }

        public ReactiveCommand<FolderViewModel, Unit> AddFolderDescriptionCommand { get; }

        public ReactiveCommand<FolderViewModel, Unit> AddFolderTagsCommand { get; }

        public ReactiveCommand<FolderViewModel, Unit> EditFolderTagsCommand { get; }

        public ReactiveCommand<FolderViewModel, Unit> AddFolderRatingCommand { get; }

        public ReactiveCommand<ImageViewModel, Unit> AddImageTagsCommand { get; }

        public ReactiveCommand<ListBox, Unit> AddMultipleImageTagsCommand { get; }

        public ReactiveCommand<ImageViewModel, Unit> EditImageTagsCommand { get; }

        public ReactiveCommand<ImageViewModel, Unit> AddImageRatingCommand { get; }

        public ReactiveCommand<Unit, Unit> DeleteLibraryCommand { get; }

        public ReactiveCommand<ImageViewModel, Unit> OpenImageInExternalViewerCommand { get; }

        public ReactiveCommand<Unit, Unit> OpenCurrentDirectoryWithExplorerCommand { get; }

        public ReactiveCommand<ImageViewModel, Task> MoveImageToTrashCommand { get; }

        public ReactiveCommand<FolderViewModel, Task> MoveFolderToTrashCommand { get; }

        public ReactiveCommand<FolderViewModel, Task> ScanFolderImagesForMetaDataCommand { get; }

        public ReactiveCommand<Unit, Unit> NextPageCommand { get; }

        public ReactiveCommand<Unit, Unit> PreviousPageCommand { get; }

        public ReactiveCommand<decimal, Task> GoToPageCommand { get; }

        public ReactiveCommand<Unit, Unit> ToggleSettingsCommand { get; }

        public ReactiveCommand<string, Unit> ToggleFiltersCommand { get; }

        public ReactiveCommand<Unit, Unit> ToggleManageImagesCommand { get; }

        public ReactiveCommand<Unit, Unit> ToggleCreateNewFolderCommand { get; }

        public ReactiveCommand<Unit, Task> ToggleGetTotalImagesCommand { get; }

        public ReactiveCommand<Unit, Unit> ToggleImportAndScanCommand { get; }

        public ReactiveCommand<Unit, Unit> ToggleListAllTagsCommand { get; }

        public ReactiveCommand<decimal, Task> FilterImagesOnRatingCommand { get; }

        public ReactiveCommand<decimal, Task> FilterFoldersOnRatingCommand { get; }

        public ReactiveCommand<string, Task> FilterImagesOnTagCommand { get; }

        public ReactiveCommand<string, Task> FilterFoldersOnTagCommand  { get; }

        public ReactiveCommand<string, Task> FilterFoldersOnDescriptionCommand { get; }

        public ReactiveCommand<Unit, Task> GetAllFoldersWithNoImportedImagesCommand { get; }

        public ReactiveCommand<Unit, Task> GetAllFoldersWithMetadataNotScannedCommand { get; }

        public ReactiveCommand<Unit, Task> GetAllFoldersWithoutCoversCommand { get; }

        public ReactiveCommand<Unit, Task> LoadCurrentDirectoryCommand { get; }

        public ReactiveCommand<string, Task> PickImageWidthCommand { get; }
        
        public ReactiveCommand<decimal, Task> SelectImageWidthCommand { get; }

        public ReactiveCommand<string, Task> PickFolderPageSizeCommand { get; }

        public ReactiveCommand<string, Task> PickImagePageSizeCommand { get; }

        public ReactiveCommand<ScrollViewer, Task> SaveDirectoryCommand { get; }

        public ReactiveCommand<ScrollViewer, Task> LoadSavedDirectoryCommand { get; }

        public ReactiveCommand<FolderViewModel, Task> SaveFolderAsFavoriteCommand { get; }

        public ReactiveCommand<Unit, Task> GetAllFavoriteFoldersCommand {  get; } 

        public ReactiveCommand<Unit, Task> RemoveAllFavoriteFoldersCommand { get; }

        public ReactiveCommand<ItemsControl, Task> MoveSelectedImagesToTrashCommand { get; }

        public ReactiveCommand<Unit, Unit> SelectAllImagesCommand { get; }

        public ReactiveCommand<ItemsControl, Task> MoveSelectedImagesToNewFolderCommand { get; }

        public ReactiveCommand<ItemsControl, Task> ImportAllFoldersOnCurrentPageCommand { get; }

        public ReactiveCommand<ItemsControl, Task> AddCoverImageOnCurrentPageCommand { get; }

        public ReactiveCommand<FolderViewModel, Task> CopyCoverImageToContainingFolderCommand { get; }

        public ReactiveCommand<ItemsControl, Task> ScanAllFoldersOnCurrentPageCommand { get; }

        public ReactiveCommand<Unit, Task> CreateNewFolderCommand { get; }

        public ReactiveCommand<Unit, Unit> ExitAppCommand { get; }

        //should technically have its own repo but only plan on having only this one method just keeping it in images repo.
        public async Task GetTagsList()
        {
            TagsList = await _imageMethods.GetTagsList();
        }
        private void ExitApp()
        {
            //Application.Current.ApplicationLifetime;
            if (Application.Current.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                desktop.Shutdown();
            }
        }
       
        public async Task LoadCurrentDirectory()
        {
            currentFilter = Filters.None;
            await RefreshFolders(CurrentDirectory);
            await RefreshImages(CurrentDirectory);
        }
        
        private List<Image> ImagePagination()
        {
            //same as FolderPagination
            int offset = SettingsVm.ImagePageSize * (CurrentImagePage -1);
            int totalImageCount = displayImages.Count;
            if(totalImageCount == 0 || totalImageCount <= SettingsVm.ImagePageSize)
                return displayImages;
            TotalImagePages = (int)Math.Ceiling(totalImageCount / (double)SettingsVm.ImagePageSize);
            List<Image> displayImagesTemp;
            if(CurrentImagePage == TotalImagePages)
            {
                displayImagesTemp = displayImages.GetRange(offset, (totalImageCount - (TotalImagePages - 1)* SettingsVm.ImagePageSize));
            }
            else
            {
                displayImagesTemp = displayImages.GetRange(offset, SettingsVm.ImagePageSize);
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
            int offest = SettingsVm.FolderPageSize * (CurrentFolderPage - 1);
            int totalFolderCount = displayFolders.Count;
            if (totalFolderCount == 0 || totalFolderCount <= SettingsVm.FolderPageSize) 
                return displayFolders; 
            TotalFolderPages = (int)Math.Ceiling(totalFolderCount / (double)SettingsVm.FolderPageSize);
            List<Folder> displayFoldersTemp;
            if (CurrentFolderPage == TotalFolderPages)
            {
                //on last page GetRange count CANNOT be FolderPageSize or index out of range 
                //thus following logical example above in a array of 14 elements the range count on the last page is 14 - 10
                //formul used: totalFolderCount - ((TotalFolderPages - 1)*FolderPageSize)
                //folderCount minus total folders on all but last page
                //14 - 10
                displayFoldersTemp = displayFolders.GetRange(offest, (totalFolderCount - (TotalFolderPages - 1)* SettingsVm.FolderPageSize));
            }
            else
            {
                displayFoldersTemp = displayFolders.GetRange(offest, SettingsVm.FolderPageSize);
            }
            MaxPage = Math.Max(TotalImagePages, TotalFolderPages);
            MaxCurrentPage = Math.Max(CurrentImagePage, CurrentFolderPage);
            return displayFoldersTemp;
        }

        public void ResetPagination()
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
                case Filters.None:
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
                case Filters.FolderRatingFilter:
                    (List<Folder> folders, List<FolderTag> tags) folderRatingResult = await _folderMethods.GetAllFoldersAtRating(selectedRatingForFilter, FilterInCurrentDirectory, CurrentDirectory);
                    displayFolders = folderRatingResult.folders;
                    displayFolderTags = folderRatingResult.tags;

                    Images.Clear();
                    LibraryFolders.Clear();
                    displayFolders = FolderPagination();
                    await MapTagsToFoldersAddToObservable();
                    break;
                case Filters.FolderTagFilter:
                    (List<Folder> folders, List<FolderTag> tags) folderTagResult = await _folderMethods.GetAllFoldersWithTag(tagForFilter, FilterInCurrentDirectory, CurrentDirectory);
                    displayFolders = folderTagResult.folders;
                    displayFolderTags = folderTagResult.tags;

                    Images.Clear();
                    LibraryFolders.Clear();
                    displayFolders = FolderPagination();
                    await MapTagsToFoldersAddToObservable();
                    break;
                case Filters.FolderDescriptionFilter:
                    (List<Folder> folders, List<FolderTag> tags) folderDescriptionResult = await _folderMethods.GetAllFoldersWithDescriptionText(textForFilter, FilterInCurrentDirectory, CurrentDirectory);
                    displayFolders = folderDescriptionResult.folders;
                    displayFolderTags = folderDescriptionResult.tags;

                    Images.Clear();
                    LibraryFolders.Clear();
                    displayFolders = FolderPagination();
                    await MapTagsToFoldersAddToObservable();
                    break;
                case Filters.AllFavoriteFolders:
                    (List<Folder> folders, List<FolderTag> tags) allFavoriteFoldersResult = await _folderMethods.GetAllFavoriteFolders();
                    displayFolders = allFavoriteFoldersResult.folders;
                    displayFolderTags = allFavoriteFoldersResult.tags;

                    Images.Clear();
                    LibraryFolders.Clear();
                    displayFolders = FolderPagination();
                    await MapTagsToFoldersAddToObservable();
                    break;
                case Filters.AllFoldersWithNoImportedImages:
                    (List<Folder> folders, List<FolderTag> tags) allFoldersWithNoImportedImagesResult = await _folderMethods.GetAllFoldersWithNoImportedImages(FilterInCurrentDirectory, CurrentDirectory);
                    displayFolders = allFoldersWithNoImportedImagesResult.folders;
                    displayFolderTags = allFoldersWithNoImportedImagesResult.tags;

                    Images.Clear();
                    LibraryFolders.Clear();
                    displayFolders = FolderPagination();
                    await MapTagsToFoldersAddToObservable();
                    break;
                case Filters.AllFoldersWithMetadataNotScanned:
                    (List<Folder> folders, List<FolderTag> tags) allFoldersWithMetadataNotScannedResult = await _folderMethods.GetAllFoldersWithMetadataNotScanned(FilterInCurrentDirectory, CurrentDirectory);
                    displayFolders = allFoldersWithMetadataNotScannedResult.folders;
                    displayFolderTags = allFoldersWithMetadataNotScannedResult.tags;

                    Images.Clear();
                    LibraryFolders.Clear();
                    displayFolders = FolderPagination();
                    await MapTagsToFoldersAddToObservable();
                    break;
                case Filters.AllFoldersWithoutCovers:
                    (List<Folder> folders, List<FolderTag> tags) allFoldersWithoutCoversResult = await _folderMethods.GetAllFoldersWithoutCovers(FilterInCurrentDirectory, CurrentDirectory);
                    displayFolders = allFoldersWithoutCoversResult.folders;
                    displayFolderTags = allFoldersWithoutCoversResult.tags;

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
                case Filters.None:
                    (List<Folder> folders, List<FolderTag> tags) folderResult = await _folderMethods.GetFoldersInDirectory(path);
                    displayFolders = folderResult.folders;
                    displayFolderTags = folderResult.tags;
                    displayFolders = FolderPagination();
                    await MapTagsToSingleFolderUpdateObservable(folderVm);
                    break;
                case Filters.FolderRatingFilter:
                    (List<Folder> folders, List<FolderTag> tags) folderRatingResult = await _folderMethods.GetAllFoldersAtRating(selectedRatingForFilter, FilterInCurrentDirectory, CurrentDirectory);
                    displayFolders = folderRatingResult.folders;
                    displayFolderTags = folderRatingResult.tags;
                    displayFolders = FolderPagination();
                    await MapTagsToSingleFolderUpdateObservable(folderVm);
                    break;
                case Filters.FolderTagFilter:
                    (List<Folder> folders, List<FolderTag> tags) folderTagResult = await _folderMethods.GetAllFoldersWithTag(tagForFilter, FilterInCurrentDirectory, CurrentDirectory);
                    displayFolders = folderTagResult.folders;
                    displayFolderTags = folderTagResult.tags;
                    displayFolders = FolderPagination();
                    await MapTagsToSingleFolderUpdateObservable(folderVm);
                    break;
                case Filters.FolderDescriptionFilter:
                    (List<Folder> folders, List<FolderTag> tags) folderDescriptionResult = await _folderMethods.GetAllFoldersWithDescriptionText(textForFilter, FilterInCurrentDirectory, CurrentDirectory);
                    displayFolders = folderDescriptionResult.folders;
                    displayFolderTags = folderDescriptionResult.tags;
                    displayFolders = FolderPagination();
                    await MapTagsToSingleFolderUpdateObservable(folderVm);
                    break;
                case Filters.AllFavoriteFolders:
                    (List<Folder> folders, List<FolderTag> tags) allFavoriteFoldersResult = await _folderMethods.GetAllFavoriteFolders();
                    displayFolders = allFavoriteFoldersResult.folders;
                    displayFolderTags = allFavoriteFoldersResult.tags;
                    displayFolders = FolderPagination();
                    await MapTagsToSingleFolderUpdateObservable(folderVm);
                    break;
                case Filters.AllFoldersWithNoImportedImages:
                    (List<Folder> folders, List<FolderTag> tags) allFoldersWithNoImportedImagesResult = await _folderMethods.GetAllFoldersWithNoImportedImages(FilterInCurrentDirectory, CurrentDirectory);
                    displayFolders = allFoldersWithNoImportedImagesResult.folders;
                    displayFolderTags = allFoldersWithNoImportedImagesResult.tags;
                    displayFolders = FolderPagination();
                    await MapTagsToSingleFolderUpdateObservable(folderVm);
                    break;
                case Filters.AllFoldersWithMetadataNotScanned:
                    (List<Folder> folders, List<FolderTag> tags) allFoldersWithMetadataNotScannedResult = await _folderMethods.GetAllFoldersWithMetadataNotScanned(FilterInCurrentDirectory, CurrentDirectory);
                    displayFolders = allFoldersWithMetadataNotScannedResult.folders;
                    displayFolderTags = allFoldersWithMetadataNotScannedResult.tags;
                    displayFolders = FolderPagination();
                    await MapTagsToSingleFolderUpdateObservable(folderVm);
                    break;
                case Filters.AllFoldersWithoutCovers:
                    (List<Folder> folders, List<FolderTag> tags) allFoldersWithoutCoversResult = await _folderMethods.GetAllFoldersWithoutCovers(FilterInCurrentDirectory, CurrentDirectory);
                    displayFolders = allFoldersWithoutCoversResult.folders;
                    displayFolderTags = allFoldersWithoutCoversResult.tags;
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
        public async Task RefreshImages(string path = "", int folderId = 0)
        {
            ShowLoading = true;
            switch (currentFilter)
            {
                case Filters.None:
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
                case Filters.ImageRatingFilter:
                    (List<Image> images, List<ImageTag> tags) imageRatingResult = await _imageMethods.GetAllImagesAtRating(selectedRatingForFilter, FilterInCurrentDirectory, CurrentDirectory);
                    displayImages = imageRatingResult.images;
                    displayImageTags = imageRatingResult.tags;

                    Images.Clear();
                    LibraryFolders.Clear();
                    displayImages = ImagePagination();
                    await MapTagsToImagesAddToObservable();
                    break;
                case Filters.ImageTagFilter:
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
                if(currentFilter == Filters.AllFoldersWithMetadataNotScanned || currentFilter == Filters.AllFoldersWithNoImportedImages)
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


        //think all three BackFolder methods can just be reduced to this one
        private async void BackFolderFromDirectoryOptionsPanel()
        {
            if(CurrentDirectory == RootFolderLocation)
            {
                return;
            }
            //not ideal but keeps pagination to the folder your in. When you go back or next start from page 1
            ResetPagination();
            
            string newPath = PathHelper.RemoveOneFolderFromPath(CurrentDirectory);
            //set the current directory -- used to add new folder to location
            CurrentDirectory = newPath;
            //refresh UI
            currentFilter = Filters.None;
            await RefreshFolders();
            await RefreshImages(newPath);
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
            currentFilter = Filters.None;
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
            currentFilter = Filters.None;
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
                CurrentDirectory = PathHelper.RemoveOneFolderFromPath(currentFolder.FolderPath);
                return;
            }
            else
            {
                //refresh UI
                currentFilter = Filters.None;
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
        
        private async void AddMultipleImageTags(ListBox selectedTagsListBox)
        {
            if (selectedTagsListBox.DataContext != null && selectedTagsListBox.SelectedItems != null)
            {
                ImageViewModel imageVm = (ImageViewModel)selectedTagsListBox.DataContext;
                List<Tag> tagsToAdd = new List<Tag>();
                //nothing selected just return
                if (selectedTagsListBox.SelectedItems.Count == 0)
                {
                    return;
                }
                //if no current tags just add all to list
                if (imageVm.ImageTags == "" || imageVm.ImageTags == null)
                {
                    foreach (Tag selectedTag in selectedTagsListBox.SelectedItems)
                    {
                        tagsToAdd.Add(selectedTag);
                    }
                }
                //else only add non duplicates
                else
                {
                    foreach (Tag selectedTag in selectedTagsListBox.SelectedItems)
                    {
                        if (!imageVm.ImageTags.Contains(selectedTag.TagName))
                        {
                            tagsToAdd.Add(selectedTag);
                        }
                    }
                }
                //add new tags to ImageTags -- KEEP!! THIS IS NEEDED TO WRITE METADATA
                foreach (Tag selectedTag in tagsToAdd)
                {
                    if (string.IsNullOrEmpty(imageVm.ImageTags))
                    {
                        imageVm.ImageTags = selectedTag.TagName;
                    }
                    else
                    {
                        imageVm.ImageTags = imageVm.ImageTags + "," + selectedTag.TagName;
                    }
                }
                //build sql for bulk insert
                string sql = SqlStringBuilder.BuildSqlForAddMultipleImageTags(tagsToAdd, imageVm);
                //update sql db
                bool success = await _imageMethods.AddMultipleImageTags(sql);
                //write new tags to image file
                if (success)
                {
                    //write new tags to image metadata
                    await ImageMetaDataHelper.WriteTagToImage(imageVm);
                }
                else
                {
                    List<string> imageTags = imageVm.ImageTags.Split(",").ToList();
                    //if fail remove the tags from the Tags list in the UI
                    foreach (Tag tag in tagsToAdd)
                    {
                       imageTags.Remove(tag.TagName);
                    }
                    for (int i = 0; i < imageTags.Count; i++) 
                    {
                        if (i == 0)
                        {
                            imageVm.ImageTags = imageTags[i];
                        }
                        else
                        {
                            imageVm.ImageTags = imageVm.ImageTags + "," + imageTags[i];
                            
                        }
                    }
                }
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

        private async Task CopyCoverImageToContainingFolder(FolderViewModel folderVm)
        {
            if (PathHelper.RemoveOneFolderFromPath(folderVm.FolderPath) == RootFolderLocation) 
            {
                var box = MessageBoxManager.GetMessageBoxStandard("Copy Cover", "Cannot copy from root folder.", ButtonEnum.Ok);
                await box.ShowAsync();
                return;
            }
            if(folderVm.CoverImagePath == "" || folderVm.CoverImagePath == null)
            {
                var box = MessageBoxManager.GetMessageBoxStandard("Copy Cover", "The folder must have a cover selected to copy.", ButtonEnum.Ok);
                await box.ShowAsync();
                return;
            }
            
            
            string coverImageCurrentPath = folderVm.CoverImagePath;
            string coverImageNewPath = PathHelper.GetCoverImagePathForCopyCoverImageToContainingFolder(folderVm);
            Folder containingFolder = await _folderMethods.GetFolderAtDirectory(PathHelper.RemoveOneFolderFromPath(folderVm.FolderPath));
            if (containingFolder.CoverImagePath != "")
            {
                var boxYesNo = MessageBoxManager.GetMessageBoxStandard("Copy Cover", "Containing folder already has a cover. Do you want to copy another?", ButtonEnum.YesNo);
                var boxResult = await boxYesNo.ShowAsync();
                if (boxResult == ButtonResult.No)
                {
                    return;
                }
            }
            if (File.Exists(coverImageNewPath)) 
            {
                var box = MessageBoxManager.GetMessageBoxStandard("Copy Cover", "A cover image in the destination has the same file name. Pick a different cover", ButtonEnum.Ok);
                await box.ShowAsync();
                return;
            }
            //add cover image path to containing folder
            bool success = await _folderMethods.UpdateCoverImage(coverImageNewPath, containingFolder.FolderId);
            //copy file in file system
            if (success) 
            {
                File.Copy(coverImageCurrentPath, coverImageNewPath);
            }
        }
        private async Task AddCoverImageOnCurrentPage(ItemsControl foldersItemsControl)
        {
            List<FolderViewModel> allFolders = foldersItemsControl.Items.OfType<FolderViewModel>().ToList();
            Random random = new Random();
            foreach (FolderViewModel folder in allFolders)
            {
                if (folder.HasFiles == true && folder.AreImagesImported == true)
                {
                    (List<Image> images, List<ImageTag> tags) imageResult = await _imageMethods.GetAllImagesInFolder(folder.FolderId);
                    List<Image> images = imageResult.images;
                    int randomIndex = random.Next(0, images.Count - 1);
                    //set random fall back cover
                    string cover = images[randomIndex].ImagePath;
                    //get cover clean
                    foreach (Image image in images) 
                    { 
                        if (image.ImagePath.ToLower().Contains("cover") && image.ImagePath.ToLower().Contains("clean"))
                        {
                            cover = image.ImagePath;    
                            break;
                        }
                    }
                    //if no cover clean get poster
                    if (!(cover.ToLower().Contains("cover") && cover.ToLower().Contains("clean")))
                    {
                        foreach (Image image in images)
                        {
                            if (image.ImagePath.ToLower().Contains("poster"))
                            {
                                cover = image.ImagePath;
                                break;
                            }
                        }
                    }
                    //if no poster or no cover clean get cover
                    if (!(cover.ToLower().Contains("poster") || (cover.ToLower().Contains("cover") && cover.ToLower().Contains("clean"))))
                    {
                        foreach (Image image in images)
                        {
                            if (image.ImagePath.ToLower().Contains("cover"))
                            {
                                cover = image.ImagePath;
                                break;
                            }
                        }
                    }
                    folder.CoverImagePath = cover;
                    await _folderMethods.UpdateFolder(FolderMapper.GetFolderFromVm(folder));
                }
            }
            await RefreshFolders();
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
            ResetPagination();
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
                ResetPagination();
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
                if (currentFilter == Filters.AllFoldersWithMetadataNotScanned || currentFilter == Filters.AllFoldersWithNoImportedImages)
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

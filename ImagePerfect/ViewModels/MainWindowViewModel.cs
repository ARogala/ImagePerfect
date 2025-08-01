﻿using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Threading;
using ImagePerfect.Models;
using ImagePerfect.ObjectMappers;
using ImagePerfect.Repository.IRepository;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Reactive;
using System.Threading.Tasks;
using Image = ImagePerfect.Models.Image;

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

            DirectoryNavigationVm = new DirectoryNavigationViewModel(this);
            ModifyFolderDataVm = new ModifyFolderDataViewModel(_unitOfWork, this);
            ModifyImageDataVm = new ModifyImageDataViewModel(_unitOfWork, this);
            ExternalProgramVm = new ExternalProgramViewModel(this);
            CoverImageVm = new CoverImageViewModel(_unitOfWork, this);
            ScanImagesForMetaDataVm = new ScanImagesForMetaDataViewModel(_unitOfWork, this);
            ImportImagesVm = new ImportImagesViewModel(_unitOfWork, this);
            InitializeVm = new InitializeViewModel(_unitOfWork, this);
            SavedDirectoryVm = new SavedDirectoryViewModel(_unitOfWork, this);
            FavoriteFoldersVm = new FavoriteFoldersViewModel(_unitOfWork);
            SettingsVm = new SettingsViewModel(_unitOfWork, this);
            MoveImages = new MoveImagesViewModel(_unitOfWork, this);
            MoveFolderToTrash = new MoveFolderToTrashViewModel(_unitOfWork, this);
            CreateNewFolder = new CreateNewFolderViewModel(_unitOfWork, this);

            NextFolderCommand = ReactiveCommand.Create((FolderViewModel currentFolder) => {
                DirectoryNavigationVm.NextFolder(currentFolder);
            });
            BackFolderCommand = ReactiveCommand.Create((FolderViewModel currentFolder) => {
                DirectoryNavigationVm.BackFolder(currentFolder);
            });
            BackFolderFromImageCommand = ReactiveCommand.Create((ImageViewModel imageVm) => {
                DirectoryNavigationVm.BackFolderFromImage(imageVm);
            });
            BackFolderFromDirectoryOptionsPanelCommand = ReactiveCommand.Create(() => {
                DirectoryNavigationVm.BackFolderFromDirectoryOptionsPanel();
            });
            ImportImagesCommand = ReactiveCommand.Create(async (FolderViewModel imageFolder) => {
                await ImportImagesVm.ImportImages(imageFolder);
            });
            AddFolderDescriptionCommand = ReactiveCommand.Create((FolderViewModel folderVm) => {
                ModifyFolderDataVm.UpdateFolder(folderVm, "Description");
            });
            AddFolderTagsCommand = ReactiveCommand.Create((FolderViewModel folderVm) => {
                ModifyFolderDataVm.AddFolderTag(folderVm);
            });
            EditFolderTagsCommand = ReactiveCommand.Create((FolderViewModel folderVm) => {
                ModifyFolderDataVm.EditFolderTag(folderVm);
            });
            AddFolderRatingCommand = ReactiveCommand.Create((FolderViewModel folderVm) => {
                ModifyFolderDataVm.UpdateFolder(folderVm, "Rating");
            });
            AddImageTagsCommand = ReactiveCommand.Create((ImageViewModel imageVm) => {
                ModifyImageDataVm.AddImageTag(imageVm);
            });
            AddMultipleImageTagsCommand = ReactiveCommand.Create((ListBox selectedTagsListBox) => {
                ModifyImageDataVm.AddMultipleImageTags(selectedTagsListBox);
            });
            EditImageTagsCommand = ReactiveCommand.Create((ImageViewModel imageVm) => {
                ModifyImageDataVm.EditImageTag(imageVm);
            });
            AddImageRatingCommand = ReactiveCommand.Create((ImageViewModel imageVm) => {
                ModifyImageDataVm.UpdateImage(imageVm, "Rating");
            });
            DeleteLibraryCommand = ReactiveCommand.Create(() => {
                DeleteLibrary();
            });
            OpenImageInExternalViewerCommand = ReactiveCommand.Create((ImageViewModel imageVm) => {
                ExternalProgramVm.OpenImageInExternalViewer(imageVm);
            });
            OpenCurrentDirectoryWithExplorerCommand = ReactiveCommand.Create(() => {
                ExternalProgramVm.OpenCurrentDirectoryWithExplorer();
            });
            MoveImageToTrashCommand = ReactiveCommand.Create(async (ImageViewModel imageVm) => {
                await MoveImages.MoveImageToTrash(imageVm);
            });
            MoveFolderToTrashCommand = ReactiveCommand.Create(async (FolderViewModel folderVm) => {
                await MoveFolderToTrash.MoveFolderToTrash(folderVm);
            });
            ScanFolderImagesForMetaDataCommand = ReactiveCommand.Create(async (FolderViewModel folderVm) => {
                await ScanImagesForMetaDataVm.ScanFolderImagesForMetaData(folderVm);
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
            MoveSelectedImagesToTrashCommand = ReactiveCommand.Create(async (ListBox imagesListBox) =>
            {
                await MoveImages.MoveSelectedImagesToTrash(imagesListBox);
            });
            SelectAllImagesCommand = ReactiveCommand.Create((ListBox imagesListBox) => {
                MoveImages.SelectAllImages(imagesListBox);
            });
            MoveSelectedImagesToNewFolderCommand = ReactiveCommand.Create(async (ListBox imagesListBox) => {
                await MoveImages.MoveSelectedImagesToNewFolder(imagesListBox);
            });
            ImportAllFoldersOnCurrentPageCommand = ReactiveCommand.Create(async (ItemsControl foldersItemsControl) => { 
                await ImportImagesVm.ImportAllFoldersOnCurrentPage(foldersItemsControl);
            });
            AddCoverImageOnCurrentPageCommand = ReactiveCommand.Create(async (ItemsControl folderItemsControl) => { 
                await CoverImageVm.AddCoverImageOnCurrentPage(folderItemsControl);
            });
            ScanAllFoldersOnCurrentPageCommand = ReactiveCommand.Create(async (ItemsControl foldersItemsControl) => {
                await ScanImagesForMetaDataVm.ScanAllFoldersOnCurrentPage(foldersItemsControl);
            });
            CopyCoverImageToContainingFolderCommand = ReactiveCommand.Create(async (FolderViewModel folderVm) => { 
                await CoverImageVm.CopyCoverImageToContainingFolder(folderVm);
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

        public DirectoryNavigationViewModel DirectoryNavigationVm { get; }
        public ModifyFolderDataViewModel ModifyFolderDataVm { get; }
        public ModifyImageDataViewModel ModifyImageDataVm { get; }
        public ExternalProgramViewModel ExternalProgramVm { get; }
        public CoverImageViewModel CoverImageVm { get; }
        public ScanImagesForMetaDataViewModel ScanImagesForMetaDataVm { get; }
        public ImportImagesViewModel ImportImagesVm { get; }
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

        public ReactiveCommand<ListBox, Task> MoveSelectedImagesToTrashCommand { get; }

        public ReactiveCommand<ListBox, Unit> SelectAllImagesCommand { get; }

        public ReactiveCommand<ListBox, Task> MoveSelectedImagesToNewFolderCommand { get; }

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
                //Parallel.ForEachAsync does not iterate in order. Need order preserved. 
                //so iterate over the correct count and store the results in order -- correct slot/index.
                //then re-iterate in order on the UIThread to display ordered results.
                FolderViewModel[] results = new FolderViewModel[displayFolders.Count];
                await Parallel.ForEachAsync(
                        Enumerable.Range(0, displayFolders.Count),
                        new ParallelOptions { MaxDegreeOfParallelism = 4 },
                        async(i, ct) => {
                            Folder taggedFolder = FolderMapper.MapTagsToFolder(displayFolders[i], displayFolderTags);
                            FolderViewModel folderViewModel = await FolderMapper.GetFolderVm(taggedFolder);
                            results[i] = folderViewModel;
                        });

                // This must be on the UI thread
                await Dispatcher.UIThread.InvokeAsync(() => {
                    foreach (FolderViewModel folderViewModel in results) 
                    { 
                        LibraryFolders.Add(folderViewModel);
                    }
                });
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
                //DB pull displayImages in the correct order I want to keep it
                //Parallel.ForEachAsync does not iterate in order. Need order preserved. 
                //so iterate over the correct count and store the results in order -- correct slot/index.
                //then re-iterate in order on the UIThread to display ordered results.
                ImageViewModel[] results = new ImageViewModel[displayImages.Count];
                await Parallel.ForEachAsync(
                    Enumerable.Range(0, displayImages.Count),
                    new ParallelOptions { MaxDegreeOfParallelism = 4 },
                    async (i, ct) =>
                    {
                        Image taggedImage = ImageMapper.MapTagsToImage(displayImages[i], displayImageTags);
                        ImageViewModel imageViewModel = await ImageMapper.GetImageVm(taggedImage);
                        results[i] = imageViewModel;
                    });
                // This must be on the UI thread
                await Dispatcher.UIThread.InvokeAsync(() =>
                {
                    foreach (ImageViewModel imageViewModel in results)
                    {
                        Images.Add(imageViewModel);
                    }
                });
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
    }
}

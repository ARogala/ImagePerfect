﻿using Avalonia.Media.Imaging;
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

namespace ImagePerfect.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly FolderMethods _folderMethods;
        private readonly ImageCsvMethods _imageCsvMethods;
        private readonly ImageMethods _imageMethods;
        private bool _showLoading;
        private bool _showFilters = false;
        private string _currentDirectory;
        private string _rootFolderLocation;
        private string _newFolderName;
        private bool _isNewFolderEnabled;
        private List<string> _tagsList = new List<string>();

        private List<Folder> displayFolders = new List<Folder>();
        private List<FolderTag> displayFolderTags = new List<FolderTag>();  
        private List<Image> displayImages = new List<Image>();
        private List<ImageTag> displayImageTags = new List<ImageTag>(); 
        private int folderPageSize = 4;
        private int _totalFolderPages = 1;
        private int _currentFolderPage = 1;

        private int imagePageSize = 20;
        private int _totalImagePages = 1;
        private int _currentImagePage = 1;

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
            FolderDescriptionFilter
        }
        private filters currentFilter = filters.None;
        private int selectedRatingForFilter = 0;

        public MainWindowViewModel() { }
        public MainWindowViewModel(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _folderMethods = new FolderMethods(_unitOfWork);
            _imageCsvMethods = new ImageCsvMethods(_unitOfWork);   
            _imageMethods = new ImageMethods(_unitOfWork);
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
            ImportImagesCommand = ReactiveCommand.Create((FolderViewModel imageFolder) => {
           
                ImportImages(imageFolder);
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
            MoveImageToTrashCommand = ReactiveCommand.Create((ImageViewModel imageVm) => { 
                MoveImageToTrash(imageVm);
            });
            MoveFolderToTrashCommand = ReactiveCommand.Create((FolderViewModel folderVm) => { 
                MoveFolderToTrash(folderVm);
            });
            ScanFolderImagesForMetaDataCommand = ReactiveCommand.Create((FolderViewModel folderVm) => { 
                ScanFolderImagesForMetaData(folderVm);
            });
            NextPageCommand = ReactiveCommand.Create(() => { 
                NextPage();
            });
            PreviousPageCommand = ReactiveCommand.Create(() => { 
                PreviousPage();
            });
            GoToPageCommand = ReactiveCommand.Create((decimal pageNumber) => {

                GoToPage(Decimal.ToInt32(pageNumber));
            });
            ToggleFiltersCommand = ReactiveCommand.Create(() => { 
                ToggleFilters();
            });
            FilterImagesOnRatingCommand = ReactiveCommand.Create(async (decimal rating) => {
                CurrentFolderPage = 1;
                TotalFolderPages = 1;
                CurrentImagePage = 1;
                TotalImagePages = 1;
                MaxCurrentPage = 1;
                MaxPage = 1;
                selectedRatingForFilter = Decimal.ToInt32(rating);
                await FilterImagesOnRating(selectedRatingForFilter);
            });
            FilterFoldersOnRatingCommand = ReactiveCommand.Create(async (decimal rating) =>
            {
                CurrentFolderPage = 1;
                TotalFolderPages = 1;
                CurrentImagePage = 1;
                TotalImagePages = 1;
                MaxCurrentPage = 1;
                MaxPage = 1;
                selectedRatingForFilter = Decimal.ToInt32(rating);
                await FilterFoldersOnRating(selectedRatingForFilter);
            });
            //CreateNewFolderCommand = ReactiveCommand.Create(() => { CreateNewFolder(); });
            Initialize();
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
        public int TotalFolderPages
        {
            get => _totalFolderPages;
            set => this.RaiseAndSetIfChanged(ref _totalFolderPages, value);
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
                if (value == "" || CurrentDirectory == RootFolderLocation) 
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
            set => _currentDirectory = value;
        }

        public string RootFolderLocation
        {
            get => _rootFolderLocation;
            set => _rootFolderLocation = value;
        }
        public PickRootFolderViewModel PickRootFolder { get => new PickRootFolderViewModel(_unitOfWork, LibraryFolders); }

        public PickNewFoldersViewModel PickNewFolders { get => new PickNewFoldersViewModel(_unitOfWork, LibraryFolders); }

        public PickMoveToFolderViewModel PickMoveToFolder { get => new PickMoveToFolderViewModel(_unitOfWork, LibraryFolders); }

        public PickFolderCoverImageViewModel PickCoverImage { get => new PickFolderCoverImageViewModel(_unitOfWork, LibraryFolders); }

        public ObservableCollection<FolderViewModel> LibraryFolders { get; } = new ObservableCollection<FolderViewModel>();

        public ObservableCollection<ImageViewModel> Images { get; } = new ObservableCollection<ImageViewModel>();

        public ReactiveCommand<FolderViewModel, Unit> NextFolderCommand { get; }

        public ReactiveCommand<FolderViewModel,Unit> BackFolderCommand { get; }

        public ReactiveCommand<ImageViewModel, Unit> BackFolderFromImageCommand { get; }

        public ReactiveCommand<FolderViewModel, Unit> ImportImagesCommand { get; }

        public ReactiveCommand<FolderViewModel, Unit> AddFolderDescriptionCommand { get; }

        public ReactiveCommand<FolderViewModel, Unit> AddFolderTagsCommand { get; }

        public ReactiveCommand<FolderViewModel, Unit> EditFolderTagsCommand { get; }

        public ReactiveCommand<FolderViewModel, Unit> AddFolderRatingCommand { get; }

        public ReactiveCommand<ImageViewModel, Unit> AddImageTagsCommand { get; }

        public ReactiveCommand<ImageViewModel, Unit> EditImageTagsCommand { get; }

        public ReactiveCommand<ImageViewModel, Unit> AddImageRatingCommand { get; }

        public ReactiveCommand<Unit, Unit> DeleteLibraryCommand { get; }

        public ReactiveCommand<ImageViewModel, Unit> OpenImageInExternalViewerCommand { get; }

        public ReactiveCommand<ImageViewModel, Unit> MoveImageToTrashCommand { get; }

        public ReactiveCommand<FolderViewModel, Unit> MoveFolderToTrashCommand { get; }

        public ReactiveCommand<FolderViewModel, Unit> ScanFolderImagesForMetaDataCommand { get; }

        public ReactiveCommand<Unit, Unit> NextPageCommand { get; }

        public ReactiveCommand<Unit, Unit> PreviousPageCommand { get; }

        public ReactiveCommand<decimal, Unit> GoToPageCommand { get; }

        public ReactiveCommand<Unit, Unit> ToggleFiltersCommand { get; }

        public ReactiveCommand<decimal, Task> FilterImagesOnRatingCommand { get; }

        public ReactiveCommand<decimal, Task> FilterFoldersOnRatingCommand { get; }

        //public ReactiveCommand<Unit, Unit> CreateNewFolderCommand { get; }

        public async Task FilterFoldersOnRating(int rating)
        {
            currentFilter = filters.FolderRatingFilter;
            (List<Folder> folders, List<FolderTag> tags) folderResult = await _folderMethods.GetAllFoldersAtRating(rating);
            displayFolders = folderResult.folders;
            displayFolderTags = folderResult.tags;

            Images.Clear();
            LibraryFolders.Clear();
            displayFolders = FolderPagination();
            for (int i = 0; i < displayFolders.Count; i++)
            {
                //need to map tags to folders 
                displayFolders[i] = FolderMapper.MapTagsToFolder(displayFolders[i], displayFolderTags);
                FolderViewModel folderViewModel = await FolderMapper.GetFolderVm(displayFolders[i]);
                LibraryFolders.Add(folderViewModel);
            }
        }
        public async Task FilterImagesOnRating(int rating)
        {
            currentFilter = filters.ImageRatingFilter;
            (List<Image> images, List<ImageTag> tags) imageResult = await _imageMethods.GetAllImagesAtRating(rating);
            displayImages = imageResult.images;
            displayImageTags = imageResult.tags;

            Images.Clear();
            LibraryFolders.Clear();
            displayImages = ImagePagination();
            for (int i = 0; i < displayImages.Count; i++)
            {
                //need to map tags to images
                displayImages[i] = ImageMapper.MapTagsToImage(displayImages[i], displayImageTags);
                ImageViewModel imageViewModel = await ImageMapper.GetImageVm(displayImages[i]);
                Images.Add(imageViewModel);
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
        private List<Image> ImagePagination()
        {
            //same as FolderPagination
            int offset = imagePageSize * (CurrentImagePage -1);
            int totalImageCount = displayImages.Count;
            if(totalImageCount == 0 || totalImageCount <= imagePageSize)
                return displayImages;
            TotalImagePages = (int)Math.Ceiling(totalImageCount / (double)imagePageSize);
            List<Image> displayImagesTemp;
            if(CurrentImagePage == TotalImagePages)
            {
                displayImagesTemp = displayImages.GetRange(offset, (totalImageCount - (TotalImagePages - 1)*imagePageSize));
            }
            else
            {
                displayImagesTemp = displayImages.GetRange(offset, imagePageSize);
            }
            MaxPage = Math.Max(TotalImagePages, TotalFolderPages);
            MaxCurrentPage = Math.Max(CurrentImagePage, CurrentFolderPage);
            return displayImagesTemp;
        }
        private List<Folder> FolderPagination()
        {
            /* Example
             * folderPageSize = 10
             * offest = 10*1 for page = 2
             * totalFolderCount = 14
             * TotalFolderPages = 2
             */
            int offest = folderPageSize * (CurrentFolderPage - 1);
            int totalFolderCount = displayFolders.Count;
            if (totalFolderCount == 0 || totalFolderCount <= folderPageSize) 
                return displayFolders; 
            TotalFolderPages = (int)Math.Ceiling(totalFolderCount / (double)folderPageSize);
            List<Folder> displayFoldersTemp;
            if (CurrentFolderPage == TotalFolderPages)
            {
                //on last page GetRange count CANNOT be folderPageSize or index out of range 
                //thus following logical example above in a array of 14 elements the range count on the last page is 14 - 10
                //formul used: totalFolderCount - ((TotalFolderPages - 1)*folderPageSize)
                //folderCount minus total folders on all but last page
                //14 - 10
                displayFoldersTemp = displayFolders.GetRange(offest, (totalFolderCount - (TotalFolderPages - 1)*folderPageSize));
            }
            else
            {
                displayFoldersTemp = displayFolders.GetRange(offest, folderPageSize);
            }
            MaxPage = Math.Max(TotalImagePages, TotalFolderPages);
            MaxCurrentPage = Math.Max(CurrentImagePage, CurrentFolderPage);
            return displayFoldersTemp;
        }

        private async Task RefreshFolders(string path)
        {
            currentFilter = filters.None;
            (List<Folder> folders, List<FolderTag> tags) folderResult = await _folderMethods.GetFoldersInDirectory(path);
            displayFolders = folderResult.folders;
            displayFolderTags = folderResult.tags;
            LibraryFolders.Clear();
            displayFolders = FolderPagination();
            for (int i = 0; i < displayFolders.Count; i++)
            {
                //need to map tags to folders 
                displayFolders[i] = FolderMapper.MapTagsToFolder(displayFolders[i], displayFolderTags);
                FolderViewModel folderViewModel = await FolderMapper.GetFolderVm(displayFolders[i]);
                LibraryFolders.Add(folderViewModel);
            }
        }

        private async Task RefreshFolderProps(string path)
        {
            currentFilter = filters.None;
            (List<Folder> folders, List<FolderTag> tags) folderResult = await _folderMethods.GetFoldersInDirectory(path);
            displayFolders = folderResult.folders;
            displayFolderTags = folderResult.tags;
            displayFolders = FolderPagination();
            for (int i = 0; i < displayFolders.Count; i++) 
            {
                //need to map tags to folders 
                displayFolders[i] = FolderMapper.MapTagsToFolder(displayFolders[i], displayFolderTags);
                FolderViewModel folderViewModel = await FolderMapper.GetFolderVm(displayFolders[i]);
                //will be in the same order unless delete/move or next back folder
                //Any non destructive operation that does not affect the number or order of items returned from
                //the sql query will be in the same order so just modify props for a much cleaner UI refresh
                LibraryFolders[i] = folderViewModel;
            }
        }
        private async Task RefreshImages(string path = "", int folderId = 0)
        {
            currentFilter = filters.None;
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
            for(int i = 0; i < displayImages.Count; i++)
            {
                //need to map tags to images
                displayImages[i] = ImageMapper.MapTagsToImage(displayImages[i], displayImageTags);
                ImageViewModel imageViewModel = await ImageMapper.GetImageVm(displayImages[i]);
                Images.Add(imageViewModel);
            }
        }

        private async Task RefreshImageProps(string path = "", int folderId = 0)
        {
            currentFilter = filters.None;
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
            for (int i = 0; i < displayImages.Count; i++)
            {
                //need to map tags to images
                displayImages[i] = ImageMapper.MapTagsToImage(displayImages[i], displayImageTags);
                ImageViewModel imageViewModel = await ImageMapper.GetImageVm(displayImages[i]);
                Images[i] = imageViewModel;
            }
        }
        private async void Initialize()
        {
            await GetRootFolder();
            await GetTagsList();
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

        private async void ImportImages(FolderViewModel imageFolder)
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
                ShowLoading = false;
                //remove one folder from path
                newPath = PathHelper.RemoveOneFolderFromPath(imageFolderPath);
                //refresh UI
                await RefreshFolderProps(newPath);
            }
        }

        //opens the previous directory location -- from image button
        private async void BackFolderFromImage(ImageViewModel imageVm)
        {
            //not ideal but keeps pagination to the folder your in. When you go back or next start from page 1
            CurrentFolderPage = 1;
            TotalFolderPages = 1;
            CurrentImagePage = 1;
            TotalImagePages = 1;
            MaxCurrentPage = 1;
            MaxPage = 1;
            /*
                Similar to Back folders except these buttons are on the image and we only need to remove one folder
                Not every folder has a folder so this is the quickest way for now to back out of a folder that only has images
             */
            string newPath = PathHelper.RemoveOneFolderFromPath(imageVm.ImageFolderPath);
            //set the current directory -- used to add new folder to location
            CurrentDirectory = newPath;
            //refresh UI
            await RefreshFolders(newPath);
            await RefreshImages(newPath);
        }

        //opens the previous directory location
        private async void BackFolder(FolderViewModel currentFolder)
        {
            CurrentFolderPage = 1;
            TotalFolderPages = 1;
            CurrentImagePage = 1;
            TotalImagePages = 1;
            MaxCurrentPage = 1;
            MaxPage = 1;
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
            await RefreshFolders(newPath);
            await RefreshImages(newPath);
        }

        //loads the previous X elements in CurrentDirectory
        private async void PreviousPage()
        {
            switch (currentFilter)
            {
                case filters.None:
                    if (CurrentFolderPage > 1)
                    {
                        CurrentFolderPage = CurrentFolderPage - 1;
                        await RefreshFolders(CurrentDirectory);
                    }
                    if (CurrentImagePage > 1)
                    {
                        CurrentImagePage = CurrentImagePage - 1;
                        await RefreshImages(CurrentDirectory);
                    }
                    break;
                case filters.ImageRatingFilter:
                    if (CurrentImagePage > 1)
                    {
                        CurrentImagePage = CurrentImagePage - 1;
                        await FilterImagesOnRating(selectedRatingForFilter);
                    }
                    break;
                case filters.FolderRatingFilter:
                    if (CurrentFolderPage > 1)
                    {
                        CurrentFolderPage = CurrentFolderPage - 1;
                        await FilterFoldersOnRating(selectedRatingForFilter);
                    }
                    break;
                case filters.ImageTagFilter:
                    break;
                case filters.FolderTagFilter: 
                    break;
                case filters.FolderDescriptionFilter: 
                    break;
            }
        }

        //opens the next directory locaion
        private async void NextFolder(FolderViewModel currentFolder)
        {
            CurrentFolderPage = 1;
            TotalFolderPages = 1;
            CurrentImagePage = 1;
            TotalImagePages = 1;
            MaxCurrentPage = 1;
            MaxPage = 1;
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
                await RefreshFolders(currentFolder.FolderPath);
                await RefreshImages("", currentFolder.FolderId);
            }  
        }

        //loads the next X elements in CurrentDirectory
        private async void NextPage()
        {
            switch (currentFilter)
            {
                case filters.None:
                    if (CurrentFolderPage < TotalFolderPages)
                    {
                        CurrentFolderPage = CurrentFolderPage + 1;
                        await RefreshFolders(CurrentDirectory);
                    }
                    if (CurrentImagePage < TotalImagePages)
                    {
                        CurrentImagePage = CurrentImagePage + 1;
                        await RefreshImages(CurrentDirectory);
                    }
                    break;
                case filters.ImageRatingFilter:
                    if (CurrentImagePage < TotalImagePages)
                    {
                        CurrentImagePage = CurrentImagePage + 1;
                        await FilterImagesOnRating(selectedRatingForFilter);
                    }
                    break;
                case filters.FolderRatingFilter:
                    if (CurrentFolderPage < TotalFolderPages)
                    {
                        CurrentFolderPage = CurrentFolderPage + 1;
                        await FilterFoldersOnRating(selectedRatingForFilter);
                    }
                    break;
                case filters.ImageTagFilter:
                    break;
                case filters.FolderTagFilter:
                    break;
                case filters.FolderDescriptionFilter:
                    break;
            }
        }

        private async void GoToPage(int pageNumber)
        {
            switch (currentFilter)
            {
                case filters.None:
                    if (pageNumber <= TotalFolderPages)
                    {
                        CurrentFolderPage = pageNumber;
                        await RefreshFolders(CurrentDirectory);
                    }
                    if (pageNumber <= TotalImagePages)
                    {
                        CurrentImagePage = pageNumber;
                        await RefreshImages(CurrentDirectory);
                    }
                    break;
                case filters.ImageRatingFilter:
                    if (pageNumber <= TotalImagePages)
                    {
                        CurrentImagePage = pageNumber;
                        await FilterImagesOnRating(selectedRatingForFilter);
                    }
                    break;
                case filters.FolderRatingFilter:
                    if (pageNumber <= TotalFolderPages)
                    {
                        CurrentFolderPage = pageNumber;
                        await FilterFoldersOnRating(selectedRatingForFilter);
                    }
                    break;
                case filters.ImageTagFilter:
                    break;
                case filters.FolderTagFilter:
                    break;
                case filters.FolderDescriptionFilter:
                    break;
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
            //refresh UI
            await RefreshFolderProps(CurrentDirectory);
        }
        private async void AddFolderTag(FolderViewModel folderVm)
        {
            //click submit with empty input just return
            if (folderVm.NewTag == "" || folderVm.NewTag == null)
            {
                return;
            }
            Folder folder = FolderMapper.GetFolderFromVm(folderVm);
            //update folder table and tags table in db
            bool success = await _folderMethods.UpdateFolderTags(folder, folderVm.NewTag);
            if (success)
            {
                //Update TagsList to show in UI AutoCompleteBox clear NewTag in box as well and refresh folders to show new tag
                await GetTagsList();
                folderVm.NewTag = "";
                //refresh UI
                await RefreshFolderProps(CurrentDirectory);
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
            //refresh UI
            await RefreshImageProps(CurrentDirectory);
        }
        //update ImageTags in db, and update image metadata
        private async void AddImageTag(ImageViewModel imageVm)
        {
            //click submit with empty input just return
            if(imageVm.NewTag == "" || imageVm.NewTag == null)
            {
                return;
            }
            //add NewTag to ImageTags
            if (imageVm.ImageTags == "")
            {
                imageVm.ImageTags = imageVm.NewTag;
            }
            else
            {
                imageVm.ImageTags = imageVm.ImageTags + "," + imageVm.NewTag;
            }
            Image image = ImageMapper.GetImageFromVm(imageVm);
            //update image table and tags table in db
            bool success = await _imageMethods.UpdateImageTags(image, imageVm.NewTag);
            if (success) 
            {
                //write new tag to image metadata
                await ImageMetaDataHelper.WriteTagToImage(imageVm);
                //Update TagsList to show in UI AutoCompleteBox clear NewTag in box as well
                await GetTagsList();
                imageVm.NewTag = "";
                await RefreshImageProps(CurrentDirectory);   
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

        private async void MoveImageToTrash(ImageViewModel imageVm)
        {
           
            var boxYesNo = MessageBoxManager.GetMessageBoxStandard("Delete Image", "Are you sure you want to delete your image?", ButtonEnum.YesNo);
            var boxResult = await boxYesNo.ShowAsync();
            if (boxResult == ButtonResult.Yes) 
            {
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
                    }
                }
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
                string pathThatContainsFolder = PathHelper.RemoveOneFolderFromPath(folderVm.FolderPath);
                (List<Image> images, List<ImageTag> tags) imageResult = await _imageMethods.GetAllImagesInFolder(pathThatContainsFolder);
                displayImages = imageResult.images;
                (List<Folder> folders, List<FolderTag> tags) folderResultA = await _folderMethods.GetFoldersInDirectory(pathThatContainsFolder);
                displayFolders = folderResultA.folders;
                if (displayFolders.Count == 1 && displayImages.Count == 0)
                {
                    var box = MessageBoxManager.GetMessageBoxStandard("Delete Folder", "This is the last folder in the current directory go back and delete the root folder", ButtonEnum.Ok);
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
                if (Directory.Exists(folderVm.FolderPath))
                {
                    //delete folder from db -- does not delete sub folders.
                    bool success = await _folderMethods.DeleteFolder(folderVm.FolderId);
                    if (success) 
                    {
                        //move folder to trash folder
                        string newFolderPath = PathHelper.GetFolderTrashPath(folderVm, trashFolderPath);
                        Directory.Move(folderVm.FolderPath, newFolderPath);

                        //refresh UI
                        await RefreshFolders(pathThatContainsFolder);
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
        private async void ScanFolderImagesForMetaData(FolderViewModel folderVm)
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
                //refresh UI
                await RefreshFolderProps(CurrentDirectory);
            }
            ShowLoading = false;
        }
   
        /*
         A bit too much complexity at the moment. For now to add a new folder with images make that in the filesystem and use
         the current add new folders method. At some point i want to add this along with moving images.
         */
        //private async void CreateNewFolder()
        //{
        //    //first check if directory exists
        //    string newFolderPath = PathHelper.GetNewFolderPath(CurrentDirectory, NewFolderName);
        //    if (Directory.Exists(newFolderPath))
        //    {
        //        var box = MessageBoxManager.GetMessageBoxStandard("New Folder", "A folder with this name already exists.", ButtonEnum.Ok);
        //        await box.ShowAsync();
        //        return;
        //    }
        //    //add dir to database -- also need to update parent folders HasChildren bool value
        //    Folder newFolder = new Folder
        //    {
        //        FolderName = NewFolderName,
        //        FolderPath = newFolderPath,
        //        HasChildren = false,
        //        CoverImagePath = "",
        //        FolderRating = 0,
        //        HasFiles = false,
        //        IsRoot = false,
        //        FolderContentMetaDataScanned = false,
        //        AreImagesImported = false,
        //    };
        //    bool success = await _folderMethods.CreateNewFolder(newFolder);

        //    //create on disk
        //    if (success)
        //    {
        //        try
        //        {
        //            Directory.CreateDirectory(newFolderPath);
        //            //refresh UI
        //            List<Folder> folders = await _folderMethods.GetFoldersInDirectory(CurrentDirectory);
        //            LibraryFolders.Clear();
        //            foreach (Folder folder in folders)
        //            {
        //                FolderViewModel folderViewModel = await FolderMapper.GetFolderVm(folder);
        //                LibraryFolders.Add(folderViewModel);
        //            }
        //        }
        //        catch (Exception e) 
        //        {
        //            var box = MessageBoxManager.GetMessageBoxStandard("New Folder", $"Error {e}.", ButtonEnum.Ok);
        //            await box.ShowAsync();
        //            return;
        //        }
        //    }   
        //}


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

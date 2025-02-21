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

namespace ImagePerfect.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly FolderMethods _folderMethods;
        private readonly ImageCsvMethods _imageCsvMethods;
        private readonly ImageMethods _imageMethods;
        private bool _showLoading;
        private string _currentDirectory;
        private string _rootFolderLocation;
        private string _newFolderName;
        private bool _isNewFolderEnabled;
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
                UpdateFolder(folderVm, "Tags");
            });
            AddFolderRatingCommand = ReactiveCommand.Create((FolderViewModel folderVm) => {
                UpdateFolder(folderVm, "Rating");
            });
            AddImageTagsCommand = ReactiveCommand.Create((ImageViewModel imageVm) => {
                UpdateImage(imageVm, "Tags");
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
            CreateNewFolderCommand = ReactiveCommand.Create(() => { CreateNewFolder(); });
            GetRootFolder();
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

        public ReactiveCommand<FolderViewModel, Unit> AddFolderRatingCommand { get; }

        public ReactiveCommand<ImageViewModel, Unit> AddImageTagsCommand { get; }

        public ReactiveCommand<ImageViewModel, Unit> AddImageRatingCommand { get; }

        public ReactiveCommand<Unit, Unit> DeleteLibraryCommand { get; }

        public ReactiveCommand<ImageViewModel, Unit> OpenImageInExternalViewerCommand { get; }

        public ReactiveCommand<ImageViewModel, Unit> MoveImageToTrashCommand { get; }

        public ReactiveCommand<FolderViewModel, Unit> MoveFolderToTrashCommand { get; }

        public ReactiveCommand<FolderViewModel, Unit> ScanFolderImagesForMetaDataCommand { get; }

        public ReactiveCommand<Unit, Unit> CreateNewFolderCommand { get; }

        private async void GetRootFolder()
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

        private async void ImportImages(FolderViewModel imageFolder)
        {
            List<Folder> folders = new List<Folder>();
            List<Image> images = new List<Image>();
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
                folders = await _folderMethods.GetFoldersInDirectory(newPath);
                //folder may or may not have images but will just be an empty list if none.
                images = await _imageMethods.GetAllImagesInFolder(newPath);
                LibraryFolders.Clear();
                Images.Clear();
                foreach (Folder folder in folders)
                {
                    FolderViewModel folderViewModel = await FolderMapper.GetFolderVm(folder);
                    LibraryFolders.Add(folderViewModel);
                }
                foreach (Image image in images)
                {
                    ImageViewModel imageViewModel = await ImageMapper.GetImageVm(image);
                    Images.Add(imageViewModel);
                }

            }
        }

        private async void BackFolderFromImage(ImageViewModel imageVm)
        {
            /*
                Similar to Back folders except these buttons are on the image and we only need to remove one folder
                Not every folder has a folder so this is the quickest way for now to back out of a folder that only has images
             */
            List<Folder> folders = new List<Folder>();
            List<Image> images = new List<Image>();
            string newPath = PathHelper.RemoveOneFolderFromPath(imageVm.ImageFolderPath);
            //set the current directory -- used to add new folder to location
            CurrentDirectory = newPath;
            folders = await _folderMethods.GetFoldersInDirectory(newPath);
            //folder may or may not have images but will just be an empty list if none.
            images = await _imageMethods.GetAllImagesInFolder(newPath);
            LibraryFolders.Clear();
            Images.Clear();
            foreach (Folder folder in folders)
            {
                FolderViewModel folderViewModel = await FolderMapper.GetFolderVm(folder);
                LibraryFolders.Add(folderViewModel);
            }
            foreach (Image image in images)
            {
                ImageViewModel imageViewModel = await ImageMapper.GetImageVm(image);
                Images.Add(imageViewModel);
            }
        }

        private async void BackFolder(FolderViewModel currentFolder)
        {
            /*
                tough to see but basically you need to remove two folders to build the regexp string
                example if you are in /pictures/hiking/bearmountian and bearmountain folder has another folder saturday_2025_05_25
                you will be clicking on the back button of folder /pictures/hiking/bearmountian/saturday_2025_05_25 -- that wil be the FolderPath
                but you want to go back to hiking so you must remove two folders to get /pictures/hiking/
             */
            List<Folder> folders = new List<Folder>();
            List<Image> images = new List<Image>();
            string newPath = PathHelper.RemoveTwoFoldersFromPath(currentFolder.FolderPath);
            //set the current directory -- used to add new folder to location
            CurrentDirectory = newPath;
            folders = await _folderMethods.GetFoldersInDirectory(newPath);
            //folder may or may not have images but will just be an empty list if none.
            images = await _imageMethods.GetAllImagesInFolder(newPath);
            LibraryFolders.Clear();
            Images.Clear();
            foreach (Folder folder in folders)
            {
                FolderViewModel folderViewModel = await FolderMapper.GetFolderVm(folder);
                LibraryFolders.Add(folderViewModel);
            }
            foreach(Image image in images)
            {
                ImageViewModel imageViewModel = await ImageMapper.GetImageVm(image);
                Images.Add(imageViewModel);
            }
        }

        private async void NextFolder(FolderViewModel currentFolder)
        {
            List<Folder> folders = new List<Folder>();
            List<Image> images = new List<Image>();
            bool hasChildren = currentFolder.HasChildren;
            bool hasFiles = currentFolder.HasFiles;
            //set the current directory -- used to add new folder to location
            CurrentDirectory = currentFolder.FolderPath;
            //two boolean varibale 4 combos TF TT FT and FF
            if (hasChildren == true && hasFiles == false) 
            {
                folders = await _folderMethods.GetFoldersInDirectory(currentFolder.FolderPath);
            }
            else if (hasChildren == true && hasFiles == true)
            {
                //get folders and images
                folders = await _folderMethods.GetFoldersInDirectory(currentFolder.FolderPath);
                images = await _imageMethods.GetAllImagesInFolder(currentFolder.FolderId);
                
            }
            else if(hasChildren == false && hasFiles == true)
            {
                //get images
                images = await _imageMethods.GetAllImagesInFolder(currentFolder.FolderId);
            }
            else
            {
                var box = MessageBoxManager.GetMessageBoxStandard("Empty Folder", "There are no Images in this folder.", ButtonEnum.Ok);
                await box.ShowAsync();
                return;
            }
            LibraryFolders.Clear();
            Images.Clear();
            foreach (Folder folder in folders) 
            {
                FolderViewModel folderViewModel = await FolderMapper.GetFolderVm(folder);
                LibraryFolders.Add(folderViewModel);
            }
            foreach(Image image in images)
            {
                ImageViewModel imageViewModel = await ImageMapper.GetImageVm(image);
                Images.Add(imageViewModel);
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
            var result = await boxYesNo.ShowAsync();
            if (result == ButtonResult.Yes) 
            {
                List<Folder> folders = await _folderMethods.GetFoldersInDirectory(imageVm.ImageFolderPath);
                List<Image> images = await _imageMethods.GetAllImagesInFolder(imageVm.FolderId);
                if (images.Count == 1 && folders.Count == 0)
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
                        images = await _imageMethods.GetAllImagesInFolder(imageVm.FolderId);
                        Images.Clear();
                        foreach (Image image in images)
                        {
                            ImageViewModel imageViewModel = await ImageMapper.GetImageVm(image);
                            Images.Add(imageViewModel);
                        }
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
            var result = await boxYesNo.ShowAsync();
            if (result == ButtonResult.Yes) 
            {
                string pathThatContainsFolder = PathHelper.RemoveOneFolderFromPath(folderVm.FolderPath);
                List<Image> images = await _imageMethods.GetAllImagesInFolder(pathThatContainsFolder);
                List<Folder> folders = await _folderMethods.GetFoldersInDirectory(pathThatContainsFolder);
                if (folders.Count == 1 && images.Count == 0)
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
                        folders = await _folderMethods.GetFoldersInDirectory(pathThatContainsFolder);
                        LibraryFolders.Clear();
                        foreach (Folder folder in folders)
                        {
                            FolderViewModel folderViewModel = await FolderMapper.GetFolderVm(folder);
                            LibraryFolders.Add(folderViewModel);
                        }
                    }
                }
            }
            
        }

        private async void ScanFolderImagesForMetaData(FolderViewModel folderVm)
        {
            //get all images at folder id
            List<Image> images = await _imageMethods.GetAllImagesInFolder(folderVm.FolderId);

            //scan images for metadata
            ImageMetaDataHelper.ScanImagesForMetaData(images);
            //add and or update database with metadata
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
            //add dir to datbase
            Folder newFolder = new Folder
            {
                FolderName = NewFolderName,
                FolderPath = newFolderPath,
                HasChildren = false,
                CoverImagePath = "",
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
                    List<Folder> folders = await _folderMethods.GetFoldersInDirectory(CurrentDirectory);
                    LibraryFolders.Clear();
                    foreach (Folder folder in folders)
                    {
                        FolderViewModel folderViewModel = await FolderMapper.GetFolderVm(folder);
                        LibraryFolders.Add(folderViewModel);
                    }
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

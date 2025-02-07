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

namespace ImagePerfect.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly FolderMethods _folderMethods;
        private readonly ImageCsvMethods _imageCsvMethods;
        private readonly ImageMethods _imageMethods;
        private bool _showLoading;
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
                AddFolderDescription(folderVm);
            });
            AddFolderTagsCommand = ReactiveCommand.Create((FolderViewModel folderVm) => { 
                AddFolderTags(folderVm);
            });
            AddFolderRatingCommand = ReactiveCommand.Create((FolderViewModel folderVm) => { 
                AddFolderRating(folderVm);
            });
            AddImageTagsCommand = ReactiveCommand.Create((ImageViewModel imageVm) => { 
                AddImageTags(imageVm);
            });
            DeleteLibraryCommand = ReactiveCommand.Create(() => {
                DeleteLibrary();
            });
            GetRootFolder();
        }
        public bool ShowLoading
        {
            get => _showLoading;
            set => this.RaiseAndSetIfChanged(ref _showLoading, value);
        }
        public PickRootFolderViewModel PickRootFolder { get => new PickRootFolderViewModel(_unitOfWork, LibraryFolders); }

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

        public ReactiveCommand<Unit, Unit> DeleteLibraryCommand { get; }
        private async void GetRootFolder()
        {
            Folder? rootFolder = await _folderMethods.GetRootFolder();
            if (rootFolder != null) 
            {
                FolderViewModel rootFolderVm = await FolderMapper.GetFolderVm(rootFolder);
                LibraryFolders.Add(rootFolderVm);
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
                folders = await _folderMethods.GetFoldersInDirectory(PathHelper.GetRegExpString(newPath));
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
            folders = await _folderMethods.GetFoldersInDirectory(PathHelper.GetRegExpString(newPath));
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

            folders = await _folderMethods.GetFoldersInDirectory(PathHelper.GetRegExpString(newPath));
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
            //two boolean varibale 4 combos TF TT FT and FF
            if (hasChildren == true && hasFiles == false) 
            {
                folders = await _folderMethods.GetFoldersInDirectory(PathHelper.GetRegExpString(currentFolder.FolderPath));
            }
            else if (hasChildren == true && hasFiles == true)
            {
                //get folders and images
                folders = await _folderMethods.GetFoldersInDirectory(PathHelper.GetRegExpString(currentFolder.FolderPath));
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
        private async void AddFolderDescription(FolderViewModel folderVm)
        {
            Folder folder = FolderMapper.GetFolderFromVm(folderVm);
            bool success = await _folderMethods.AddFolderDescription(folder);
            if (success) 
            {
                var box = MessageBoxManager.GetMessageBoxStandard("Add Description", "Folder Description updated successfully.", ButtonEnum.Ok);
                await box.ShowAsync();
                return;
            }
            else
            {
                var box = MessageBoxManager.GetMessageBoxStandard("Add Description", "Folder Description update error. Try again.", ButtonEnum.Ok);
                await box.ShowAsync();
                return;
            }
        }
        private async void AddFolderTags(FolderViewModel folderVm)
        {
            Folder folder = FolderMapper.GetFolderFromVm(folderVm);
            bool success = await _folderMethods.AddFolderTags(folder);
            if (success)
            {
                var box = MessageBoxManager.GetMessageBoxStandard("Add Tags", "Folder Tags updated successfully.", ButtonEnum.Ok);
                await box.ShowAsync();
                return;
            }
            else
            {
                var box = MessageBoxManager.GetMessageBoxStandard("Add Tags", "Folder Tags update error. Try again.", ButtonEnum.Ok);
                await box.ShowAsync();
                return;
            }
        }

        private async void AddFolderRating(FolderViewModel folderVm)
        {
            Folder folder = FolderMapper.GetFolderFromVm(folderVm);
            bool success = await _folderMethods.AddFolderRating(folder);
            if (success)
            {
                var box = MessageBoxManager.GetMessageBoxStandard("Add Rating", "Folder Rating updated successfully.", ButtonEnum.Ok);
                await box.ShowAsync();
                return;
            }
            else
            {
                var box = MessageBoxManager.GetMessageBoxStandard("Add Rating", "Folder Rating update error. Try again.", ButtonEnum.Ok);
                await box.ShowAsync();
                return;
            }
        }
        private async void AddImageTags(ImageViewModel imageVm)
        {

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

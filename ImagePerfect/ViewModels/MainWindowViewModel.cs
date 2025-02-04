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
using System.IO;
using System.Reactive;

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
            ImportImagesCommand = ReactiveCommand.Create((FolderViewModel imageFolder) => {
           
                ImportImages(imageFolder.FolderPath, imageFolder.FolderId);
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

        public ReactiveCommand<FolderViewModel, Unit> ImportImagesCommand { get; }

        public ReactiveCommand<Unit, Unit> DeleteLibraryCommand { get; }
        private async void GetRootFolder()
        {
            Folder? rootFolder = await _folderMethods.GetRootFolder();
            if (rootFolder != null) 
            {
                FolderViewModel rootFolderVm = new()
                {
                    FolderId = rootFolder.FolderId,
                    FolderName = rootFolder.FolderName,
                    FolderPath = rootFolder.FolderPath,
                    HasChildren = rootFolder.HasChildren,
                    CoverImagePath = rootFolder.CoverImagePath == "" ? ImageHelper.LoadFromResource(new Uri("avares://ImagePerfect/Assets/icons8-folder-600.png")) : await ImageHelper.FormatImage(rootFolder.CoverImagePath),
                    FolderDescription = rootFolder.FolderDescription,
                    FolderTags = rootFolder.FolderTags,
                    FolderRating = rootFolder.FolderRating,
                    HasFiles = rootFolder.HasFiles,
                    IsRoot = rootFolder.IsRoot,
                    FolderContentMetaDataScanned = rootFolder.FolderContentMetaDataScanned,
                };
                LibraryFolders.Add(rootFolderVm);
            }
        }
        private async void ImportImages(string imageFolderPath, int imageFolderId)
        {
            ShowLoading = true;
            //build csv
            bool csvIsSet = await ImageCsvMethods.BuildImageCsv(imageFolderPath, imageFolderId);
            //write csv to database
            if (csvIsSet) 
            {
                await _imageCsvMethods.AddImageCsv();
                ShowLoading = false;
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
                folders = await _folderMethods.NextFolder(currentFolder.FolderPath.Replace(@"\", @"\\\\") + @"\\\\[^\\\\]+\\\\?$");
            }
            else if (hasChildren == true && hasFiles == true)
            {
                //get folders and images
                folders = await _folderMethods.NextFolder(currentFolder.FolderPath.Replace(@"\", @"\\\\") + @"\\\\[^\\\\]+\\\\?$");
                images = await _imageMethods.GetAllImagesInFolder(currentFolder.FolderId);
                
            }
            else if(hasChildren == false && hasFiles == true)
            {
                //get images
                images = await _imageMethods.GetAllImagesInFolder(currentFolder.FolderId);
            }
            else
            {
                return;
            }
            LibraryFolders.Clear();
            Images.Clear();
            foreach (Folder folder in folders) 
            {
                FolderViewModel folderViewModel = new()
                {
                    FolderId = folder.FolderId,
                    FolderName = folder.FolderName,
                    FolderPath = folder.FolderPath,
                    HasChildren = folder.HasChildren,
                    CoverImagePath = folder.CoverImagePath == "" ? ImageHelper.LoadFromResource(new Uri("avares://ImagePerfect/Assets/icons8-folder-600.png")) : await ImageHelper.FormatImage(folder.CoverImagePath),
                    FolderDescription = folder.FolderDescription,
                    FolderTags = folder.FolderTags,
                    FolderRating = folder.FolderRating,
                    HasFiles = folder.HasFiles,
                    IsRoot = folder.IsRoot,
                    FolderContentMetaDataScanned = folder.FolderContentMetaDataScanned,
                };
                LibraryFolders.Add(folderViewModel);
            }
            foreach(Image image in images)
            {
                //Bitmap imgBitmap = await ImageHelper.FormatImage(image.ImagePath);
                ImageViewModel imageViewModel = new() 
                { 
                    ImageId = image.ImageId,
                    ImagePath = await ImageHelper.FormatImage(image.ImagePath),
                    ImageTags = image.ImageTags,
                    ImageRating = image.ImageRating,
                    ImageFolderPath = image.ImageFolderPath,
                    ImageMetaDataScanned = image.ImageMetaDataScanned,
                    FolderId = image.FolderId,
                };
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
        private async void GetAllFolders()
        {
            List<Folder> allFolders = await _folderMethods.GetAllFolders();
            foreach (Folder folder in allFolders) 
            {
                FolderViewModel folderViewModel = new() 
                { 
                    FolderId = folder.FolderId,
                    FolderName = folder.FolderName,
                    FolderPath = folder.FolderPath,
                    HasChildren = folder.HasChildren,
                    CoverImagePath = folder.CoverImagePath == "" ? ImageHelper.LoadFromResource(new Uri("avares://ImagePerfect/Assets/icons8-folder-600.png")) : await ImageHelper.FormatImage(folder.CoverImagePath),
                    FolderDescription = folder.FolderDescription,
                    FolderTags = folder.FolderTags,
                    FolderRating = folder.FolderRating,
                    HasFiles = folder.HasFiles,
                    IsRoot = folder.IsRoot,
                    FolderContentMetaDataScanned = folder.FolderContentMetaDataScanned,
                };
                LibraryFolders.Add(folderViewModel);
            }
        }
    }
}

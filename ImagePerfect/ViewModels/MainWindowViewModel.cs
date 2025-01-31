using ImagePerfect.Helpers;
using ImagePerfect.Models;
using ImagePerfect.Repository.IRepository;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Reactive;

namespace ImagePerfect.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly FolderMethods _folderMethods;

        public MainWindowViewModel() { }
        public MainWindowViewModel(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _folderMethods = new FolderMethods(_unitOfWork);

            NextFolderCommand = ReactiveCommand.Create((FolderViewModel currentFolder) => {
                NextFolder(currentFolder);
            });
            GetRootFolder();
        }
        public PickRootFolderViewModel PickRootFolder { get => new PickRootFolderViewModel(_unitOfWork); }

        public ObservableCollection<FolderViewModel> LibraryFolders { get; } = new ObservableCollection<FolderViewModel>();

        public ReactiveCommand<FolderViewModel, Unit> NextFolderCommand { get; }
        private async void GetRootFolder()
        {
            Folder rootFolder = await _folderMethods.GetRootFolder();
            FolderViewModel rootFolderVm = new() 
            {
                FolderId = rootFolder.FolderId,
                FolderName = rootFolder.FolderName,
                FolderPath = rootFolder.FolderPath,
                HasChildren = rootFolder.HasChildren,
                CoverImagePath = rootFolder.CoverImagePath == "" ? ImageHelper.LoadFromResource(new Uri("avares://ImagePerfect/Assets/icons8-folder-600.png")) : ImageHelper.LoadFromFileSystem(rootFolder.CoverImagePath),
                FolderDescription = rootFolder.FolderDescription,
                FolderTags = rootFolder.FolderTags,
                FolderRating = rootFolder.FolderRating,
                HasFiles = rootFolder.HasFiles,
                IsRoot = rootFolder.IsRoot,
                FolderContentMetaDataScanned = rootFolder.FolderContentMetaDataScanned,
            };
            LibraryFolders.Add(rootFolderVm);
        }

        private void NextFolder(FolderViewModel currentFolder)
        {
            Debug.WriteLine("Get next folder");
            Debug.WriteLine(currentFolder.FolderPath);
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
                    CoverImagePath = folder.CoverImagePath == "" ? ImageHelper.LoadFromResource(new Uri("avares://ImagePerfect/Assets/icons8-folder-600.png")) : ImageHelper.LoadFromFileSystem(folder.CoverImagePath),
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

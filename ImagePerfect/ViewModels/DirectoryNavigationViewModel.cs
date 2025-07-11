using System;
using System.Collections.Generic;
using ImagePerfect.Helpers;
using MsBox.Avalonia.Enums;
using MsBox.Avalonia;
using ReactiveUI;

namespace ImagePerfect.ViewModels
{
	public class DirectoryNavigationViewModel : ViewModelBase
	{
        private readonly MainWindowViewModel _mainWindowViewModel;
        public DirectoryNavigationViewModel(MainWindowViewModel mainWindowViewModel) 
		{
            _mainWindowViewModel = mainWindowViewModel;
        }
        //Reason for all 3 methods
        //All three BackFolder methods can just be reduced to BackFolderFromDirectoryOptionsPanel()
        //However the other two methods are useful after applying filters.
        //Especially with large libraries whe you apply a filter it pulls folders or images from many places.
        //Thus,it would be nice to hit BACK on the image/folder and actully load the images/folders containing folder; not just go back from current directory.
        //This can aid the user in finding just where that image/folder is located in the library

        //method opens the previous directory location
        public async void BackFolderFromDirectoryOptionsPanel()
        {
            if (_mainWindowViewModel.CurrentDirectory == _mainWindowViewModel.InitializeVm.RootFolderLocation)
            {
                return;
            }
            //not ideal but keeps pagination to the folder your in. When you go back or next start from page 1
            _mainWindowViewModel.ResetPagination();

            string newPath = PathHelper.RemoveOneFolderFromPath(_mainWindowViewModel.CurrentDirectory);
            //set the current directory -- used to add new folder to location
            _mainWindowViewModel.CurrentDirectory = newPath;
            //refresh UI
            _mainWindowViewModel.currentFilter = MainWindowViewModel.Filters.None;
            await _mainWindowViewModel.RefreshFolders();
            await _mainWindowViewModel.RefreshImages(newPath);
        }

        //opens the previous directory location -- from image button
        public async void BackFolderFromImage(ImageViewModel imageVm)
        {
            //not ideal but keeps pagination to the folder your in. When you go back or next start from page 1
            _mainWindowViewModel.ResetPagination();
            /*
                Similar to Back folders except these buttons are on the image and we only need to remove one folder
                Not every folder has a folder so this is the quickest way for now to back out of a folder that only has images
             */
            string newPath = PathHelper.RemoveOneFolderFromPath(imageVm.ImageFolderPath);
            //set the current directory -- used to add new folder to location
            _mainWindowViewModel.CurrentDirectory = newPath;
            //refresh UI
            _mainWindowViewModel.currentFilter = MainWindowViewModel.Filters.None;
            await _mainWindowViewModel.RefreshFolders();
            await _mainWindowViewModel.RefreshImages(newPath);
        }

        //opens the previous directory location -- from folder button
        public async void BackFolder(FolderViewModel currentFolder)
        {
            _mainWindowViewModel.ResetPagination();
            /*
                tough to see but basically you need to remove two folders to build the regexp string
                example if you are in /pictures/hiking/bearmountian and bearmountain folder has another folder saturday_2025_05_25
                you will be clicking on the back button of folder /pictures/hiking/bearmountian/saturday_2025_05_25 -- that wil be the FolderPath
                but you want to go back to hiking so you must remove two folders to get /pictures/hiking/
             */
            string newPath = PathHelper.RemoveTwoFoldersFromPath(currentFolder.FolderPath);
            //set the current directory -- used to add new folder to location
            _mainWindowViewModel.CurrentDirectory = newPath;
            //refresh UI
            _mainWindowViewModel.currentFilter = MainWindowViewModel.Filters.None;
            await _mainWindowViewModel.RefreshFolders();
            await _mainWindowViewModel.RefreshImages(newPath);
        }

        //opens the next directory location
        public async void NextFolder(FolderViewModel currentFolder)
        {
            _mainWindowViewModel.ResetPagination();
            bool hasChildren = currentFolder.HasChildren;
            bool hasFiles = currentFolder.HasFiles;
            //set the current directory -- used to add new folder to location
            _mainWindowViewModel.CurrentDirectory = currentFolder.FolderPath;
            //two boolean varibale 4 combos TF TT FT and FF
            if (hasChildren == false && hasFiles == false)
            {
                var box = MessageBoxManager.GetMessageBoxStandard("Empty Folder", "There are no Images in this folder.", ButtonEnum.Ok);
                await box.ShowAsync();
                _mainWindowViewModel.CurrentDirectory = PathHelper.RemoveOneFolderFromPath(currentFolder.FolderPath);
                return;
            }
            else
            {
                //refresh UI
                _mainWindowViewModel.currentFilter = MainWindowViewModel.Filters.None;
                await _mainWindowViewModel.RefreshFolders();
                await _mainWindowViewModel.RefreshImages("", currentFolder.FolderId);
            }
        }
    }
}
using System;
using System.Collections.Generic;
using System.Diagnostics;
using ReactiveUI;

//sub-ViewModel has no VIEW it is just to help factor out MainWindowViewModel
namespace ImagePerfect.ViewModels
{
	public class ToggleUIViewModel : ViewModelBase
	{
        private bool _showAllTags = false;
        private bool _showImportAndScan = false;
        private bool _showFolderFilters = false;
        private bool _showImageFilters = false;
        private bool _showSettings = false;
        private bool _showTotalImages = false;
        private bool _showCreateNewFolder = false;
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
        public bool ShowFolderFilters
        {
            get => _showFolderFilters;
            set => this.RaiseAndSetIfChanged(ref _showFolderFilters, value);
        }
        public bool ShowImageFilters
        {
            get => _showImageFilters;
            set => this.RaiseAndSetIfChanged(ref _showImageFilters, value);
        }
        public bool ShowSettings
        {
            get => _showSettings;
            set => this.RaiseAndSetIfChanged(ref _showSettings, value);
        }
        public bool ShowTotalImages
        {
            get => _showTotalImages;
            set => this.RaiseAndSetIfChanged(ref _showTotalImages, value);
        }
        public bool ShowCreateNewFolder
        {
            get => _showCreateNewFolder;
            set => this.RaiseAndSetIfChanged(ref _showCreateNewFolder, value);
        }

        public void ToggleListAllTags()
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

        public void ToggleImportAndScan()
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

        public void ToggleFilters(string showFilter)
        {
            if (showFilter == "FolderFilters")
            {
                if (ShowFolderFilters)
                {
                    ShowFolderFilters = false;
                }
                else
                {
                    ShowFolderFilters = true;
                }
            }
            if (showFilter == "ImageFilters")
            {
                if (ShowImageFilters)
                {
                    ShowImageFilters = false;
                }
                else
                {
                    ShowImageFilters = true;
                }
            }
        }
        public void ToggleSettings()
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

        public void ToggleGetTotalImages()
        {
            if (ShowTotalImages)
            {
                ShowTotalImages = false;
            }
            else
            {
                ShowTotalImages = true;
            }
        }
        public void ToggleCreateNewFolder()
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


    }
}
using System;
using System.Collections.Generic;
using Avalonia.Controls;
using System.Threading.Tasks;
using ImagePerfect.Models;
using ImagePerfect.Repository.IRepository;
using ReactiveUI;
using Avalonia;

namespace ImagePerfect.ViewModels
{
    //see InitializeViewModel as that class initally sets up the SavedDirectory
    public class SavedDirectoryViewModel : ViewModelBase
	{
        private readonly IUnitOfWork _unitOfWork;
        private readonly SaveDirectoryMethods _saveDirectoryMethods;
        private readonly MainWindowViewModel _mainWindowViewModel;

        public SavedDirectoryViewModel(IUnitOfWork unitOfWork, MainWindowViewModel mainWindowViewModel)
        {
            _unitOfWork = unitOfWork;
            _mainWindowViewModel = mainWindowViewModel;
            _saveDirectoryMethods = new SaveDirectoryMethods(_unitOfWork);
        }

        public async Task SaveDirectory(ScrollViewer scrollViewer)
        {
            //update variables
            _mainWindowViewModel.SavedDirectory = _mainWindowViewModel.CurrentDirectory;
            _mainWindowViewModel.SavedFolderPage = _mainWindowViewModel.CurrentFolderPage;
            _mainWindowViewModel.SavedTotalFolderPages = _mainWindowViewModel.TotalFolderPages;
            _mainWindowViewModel.SavedImagePage = _mainWindowViewModel.CurrentImagePage;
            _mainWindowViewModel.SavedTotalImagePages = _mainWindowViewModel.TotalImagePages;
            double XVector = scrollViewer.Offset.X;
            double YVector = scrollViewer.Offset.Y;
            _mainWindowViewModel.SavedOffsetVector = new Vector(XVector, YVector);
            //persist to database
            SaveDirectory saveDirectory = new()
            {
                SavedDirectoryId = 1,
                SavedDirectory = _mainWindowViewModel.CurrentDirectory,
                SavedFolderPage = _mainWindowViewModel.CurrentFolderPage,
                SavedTotalFolderPages = _mainWindowViewModel.TotalFolderPages,
                SavedImagePage = _mainWindowViewModel.CurrentImagePage,
                SavedTotalImagePages = _mainWindowViewModel.TotalImagePages,
                XVector = scrollViewer.Offset.X,
                YVector = scrollViewer.Offset.Y
            };
            await _saveDirectoryMethods.UpdateSaveDirectory(saveDirectory);
        }

        public async Task LoadSavedDirectory(ScrollViewer scrollViewer)
        {
            _mainWindowViewModel.CurrentDirectory = _mainWindowViewModel.SavedDirectory;
            _mainWindowViewModel.CurrentFolderPage = _mainWindowViewModel.SavedFolderPage;
            _mainWindowViewModel.TotalFolderPages = _mainWindowViewModel.SavedTotalFolderPages;
            _mainWindowViewModel.CurrentImagePage = _mainWindowViewModel.SavedImagePage;
            _mainWindowViewModel.TotalImagePages = _mainWindowViewModel.SavedTotalImagePages;
            _mainWindowViewModel.MaxPage = Math.Max(_mainWindowViewModel.TotalImagePages, _mainWindowViewModel.TotalFolderPages);
            _mainWindowViewModel.MaxCurrentPage = Math.Max(_mainWindowViewModel.CurrentImagePage, _mainWindowViewModel.CurrentFolderPage);
            await _mainWindowViewModel.LoadCurrentDirectory();
            scrollViewer.Offset = _mainWindowViewModel.SavedOffsetVector;
        }
    }
}
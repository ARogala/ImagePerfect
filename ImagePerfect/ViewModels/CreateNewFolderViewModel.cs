using System;
using MsBox.Avalonia.Enums;
using MsBox.Avalonia;
using System.IO;
using ImagePerfect.Repository.IRepository;
using ImagePerfect.Models;
using ImagePerfect.Helpers;
using System.Threading.Tasks;
using ReactiveUI;

namespace ImagePerfect.ViewModels
{
	public class CreateNewFolderViewModel : ViewModelBase
	{
        private string _newFolderName = string.Empty;
        private bool _isNewFolderEnabled;
        private readonly IUnitOfWork _unitOfWork;
        private readonly FolderMethods _folderMethods;
        private readonly MainWindowViewModel _mainWindowViewModel;
        public CreateNewFolderViewModel(IUnitOfWork unitOfWork, MainWindowViewModel mainWindowViewModel) 
        {
            _unitOfWork = unitOfWork;
            _mainWindowViewModel = mainWindowViewModel;
            _folderMethods = new FolderMethods(_unitOfWork);
        }

        public string NewFolderName
        {
            get => _newFolderName;
            set
            {
                this.RaiseAndSetIfChanged(ref _newFolderName, value);
                if (value == "" || _mainWindowViewModel.CurrentDirectory == _mainWindowViewModel.RootFolderLocation || _mainWindowViewModel.currentFilter != MainWindowViewModel.Filters.None)
                {
                    IsNewFolderEnabled = false;
                }
                else
                {
                    IsNewFolderEnabled = true;
                }
            }
        }

        public bool IsNewFolderEnabled
        {
            get => _isNewFolderEnabled;
            set => this.RaiseAndSetIfChanged(ref _isNewFolderEnabled, value);
        }

        public async Task CreateNewFolder()
		{
            //first check if directory exists
            string newFolderPath = PathHelper.GetNewFolderPath(_mainWindowViewModel.CurrentDirectory, NewFolderName);
            if (Directory.Exists(newFolderPath))
            {
                var box = MessageBoxManager.GetMessageBoxStandard("New Folder", "A folder with this name already exists.", ButtonEnum.Ok);
                await box.ShowAsync();
                return;
            }
            //add dir to database -- also need to update parent folders HasChildren bool value
            Folder newFolder = new Folder
            {
                FolderName = NewFolderName,
                FolderPath = newFolderPath,
                HasChildren = false,
                CoverImagePath = "",
                FolderDescription = "",
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
                    _mainWindowViewModel.currentFilter = MainWindowViewModel.Filters.None;
                    await _mainWindowViewModel.RefreshFolders();
                }
                catch (Exception e)
                {
                    var box = MessageBoxManager.GetMessageBoxStandard("New Folder", $"Error {e}.", ButtonEnum.Ok);
                    await box.ShowAsync();
                    return;
                }
            }
        }
	}
}
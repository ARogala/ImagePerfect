using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ImagePerfect.Models;
using ImagePerfect.Repository.IRepository;
using ReactiveUI;

namespace ImagePerfect.ViewModels
{
	public class SettingsViewModel : ViewModelBase
	{
        //FolderPageSize and ImagePageSize are needed for pagination as well.
        //image width max of 600 min of 300
        private int _maxImageWidth = 600;
        private int _folderPageSize = 20;
        private int _imagePageSize = 20;

        private readonly IUnitOfWork _unitOfWork;
        
        private readonly SettingsMethods _settingsMethods;
        private readonly MainWindowViewModel _mainWindowViewModel;
        public SettingsViewModel(IUnitOfWork unitOfWork, MainWindowViewModel mainWindowViewModel) 
		{
            _unitOfWork = unitOfWork;
            _mainWindowViewModel = mainWindowViewModel;
            _settingsMethods = new SettingsMethods(_unitOfWork);
        }

        public int ImagePageSize
        {
            get => _imagePageSize;
            set => this.RaiseAndSetIfChanged(ref _imagePageSize, value);
        }
        public int FolderPageSize
        {
            get => _folderPageSize;
            set => this.RaiseAndSetIfChanged(ref _folderPageSize, value);
        }
        public int MaxImageWidth
        {
            get => _maxImageWidth;
            set => this.RaiseAndSetIfChanged(ref _maxImageWidth, value);
        }

        private async Task UpdateSettings()
        {
            //update database
            Settings settings = new()
            {
                SettingsId = 1,
                MaxImageWidth = MaxImageWidth,
                FolderPageSize = FolderPageSize,
                ImagePageSize = ImagePageSize,
            };
            await _settingsMethods.UpdateSettings(settings);
        }

        public async Task GetSettings()
        {
            Settings settings = await _settingsMethods.GetSettings();
            MaxImageWidth = settings.MaxImageWidth;
            FolderPageSize = settings.FolderPageSize;
            ImagePageSize = settings.ImagePageSize;
        }

        public async Task PickImagePageSize(string size)
        {
            switch (size)
            {
                case "20":
                    ImagePageSize = 20;
                    break;
                case "40":
                    ImagePageSize = 40;
                    break;
                case "60":
                    ImagePageSize = 60;
                    break;
                case "80":
                    ImagePageSize = 80;
                    break;
                case "100":
                    ImagePageSize = 100;
                    break;
                case "125":
                    ImagePageSize = 125;
                    break;
                case "150":
                    ImagePageSize = 150;
                    break;
                case "175":
                    ImagePageSize = 175;
                    break;
                case "200":
                    ImagePageSize = 200;
                    break;
            }
            await UpdateSettings();
            _mainWindowViewModel.ResetPagination();
            await _mainWindowViewModel.RefreshFolders();
            await _mainWindowViewModel.RefreshImages(_mainWindowViewModel.CurrentDirectory);
        }
        public async Task PickFolderPageSize(string size)
        {
            switch (size)
            {
                case "20":
                    FolderPageSize = 20;
                    break;
                case "40":
                    FolderPageSize = 40;
                    break;
                case "60":
                    FolderPageSize = 60;
                    break;
                case "80":
                    FolderPageSize = 80;
                    break;
                case "100":
                    FolderPageSize = 100;
                    break;
                case "125":
                    FolderPageSize = 125;
                    break;
                case "150":
                    FolderPageSize = 150;
                    break;
                case "175":
                    FolderPageSize = 175;
                    break;
                case "200":
                    FolderPageSize = 200;
                    break;
            }
            await UpdateSettings();
            _mainWindowViewModel.ResetPagination();
            await _mainWindowViewModel.RefreshFolders();
            await _mainWindowViewModel.RefreshImages(_mainWindowViewModel.CurrentDirectory);
        }
        public async Task SelectImageWidth(decimal size)
        {
            MaxImageWidth = (int)size;
            await UpdateSettings();
        }
        public async Task PickImageWidth(string size)
        {
            switch (size)
            {
                case "Small":
                    MaxImageWidth = 300;
                    break;
                case "Medium":
                    MaxImageWidth = 400;
                    break;
                case "Large":
                    MaxImageWidth = 500;
                    break;
                case "XLarge":
                    MaxImageWidth = 550;
                    break;
                case "XXLarge":
                    MaxImageWidth = 600;
                    break;
            }
            await UpdateSettings();
        }
	}
}
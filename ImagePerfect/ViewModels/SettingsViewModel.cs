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
        private readonly IUnitOfWork _unitOfWork;
        
        private readonly SettingsMethods _settingsMethods;
        private readonly MainWindowViewModel _mainWindowViewModel;
        public SettingsViewModel(IUnitOfWork unitOfWork, MainWindowViewModel mainWindowViewModel) 
		{
            _unitOfWork = unitOfWork;
            _mainWindowViewModel = mainWindowViewModel;
            _settingsMethods = new SettingsMethods(_unitOfWork);
        }

        private async Task UpdateSettings()
        {
            //update database
            Settings settings = new()
            {
                SettingsId = 1,
                MaxImageWidth = _mainWindowViewModel.MaxImageWidth,
                FolderPageSize = _mainWindowViewModel.FolderPageSize,
                ImagePageSize = _mainWindowViewModel.ImagePageSize,
            };
            await _settingsMethods.UpdateSettings(settings);
        }

        public async Task GetSettings()
        {
            Settings settings = await _settingsMethods.GetSettings();
            _mainWindowViewModel.MaxImageWidth = settings.MaxImageWidth;
            _mainWindowViewModel.FolderPageSize = settings.FolderPageSize;
            _mainWindowViewModel.ImagePageSize = settings.ImagePageSize;
        }

        public async Task PickImagePageSize(string size)
        {
            switch (size)
            {
                case "20":
                    _mainWindowViewModel.ImagePageSize = 20;
                    break;
                case "40":
                    _mainWindowViewModel.ImagePageSize = 40;
                    break;
                case "60":
                    _mainWindowViewModel.ImagePageSize = 60;
                    break;
                case "80":
                    _mainWindowViewModel.ImagePageSize = 80;
                    break;
                case "100":
                    _mainWindowViewModel.ImagePageSize = 100;
                    break;
                case "125":
                    _mainWindowViewModel.ImagePageSize = 125;
                    break;
                case "150":
                    _mainWindowViewModel.ImagePageSize = 150;
                    break;
                case "175":
                    _mainWindowViewModel.ImagePageSize = 175;
                    break;
                case "200":
                    _mainWindowViewModel.ImagePageSize = 200;
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
                    _mainWindowViewModel.FolderPageSize = 20;
                    break;
                case "40":
                    _mainWindowViewModel.FolderPageSize = 40;
                    break;
                case "60":
                    _mainWindowViewModel.FolderPageSize = 60;
                    break;
                case "80":
                    _mainWindowViewModel.FolderPageSize = 80;
                    break;
                case "100":
                    _mainWindowViewModel.FolderPageSize = 100;
                    break;
            }
            await UpdateSettings();
            _mainWindowViewModel.ResetPagination();
            await _mainWindowViewModel.RefreshFolders();
            await _mainWindowViewModel.RefreshImages(_mainWindowViewModel.CurrentDirectory);
        }
        public async Task SelectImageWidth(decimal size)
        {
            _mainWindowViewModel.MaxImageWidth = (int)size;
            await UpdateSettings();
        }
        public async Task PickImageWidth(string size)
        {
            switch (size)
            {
                case "Small":
                    _mainWindowViewModel.MaxImageWidth = 300;
                    break;
                case "Medium":
                    _mainWindowViewModel.MaxImageWidth = 400;
                    break;
                case "Large":
                    _mainWindowViewModel.MaxImageWidth = 500;
                    break;
                case "XLarge":
                    _mainWindowViewModel.MaxImageWidth = 550;
                    break;
                case "XXLarge":
                    _mainWindowViewModel.MaxImageWidth = 600;
                    break;
            }
            await UpdateSettings();
        }
	}
}
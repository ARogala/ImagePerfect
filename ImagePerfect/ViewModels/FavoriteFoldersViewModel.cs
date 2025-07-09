using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ImagePerfect.Models;
using ImagePerfect.Repository.IRepository;
using MsBox.Avalonia.Enums;
using MsBox.Avalonia;
using ReactiveUI;

namespace ImagePerfect.ViewModels
{
	public class FavoriteFoldersViewModel : ViewModelBase
	{
        private readonly IUnitOfWork _unitOfWork;
        private readonly FolderMethods _folderMethods;

        public FavoriteFoldersViewModel(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _folderMethods = new FolderMethods(_unitOfWork);
        }

        public async Task SaveFolderAsFavorite(FolderViewModel folderVm)
        {
            await _folderMethods.SaveFolderToFavorites(folderVm.FolderId);
        }

        public async Task RemoveAllFavoriteFolders()
        {
            var box = MessageBoxManager.GetMessageBoxStandard("Remove Favorite Folders", "Are you sure you want to remove your favorite folders from the data base? The folders on the file system will remain.", ButtonEnum.YesNo);
            var result = await box.ShowAsync();
            if (result == ButtonResult.Yes)
            {
                await _folderMethods.RemoveAllFavoriteFolders();
            }
            else
            {
                return;
            }
        }
    }
}
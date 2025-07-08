using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using ImagePerfect.Repository.IRepository;
using MsBox.Avalonia.Enums;
using MsBox.Avalonia;
using ReactiveUI;
using ImagePerfect.Models;
using ImagePerfect.Helpers;

namespace ImagePerfect.ViewModels
{
	public class MoveImagesViewModel : ViewModelBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly FolderMethods _folderMethods;
        private readonly ImageMethods _imageMethods;
        private readonly MainWindowViewModel _mainWindowViewModel;
        public MoveImagesViewModel(IUnitOfWork unitOfWork, MainWindowViewModel mainWindowViewModel) 
		{
            _unitOfWork = unitOfWork;
            _mainWindowViewModel = mainWindowViewModel;
            _folderMethods = new FolderMethods(_unitOfWork);
            _imageMethods = new ImageMethods(_unitOfWork);
        }

        public async Task MoveImageToTrash(ImageViewModel imageVm) 
        {
            var boxYesNo = MessageBoxManager.GetMessageBoxStandard("Delete Image", "Are you sure you want to delete your image?", ButtonEnum.YesNo);
            var boxResult = await boxYesNo.ShowAsync();
            if (boxResult == ButtonResult.Yes)
            {
                _mainWindowViewModel.ShowLoading = true;
                (List<Folder> folders, List<FolderTag> tags) folderResult = await _folderMethods.GetFoldersInDirectory(imageVm.ImageFolderPath);
                _mainWindowViewModel.displayFolders = folderResult.folders;
                (List<Image> images, List<ImageTag> tags) imageResultA = await _imageMethods.GetAllImagesInFolder(imageVm.FolderId);
                _mainWindowViewModel.displayImages = imageResultA.images;
                if (_mainWindowViewModel.displayImages.Count == 1 && _mainWindowViewModel.displayFolders.Count == 0)
                {
                    var box = MessageBoxManager.GetMessageBoxStandard("Delete Image", "This is the last image in the folder go back and delete the folder", ButtonEnum.Ok);
                    await box.ShowAsync();
                    return;
                }
                Folder? rootFolder = await _folderMethods.GetRootFolder();
                string trashFolderPath = PathHelper.GetTrashFolderPath(rootFolder.FolderPath);

                //create ImagePerfectTRASH if it doesnt exist
                if (!Directory.Exists(trashFolderPath))
                {
                    Directory.CreateDirectory(trashFolderPath);
                }
                if (File.Exists(imageVm.ImagePath))
                {
                    //delete image from db
                    bool success = await _imageMethods.DeleteImage(imageVm.ImageId);

                    if (success)
                    {
                        //move file to trash folder
                        string newImagePath = PathHelper.GetImageFileTrashPath(imageVm, trashFolderPath);
                        File.Move(imageVm.ImagePath, newImagePath);

                        //refresh UI
                        await _mainWindowViewModel.RefreshImages("", imageVm.FolderId);
                        _mainWindowViewModel.ShowLoading = false;
                    }
                }
                _mainWindowViewModel.ShowLoading = false;
            }
        }
	}
}
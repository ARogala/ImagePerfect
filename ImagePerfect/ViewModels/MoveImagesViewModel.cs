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
using Image = ImagePerfect.Models.Image;
using Avalonia.Controls;
using System.Linq;

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

        public async Task MoveSelectedImagesToTrash(ItemsControl imagesItemsControl)
        {
            List<ImageViewModel> allImages = imagesItemsControl.Items.OfType<ImageViewModel>().ToList();
            List<ImageViewModel> imagesToDelete = new List<ImageViewModel>();
            foreach (ImageViewModel image in allImages)
            {
                if (image.IsSelected && File.Exists(image.ImagePath))
                {
                    imagesToDelete.Add(image);
                }
            }
            if (imagesToDelete.Count == 0)
            {
                var box = MessageBoxManager.GetMessageBoxStandard("Delete Images", "You need to select images to delete.", ButtonEnum.Ok);
                await box.ShowAsync();
                return;
            }
            var boxYesNo = MessageBoxManager.GetMessageBoxStandard("Delete Images", "Are you sure you want to delete these images?", ButtonEnum.YesNo);
            var boxResult = await boxYesNo.ShowAsync();
            if (boxResult == ButtonResult.Yes)
            {
                _mainWindowViewModel.ShowLoading = true;
                Folder imagesFolder = await _folderMethods.GetFolderAtDirectory(allImages[0].ImageFolderPath);
                Folder? rootFolder = await _folderMethods.GetRootFolder();
                string trashFolderPath = PathHelper.GetTrashFolderPath(rootFolder.FolderPath);

                //create ImagePerfectTRASH if it doesnt exist
                if (!Directory.Exists(trashFolderPath))
                {
                    Directory.CreateDirectory(trashFolderPath);
                }

                string sql = SqlStringBuilder.BuildSqlForMoveImagesToTrash(imagesToDelete);
                bool success = await _imageMethods.DeleteSelectedImages(sql);
                if (success)
                {
                    foreach (ImageViewModel image in imagesToDelete)
                    {
                        //move file to trash folder
                        string newImagePath = PathHelper.GetImageFileTrashPath(image, trashFolderPath);
                        File.Move(image.ImagePath, newImagePath);
                    }
                    //update imagesFolder HasFiles, AreImagesImported, and FolderContentMetaDataScanned
                    //set all back to false if moved all images to trash
                    IEnumerable<string> folderFiles = Directory.EnumerateFiles(imagesFolder.FolderPath).Where(s => s.ToLower().EndsWith(".jpeg") || s.ToLower().EndsWith(".jpg") || s.ToLower().EndsWith(".png") || s.ToLower().EndsWith(".gif"));
                    if (!folderFiles.Any())
                    {
                        imagesFolder.HasFiles = false;
                        imagesFolder.AreImagesImported = false;
                        imagesFolder.FolderContentMetaDataScanned = false;
                        await _folderMethods.UpdateFolder(imagesFolder);
                    }
                    //refresh UI
                    await _mainWindowViewModel.RefreshImages("", allImages[0].FolderId);
                    _mainWindowViewModel.ShowLoading = false;
                }
                _mainWindowViewModel.ShowLoading = false;
            }
        }
	}
}
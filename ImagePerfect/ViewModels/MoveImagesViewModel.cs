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
using System.Collections;
using System.Diagnostics;

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
                        if(!_mainWindowViewModel.SuppressImageRefresh)
                            await _mainWindowViewModel.RefreshImages("", imageVm.FolderId);
                        _mainWindowViewModel.ShowLoading = false;
                    }
                }
                _mainWindowViewModel.ShowLoading = false;
            }
        }

        public async Task MoveSelectedImagesToTrash(IList selectedImages)
        {
            List<ImageViewModel> imagesToDelete = selectedImages.OfType<ImageViewModel>().ToList();

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
                Folder imagesFolder = await _folderMethods.GetFolderAtDirectory(imagesToDelete[0].ImageFolderPath);
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
                    if (!_mainWindowViewModel.SuppressImageRefresh)
                        await _mainWindowViewModel.RefreshImages("", imagesToDelete[0].FolderId);
                    _mainWindowViewModel.ShowLoading = false;
                }
                _mainWindowViewModel.ShowLoading = false;
            }
        }

        public async Task MoveSelectedImagesToNewFolder(IList selectedImages)
        {
            List<ImageViewModel> imagesToMove = selectedImages.OfType<ImageViewModel>().ToList();
            if (imagesToMove.Count == 0)
                return;
            
            Folder imagesCurrentFolder = await _folderMethods.GetFolderAtDirectory(imagesToMove[0].ImageFolderPath);

            //get folder at SelectedImagesNewDirectory
            Folder imagesNewFolder = await _folderMethods.GetFolderAtDirectory(_mainWindowViewModel.SelectedImagesNewDirectory);
            if (imagesNewFolder.FolderPath == imagesCurrentFolder.FolderPath)
            {
                var box = MessageBoxManager.GetMessageBoxStandard("Move Images", "New folder path cannot be the same as the current folder path.", ButtonEnum.Ok);
                await box.ShowAsync();
                return;
            }
            //prevent a double import and only allow move to folders that are already imported
            if (imagesNewFolder.HasFiles == true && imagesNewFolder.AreImagesImported == false)
            {
                var box = MessageBoxManager.GetMessageBoxStandard("Move Images", "The move to folder has to have its current images imported first.", ButtonEnum.Ok);
                await box.ShowAsync();
                return;
            }

            var boxYesNo = MessageBoxManager.GetMessageBoxStandard("Move Images", $"Are you sure you want to move these images to {_mainWindowViewModel.SelectedImagesNewDirectory}?", ButtonEnum.YesNo);
            var boxResult = await boxYesNo.ShowAsync();
            if (boxResult == ButtonResult.Yes)
            {
                _mainWindowViewModel.ShowLoading = true;
                //modify ImagePath, ImageFolderPath and FolderId for each image in imagesToMove 
                List<ImageViewModel> imagesToMoveModifiedPaths = PathHelper.ModifyImagePathsForMoveImagesToNewFolder(imagesToMove, imagesNewFolder);
                
                //get image move sql
                string imageMoveSql = SqlStringBuilder.BuildSqlForMoveImagesToNewFolder(imagesToMoveModifiedPaths);
                //move images in db
                bool success = await _imageMethods.MoveSelectedImageToNewFolder(imageMoveSql);
                //move images on disk
                if (success)
                {
                    for (int i = 0; i < imagesToMoveModifiedPaths.Count; i++)
                    {
                        File.Move(imagesToMove[i].ImagePath, imagesToMoveModifiedPaths[i].ImagePath);
                    }
                    //after adding new images to a folder make sure the user is alerted to rescann them for metadata 
                    imagesNewFolder.FolderContentMetaDataScanned = false;
                    await _folderMethods.UpdateFolder(imagesNewFolder);
                    //if new folder did not have images before it does now so set to true
                    if (imagesNewFolder.HasFiles == false)
                    {
                        imagesNewFolder.HasFiles = true;
                        imagesNewFolder.AreImagesImported = true;
                        await _folderMethods.UpdateFolder(imagesNewFolder);
                    }
                    //update imagesCurrentFolder HasFiles, AreImagesImported, and FolderContentMetaDataScanned
                    //set all back to false if moved all images to new folder
                    IEnumerable<string> folderFiles = Directory.EnumerateFiles(imagesCurrentFolder.FolderPath).Where(s => s.ToLower().EndsWith(".jpeg") || s.ToLower().EndsWith(".jpg") || s.ToLower().EndsWith(".png") || s.ToLower().EndsWith(".gif"));
                    if (!folderFiles.Any())
                    {
                        imagesCurrentFolder.HasFiles = false;
                        imagesCurrentFolder.AreImagesImported = false;
                        imagesCurrentFolder.FolderContentMetaDataScanned = false;
                        await _folderMethods.UpdateFolder(imagesCurrentFolder);
                    }
                    //reset SelectedImageNewDirectory
                    _mainWindowViewModel.SelectedImagesNewDirectory = string.Empty;
                    //refresh UI
                    await _mainWindowViewModel.RefreshImages("", imagesToMove[0].FolderId);
                    await _mainWindowViewModel.RefreshFolders(imagesCurrentFolder.FolderPath);
                    _mainWindowViewModel.ShowLoading = false;
                }
                _mainWindowViewModel.ShowLoading = false;
            }
        }
	}
}
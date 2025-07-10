using System;
using System.Collections.Generic;
using Avalonia.Controls;
using Image = ImagePerfect.Models.Image;
using System.Threading.Tasks;
using ImagePerfect.Models;
using ImagePerfect.Repository.IRepository;
using ReactiveUI;
using System.Linq;
using ImagePerfect.ObjectMappers;
using MsBox.Avalonia.Enums;
using MsBox.Avalonia;
using ImagePerfect.Helpers;
using System.IO;

namespace ImagePerfect.ViewModels
{
	public class CoverImageViewModel : ViewModelBase
	{
        private readonly IUnitOfWork _unitOfWork;
        private readonly FolderMethods _folderMethods;
		private readonly ImageMethods _imageMethods;
        private readonly MainWindowViewModel _mainWindowViewModel;
        public CoverImageViewModel(IUnitOfWork unitOfWork, MainWindowViewModel mainWindowViewModel) 
		{
            _unitOfWork = unitOfWork;
            _mainWindowViewModel = mainWindowViewModel;
            _folderMethods = new FolderMethods(_unitOfWork);
            _imageMethods = new ImageMethods(_unitOfWork);
        }

        public async Task CopyCoverImageToContainingFolder(FolderViewModel folderVm)
        {
            if (PathHelper.RemoveOneFolderFromPath(folderVm.FolderPath) == _mainWindowViewModel.InitializeVm.RootFolderLocation)
            {
                var box = MessageBoxManager.GetMessageBoxStandard("Copy Cover", "Cannot copy from root folder.", ButtonEnum.Ok);
                await box.ShowAsync();
                return;
            }
            if (folderVm.CoverImagePath == "" || folderVm.CoverImagePath == null)
            {
                var box = MessageBoxManager.GetMessageBoxStandard("Copy Cover", "The folder must have a cover selected to copy.", ButtonEnum.Ok);
                await box.ShowAsync();
                return;
            }


            string coverImageCurrentPath = folderVm.CoverImagePath;
            string coverImageNewPath = PathHelper.GetCoverImagePathForCopyCoverImageToContainingFolder(folderVm);
            Folder containingFolder = await _folderMethods.GetFolderAtDirectory(PathHelper.RemoveOneFolderFromPath(folderVm.FolderPath));
            if (containingFolder.CoverImagePath != "")
            {
                var boxYesNo = MessageBoxManager.GetMessageBoxStandard("Copy Cover", "Containing folder already has a cover. Do you want to copy another?", ButtonEnum.YesNo);
                var boxResult = await boxYesNo.ShowAsync();
                if (boxResult == ButtonResult.No)
                {
                    return;
                }
            }
            if (File.Exists(coverImageNewPath))
            {
                var box = MessageBoxManager.GetMessageBoxStandard("Copy Cover", "A cover image in the destination has the same file name. Pick a different cover", ButtonEnum.Ok);
                await box.ShowAsync();
                return;
            }
            //add cover image path to containing folder
            bool success = await _folderMethods.UpdateCoverImage(coverImageNewPath, containingFolder.FolderId);
            //copy file in file system
            if (success)
            {
                File.Copy(coverImageCurrentPath, coverImageNewPath);
            }
        }

        public async Task AddCoverImageOnCurrentPage(ItemsControl foldersItemsControl)
        {
            List<FolderViewModel> allFolders = foldersItemsControl.Items.OfType<FolderViewModel>().ToList();
            Random random = new Random();
            foreach (FolderViewModel folder in allFolders)
            {
                if (folder.HasFiles == true && folder.AreImagesImported == true)
                {
                    (List<Image> images, List<ImageTag> tags) imageResult = await _imageMethods.GetAllImagesInFolder(folder.FolderId);
                    List<Image> images = imageResult.images;
                    int randomIndex = random.Next(0, images.Count - 1);
                    //set random fall back cover
                    string cover = images[randomIndex].ImagePath;
                    //get cover clean
                    foreach (Image image in images)
                    {
                        if (image.ImagePath.ToLower().Contains("cover") && image.ImagePath.ToLower().Contains("clean"))
                        {
                            cover = image.ImagePath;
                            break;
                        }
                    }
                    //if no cover clean get poster
                    if (!(cover.ToLower().Contains("cover") && cover.ToLower().Contains("clean")))
                    {
                        foreach (Image image in images)
                        {
                            if (image.ImagePath.ToLower().Contains("poster"))
                            {
                                cover = image.ImagePath;
                                break;
                            }
                        }
                    }
                    //if no poster or no cover clean get cover
                    if (!(cover.ToLower().Contains("poster") || (cover.ToLower().Contains("cover") && cover.ToLower().Contains("clean"))))
                    {
                        foreach (Image image in images)
                        {
                            if (image.ImagePath.ToLower().Contains("cover"))
                            {
                                cover = image.ImagePath;
                                break;
                            }
                        }
                    }
                    folder.CoverImagePath = cover;
                    await _folderMethods.UpdateFolder(FolderMapper.GetFolderFromVm(folder));
                }
            }
            await _mainWindowViewModel.RefreshFolders();
        }

    }
}
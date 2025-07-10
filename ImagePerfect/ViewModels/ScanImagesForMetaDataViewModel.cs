using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonia.Controls;
using Image = ImagePerfect.Models.Image;
using ImagePerfect.Helpers;
using ImagePerfect.Models;
using ImagePerfect.Repository.IRepository;
using MsBox.Avalonia.Enums;
using MsBox.Avalonia;
using ReactiveUI;
using System.Linq;

namespace ImagePerfect.ViewModels
{
	public class ScanImagesForMetaDataViewModel : ViewModelBase
	{
        private readonly IUnitOfWork _unitOfWork;
        private readonly ImageMethods _imageMethods;
        private readonly MainWindowViewModel _mainWindowViewModel;
        public ScanImagesForMetaDataViewModel(IUnitOfWork unitOfWork, MainWindowViewModel mainWindowViewModel) 
		{
            _unitOfWork = unitOfWork;
            _mainWindowViewModel = mainWindowViewModel;
            _imageMethods = new ImageMethods(_unitOfWork);
        }

        /*
         * complicated because tags are in image_tags_join table also the tags on image metadata may or may not be in the tags table in database
         * goal is to take metadata from image and write to database. The two should be identical after this point. 
         * With image metadata taking more importance because the app also writes tags and rating to image metadata -- so count that as the master record
         * 
         * Because ImageRating is on the images table and tags are on image_tags_join it is easy to update the ImageRating 
         * in one database trip but the tags are much more complicated because the tag metadata from the image itself will not have
         * the tagId needed for the database also these metadata tags may or may not be in the tags table.
         * 
         * thus for now the most efficient thing i could think to do was to update the ratings in one shot
         * then since not every image will even have a tag only update the ones that have tags -- least amout of db round trips
         * Also for the images that do have tags clear the image_tag_join table 1st so we dont double up on tags in the db. 
         * 
         * perfect heck no... But it works fine for a few hundred or maybe thousand images. 
         * Really how many images are going to be on one folder? I am assuming at most maybe a few thousand
         * 
         */
        public async Task ScanFolderImagesForMetaData(FolderViewModel folderVm)
        {
            _mainWindowViewModel.ShowLoading = true;
            //get all images at folder id
            (List<Image> images, List<ImageTag> tags) imageResultA = await _imageMethods.GetAllImagesInFolder(folderVm.FolderId);
            List<Image> images = imageResultA.images;
            //scan images for metadata
            List<Image> imagesPlusUpdatedMetaData = await ImageMetaDataHelper.ScanImagesForMetaData(images);
            string imageUpdateSql = SqlStringBuilder.BuildImageSqlForScanMetadata(imagesPlusUpdatedMetaData);
            bool success = await _imageMethods.UpdateImageRatingFromMetaData(imageUpdateSql, folderVm.FolderId);
            foreach (Image image in imagesPlusUpdatedMetaData)
            {
                if (image.Tags.Count > 0)
                {
                    //avoid duplicates
                    await _imageMethods.ClearImageTagsJoinForMetaData(image);
                    foreach (ImageTag tag in image.Tags)
                    {
                        await _imageMethods.UpdateImageTagFromMetaData(tag);
                    }
                }
            }
            //show data scanned success
            if (success)
            {
                //Update TagsList to show in UI AutoCompleteBox
                await _mainWindowViewModel.GetTagsList();
                //refresh UI
                if (_mainWindowViewModel.currentFilter == MainWindowViewModel.Filters.AllFoldersWithMetadataNotScanned || _mainWindowViewModel.currentFilter == MainWindowViewModel.Filters.AllFoldersWithNoImportedImages)
                {
                    //have to call hard refresh for these two cases as they will not be returned from the query to update props
                    await _mainWindowViewModel.RefreshFolders();
                }
                else
                {
                    await _mainWindowViewModel.RefreshFolderProps(_mainWindowViewModel.CurrentDirectory, folderVm);
                }
            }
            _mainWindowViewModel.ShowLoading = false;
        }

        public async Task ScanAllFoldersOnCurrentPage(ItemsControl foldersItemsControl)
        {
            var boxYesNo = MessageBoxManager.GetMessageBoxStandard("Scan All Folders", "CAUTION this could take a long time are you sure? Make sure to import images first.", ButtonEnum.YesNo);
            var boxResult = await boxYesNo.ShowAsync();
            if (boxResult == ButtonResult.Yes)
            {
                List<FolderViewModel> allFolders = foldersItemsControl.Items.OfType<FolderViewModel>().ToList();
                foreach (FolderViewModel folder in allFolders)
                {
                    if (folder.HasFiles == true && folder.AreImagesImported == true && folder.FolderContentMetaDataScanned == false)
                    {
                        await ScanFolderImagesForMetaData(folder);
                    }
                }
                _mainWindowViewModel.ResetPagination();
            }
        }

    }
}
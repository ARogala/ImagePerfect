using Avalonia.Controls;
using ImagePerfect.Helpers;
using ImagePerfect.Models;
using ImagePerfect.Repository.IRepository;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Image = ImagePerfect.Models.Image;

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
         *
         * then do the following
         * 1. get list of all distinct tags from the image meatadata
         *      a. if none return
         * 2. bulk insert all distinct tags into tags table -- IGNORE duplicates
         * 3. get all the tag id's
         * 4. build the image tags join List
         * 5. Clear image tags join in database -- do this in one shot  -- done so no duplicates when rescan
         * 6. Bulk insert image tags join from the built List in step 4
         * 
         * perfect heck no... But it works fine for a few hundred or maybe thousand images. 
         * Really how many images are going to be on one folder? I am assuming at most maybe a few thousand
         * 
         */
        public async Task ScanFolderImagesForMetaData(FolderViewModel folderVm, bool bulkScan)
        {
            if (bulkScan == false)
                _mainWindowViewModel.ShowLoading = true;
            //get all images at folder id
            (List<Image> images, List<ImageTag> tags) imageResultA = await _imageMethods.GetAllImagesInFolder(folderVm.FolderId);
            List<Image> images = imageResultA.images;
            //scan images for metadata
            List<Image> imagesPlusUpdatedMetaData = await ImageMetaDataHelper.ScanImagesForMetaData(images);

            bool success = await _imageMethods.UpdateImageTagsAndRatingFromMetaData(imagesPlusUpdatedMetaData, folderVm.FolderId);

            //show data scanned success
            if (success)
            {
                if (bulkScan == false)
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
            }
            if (bulkScan == false)
                _mainWindowViewModel.ShowLoading = false;
        }

        public async Task ScanAllFoldersOnCurrentPage(ItemsControl foldersItemsControl)
        {
            var boxYesNo = MessageBoxManager.GetMessageBoxStandard("Scan All Folders", "CAUTION this could take a long time are you sure? Make sure to import images first.", ButtonEnum.YesNo);
            var boxResult = await boxYesNo.ShowAsync();
            if (boxResult == ButtonResult.Yes)
            {
                _mainWindowViewModel.ShowLoading = true;
                List<FolderViewModel> allFolders = foldersItemsControl.Items.OfType<FolderViewModel>()
                    .Where(folder => folder.HasFiles == true && folder.AreImagesImported == true && folder.FolderContentMetaDataScanned == false)
                    .ToList();
                await Parallel.ForEachAsync(allFolders, new ParallelOptions { MaxDegreeOfParallelism = 3 }, async (folder, ct) =>
                {
                    await ScanFolderImagesForMetaData(folder, true);
                });

                _mainWindowViewModel.ResetPagination();
                //Update TagsList to show in UI AutoCompleteBox
                await _mainWindowViewModel.GetTagsList();
                await _mainWindowViewModel.RefreshFolders();
                _mainWindowViewModel.ShowLoading = false;
            }
        }
    }
}
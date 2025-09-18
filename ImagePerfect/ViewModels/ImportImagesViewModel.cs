using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonia.Controls;
using ImagePerfect.Helpers;
using ImagePerfect.Models;
using ImagePerfect.Repository.IRepository;
using MsBox.Avalonia.Enums;
using MsBox.Avalonia;
using ReactiveUI;
using System.Linq;

namespace ImagePerfect.ViewModels
{
	public class ImportImagesViewModel : ViewModelBase
	{
        private readonly IUnitOfWork _unitOfWork;
        private readonly MainWindowViewModel _mainWindowViewModel;
        private readonly ImageCsvMethods _imageCsvMethods;
        public ImportImagesViewModel(IUnitOfWork unitOfWork, MainWindowViewModel mainWindowViewModel) 
		{
            _unitOfWork = unitOfWork;
            _mainWindowViewModel = mainWindowViewModel;
            _imageCsvMethods = new ImageCsvMethods(_unitOfWork);
        }

        public async Task ImportImages(FolderViewModel imageFolder, bool bulkScan)
        {
            string newPath = string.Empty;
            string imageFolderPath = imageFolder.FolderPath;
            int imageFolderId = imageFolder.FolderId;
            if(bulkScan == false)
                _mainWindowViewModel.ShowLoading = true;
            //build csv
            bool csvIsSet = await ImageCsvMethods.BuildImageCsv(imageFolderPath, imageFolderId);
            //write csv to database and load folders and images at the location again
            //load again so the import button will go away
            if (csvIsSet)
            {
                await _imageCsvMethods.AddImageCsv(imageFolderId);
                if(bulkScan == false)
                {
                    //remove one folder from path
                    newPath = PathHelper.RemoveOneFolderFromPath(imageFolderPath);
                    //refresh UI
                    if (_mainWindowViewModel.currentFilter == MainWindowViewModel.Filters.AllFoldersWithMetadataNotScanned || _mainWindowViewModel.currentFilter == MainWindowViewModel.Filters.AllFoldersWithNoImportedImages)
                    {
                        //have to call hard refresh for these two cases as they will not be returned from the query to update props
                        await _mainWindowViewModel.RefreshFolders();
                    }
                    else
                    {
                        await _mainWindowViewModel.RefreshFolderProps(newPath, imageFolder);
                    }
                }
            }
            if(bulkScan == false)
                _mainWindowViewModel.ShowLoading = false;
        }

        public async Task ImportAllFoldersOnCurrentPage(ItemsControl foldersItemsControl)
        {
            var boxYesNo = MessageBoxManager.GetMessageBoxStandard("Import All Folders", "CAUTION this could take a long time are you sure?", ButtonEnum.YesNo);
            var boxResult = await boxYesNo.ShowAsync();
            if (boxResult == ButtonResult.Yes)
            {
                _mainWindowViewModel.ShowLoading = true;
                List<FolderViewModel> allFolders = foldersItemsControl.Items.OfType<FolderViewModel>().ToList();
                foreach (FolderViewModel folder in allFolders)
                {
                    if (folder.HasFiles == true && folder.AreImagesImported == false)
                    {
                        await ImportImages(folder, true);
                    }
                }
                _mainWindowViewModel.ResetPagination();
                await _mainWindowViewModel.RefreshFolders();
                _mainWindowViewModel.ShowLoading = false;
            }
        }
    }
}